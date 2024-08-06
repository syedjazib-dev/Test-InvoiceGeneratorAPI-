
using AutoMapper;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenrator.Model.Models;

namespace InvoiceGenrator
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ApplicationUser, UserResponseDTO>().ReverseMap();
            CreateMap<ApplicationUser, UserUpdateDTO>().ReverseMap();
            CreateMap<Customer, CustomerCreateDTO>().ReverseMap();
            CreateMap<Customer, CustomerResponseDTO>().ReverseMap();
            CreateMap<Customer, CustomerUpdateDTO>().ReverseMap();
            CreateMap<Approval, ApprovalCreateDTO>().ReverseMap();
            CreateMap<Approval, ApprovalResponseDTO>().ReverseMap();
            CreateMap<Approval, ApprovalUpdateDTO>().ReverseMap();
            CreateMap<Item, ItemCreateDTO>().ReverseMap();
            CreateMap<Item, ItemUpdateDTO>().ReverseMap();
            CreateMap<Item, ItemResponseDTO>().ReverseMap();
            CreateMap<Invoice, InvoiceCreateDTO>().ReverseMap();
            CreateMap<Invoice, InvoiceUpdateDTO>().ReverseMap();
            CreateMap<Invoice, InvoiceResponseDTO>().ReverseMap();
            CreateMap<InvoiceApproval, InvoiceApprovalResponseDTO>().ReverseMap();
            CreateMap<InvoiceItem, InvoiceItemResponseDTO>().ReverseMap();
        }
    }
}
