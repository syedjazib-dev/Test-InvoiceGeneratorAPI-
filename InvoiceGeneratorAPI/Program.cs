using Microsoft.EntityFrameworkCore;
using InvoiceGenerator.DataAccess.DbContext;
using InvoiceGenerator.DataAccess.DbInitialize;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.DataAccess.Repository;
using InvoiceGenerator.StaticData;
using DinkToPdf.Contracts;
using DinkToPdf;
using InvoiceGenerator.Utility.PDFService;
using InvoiceGenrator;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultSQLConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))

    );

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPDFService, PDFService>();



var Issuer = builder.Configuration.GetValue<string>("JTWSettings:Issuer");
var Audience = builder.Configuration.GetValue<string>("JTWSettings:Audience");
var SecretKey = builder.Configuration.GetValue<string>("JTWSettings:SecretKey");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidIssuer = Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey!)),
        ValidateIssuerSigningKey = true,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context => {
            context.Request.Cookies.TryGetValue(CookieKey.AccessToken, out var accessToken);
            if (accessToken != null)
                context.Token = accessToken;

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<IDbinitializer, DBInitializer>();
builder.Services.AddSingleton(typeof(IConverter), new STASynchronizedConverter(new PdfTools()));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var AllowedURL = builder.Configuration.GetValue<string>("AllowedURL");

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins(AllowedURL, "https://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
SeedDatabse();

app.MapControllers();

app.Run();


void SeedDatabse()
{
    using (var scop = app.Services.CreateScope())
    {
        var DBInitialiser = scop.ServiceProvider.GetRequiredService<IDbinitializer>();
        DBInitialiser.Initialize();
    }
}

