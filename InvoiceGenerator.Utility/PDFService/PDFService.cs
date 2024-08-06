
using CRM.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Utility.NumberToWord;
using InvoiceGenerator.StaticData.CompanyInfo;
using InvoiceGenerator.StaticData.Status;
using System;
using System.Net.Http;
using System.Reflection;
using InvoiceGenerator.StaticData.ImageData;

namespace InvoiceGenerator.Utility.PDFService
{
    public class PDFService : IPDFService
    {
        private readonly IConverter _converter;

        public PDFService(IConverter converter)
        {
            _converter = converter;
        }


        public byte[] GenerateApprovalPDF(ApprovalPDF approvalPDF)
        {
            // Create header 
            string headerURl = Path.Combine(Environment.CurrentDirectory,
                @"TempHtmlTemplate/Headers/" + "Approval-Header-" + Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddHHmmssffff") + approvalPDF.Approval.Id + ".html");
            var HeaderHtmlString = @$"<!DOCTYPE html>
<html>
<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        .w-full {{
            width: 100%;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        a {{
            text-decoration: none;
            color: #444142;
        }}

        .text-center {{
            text-align: center;
        }}

        .header {{
            margin: auto;
            border-collapse: collapse;
            border-bottom: 1px solid #b99a37;
        }}

        .logo-container {{
            padding-inline-end: 0px;
            width: 92px;
        }}

        .logo {{
            height: 5rem;
        }}

        .logo-text {{
            font-weight: 600;
            font-size: 2.75rem;
            line-height: 2.75rem;
            color: #B99A37;
        }}

        .logo-description {{
            font-weight: 600;
            font-size: 16px;
        }}

        .spacing {{
            display: inline-block;
            margin: 0;
             margin-left:0.06rem;
        }}

        .header-right {{
            width: 40%;
            padding: 2.5rem;
            padding-right: 0.5rem;
            color: #444142;
            text-align: right;
        }}

        .header-svg {{
            display: inline;
            margin-bottom: 0.25rem;
            padding-left: 0.375rem;
        }}

        /* Body Header */

        .body-header {{
            margin-top: 1.75rem;
            text-align: center;
            letter-spacing: 0.05em;
        }}

        .text-28 {{
            font-size: 31px;
        }}

        .text-xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .w-full {{
            width: 100%;
            margin-left: auto;
            margin-right: auto;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .font-medium {{
            font-weight: 500;
        }}

        .text-base {{
            font-size: 1.25rem;
            line-height: 1.75rem;
        }}

        /* Details */

        .details {{
            margin-top: 2rem;
            border-bottom: 2px solid #bbbbbb;
            padding-bottom: 1rem;
        }}

        .pb-6 {{
            margin-top: 2rem;
        }}

        .text-gray-500 {{
            color: rgb(107 114 128);
        }}

        .text-2xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .pb-2 {{
            padding-bottom: 0.5rem;
        }}

        .text-sm {{
            font-size: 1.20rem;
            line-height: 1.50rem;
        }}

        .text-end {{
            text-align: end;
        }}
    </style>
</head>
<body>
    <div id=""invoice"">
        <!-- Invoice Header -->
        <table class=""w-full table-fixed header"">
            <tr>
                <td>
                    <table class=""w-full uppercase"">
                        <tr>
                            <td class=""logo-container"">
                                <!-- Company Logo -->
                                <img src=""{ImageData.Logo}"" alt=""Company Logo"" class=""logo"">
                            </td>
                            <td>
                                <p class=""logo-text"">Raza Gems lLC</p>
                                <p class=""logo-description"">
                                    <span class=""spacing"">S</span>
                                    <span class=""spacing"">t</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">n</span>
                                    <span class=""spacing"">e</span>
                                    <span class=""spacing"">s</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">t</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">m</span>
                                    <span class=""spacing"">a</span>
                                    <span class=""spacing"">k</span>
                                    <span class=""spacing"">e</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">y</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">u</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">s</span>
                                    <span class=""spacing"">h</span>
                                    <span class=""spacing"">i</span>
                                    <span class=""spacing"">n</span>
                                    <span class=""spacing"">e</span>
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
                <td class=""header-right"">
                    <table align=""right"" class=""text-sm"">
                        <tr>
                            <td>
                                <a  style=""margin-bottom:5px;"" href=""{CompanyInfo.Website}"" target=""_blank"">{CompanyInfo.Website}</a>
                            </td>
                            <td class=""text-center"">
                                <img style=""margin-bottom:-5px;"" src=""../Images/browser.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <a  style=""margin-bottom:5px;"" href=""mailto:{CompanyInfo.Email}"">{CompanyInfo.Email}</a>
                            </td>
                            <td class=""text-center"">
                                <img  style=""margin-bottom:-5px;"" src=""../Images/attherate.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span  style=""margin-bottom:5px;"" >{CompanyInfo.Area1}&nbsp;|&nbsp;{CompanyInfo.Area2}</span>
                            </td>
                            <td class=""text-center"">
                                <img  style=""margin-bottom:-5px;"" src=""../Images/location.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <table class=""w-full uppercase body-header"">
            <tr>
                <td class=""font-bold text-28"">
                    <span>Tax Approval</span>
                </td>
            </tr>
            <tr class=""text-xl"">
                <td>
                    <span class=""font-bold "">Trn:</span>
                    <span>{CompanyInfo.Trn}</span>
                </td>
            </tr>
        </table>
        <!-- Details -->
        <table class=""w-full details"">
            <tr>
                <td class=""pb-6"">
                    <table class=""text-gray-500"">
                        <tr>
                            <td>
                            <span class=""text-2xl font-bold"">{approvalPDF.Approval.Customer.Name}</span>
                        </td>
                        </tr>
                        <tr class=""text-sm"">
                            <td>
                                <span style=""width:4rem"">{approvalPDF.Approval.Customer.Address}</span>
                            </td>
                        </tr>
                        <tr class=""text-sm"">
                            <td>{approvalPDF.Approval.Customer.Phone}</td>
                        </tr>
                    </table>
                </td>
                <td align=""right"" class=""pb-6"">
                    <table class=""text-end"">
                        <tr>
                            <td class="""">
                                <span class=""text-gray-500 text-sm"">APPROVAL</span>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span class=""font-semibold text-2xl"">#{approvalPDF.Approval.ApprovalNo}</span>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</body>
</html>";
            System.IO.File.WriteAllBytes(headerURl, System.Text.Encoding.ASCII.GetBytes(HeaderHtmlString));

            // Create header 
            string footerURl = Path.Combine(Environment.CurrentDirectory,
                @"TempHtmlTemplate/Footers/" + "Approval-Footer-" + Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddHHmmssffff") + approvalPDF.Approval.Id + ".html");
            var FooterHtmlString = @$"<!DOCTYPE html>
<html>

<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        a {{
            text-decoration: none;
            color: black;
        }}

        .w-full {{
            width: 100%;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .text-base {{
            font-size: 1.25rem;
            line-height: 1.75rem;
        }}

        .font-normal {{
            font-weight: 400;
        }}

        /* FOOTER */
        .footer {{
            margin: auto;
            margin-top: 6px;
            margin-bottom: 10px;
        }}

        /* section-1 */
        .footer-section1 {{
            border-bottom: 1px solid #b99a37;
        }}

        .pb-7 {{
            padding-bottom: 7px;
        }}

        .footer-border-bottom {{
            border-bottom: 1px solid #b99a37;
        }}

        .w-245 {{
            width: 350px;
        }}

        .text-2xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .text-17 {{
            font-size: 22px;
        }}

        .text-3xl {{
            font-size: 2rem;
            line-height: 2.25rem;
        }}

        /* Section -2 */

        .footer-section2 {{
            padding-top: 0.75rem;
        }}

        .pt-1 {{
            padding-top: 0.25rem;
        }}

        .text-xl {{
            font-size: 15rem;
            line-height: 2rem;
        }}

        .phone-icon {{
            color: #B99A37;
        }}

        .text-12 {{
            font-size: 16px;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .ps-1 {{
            padding-inline-start: 0.25rem;
        }}
    </style>
</head>

<body>
    <!-- Invoice Footer -->
    <table class=""w-full footer"" id=""footer"">
        <tr>
            <td class=""pb-7 footer-section1"">
                <table class=""w-full"">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <span class=""font-bold text-base"">Terms: </span>
                                        <span class=""font-normal text-base"">{approvalPDF.Terms}</span>
                                    </td>
                                </tr>
                                <tr style=""margin-bottom: 20px;"">
                                    <td>
                                        <span class=""font-bold text-base"">Remarks: </span>
                                        <span class=""font-normal text-base"">{approvalPDF.Remarks}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span class=""font-bold text-17"">Reciever:&nbsp;{approvalPDF.Reciver}</span><br>
                                        <span class=""font-bold text-17"">Signature:</span>
                                        <span class=""font-black text-3xl"">_________</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td class=""w-245"">
                            <span class=""font-bold text-17"">Sales Person:&nbsp;{approvalPDF.salesman}</span><br>
                            <span class=""font-bold text-17"">Signature:</span>
                            <span class=""font-black text-3xl"">_________</span>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class=""footer-section2"">
                <table class=""w-full"">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td colspan=""2"" class=""pt-1"">
                                        <img style=""margin-bottom:-5px;"" src=""../Images/phone.svg"">
                                        <span class=""text-base font-bold""
                                              style=""margin-bottom:10px;"">Helpline</span><br>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>Mob:</span>
                                    </td>
                                    <td class=""font-semibold"">
                                        <a href=""tel:{CompanyInfo.Phone1}""> {CompanyInfo.Phone1}</a>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>Mob:</span>
                                    </td>
                                    <td class=""font-semibold"">
                                        <a href=""tel:{CompanyInfo.Phone2}""> {CompanyInfo.Phone2}</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td align=""right"">
                            <table>
                                <tr>
                                    <td colspan=""2"">
                                            <img  style=""margin-bottom:-5px;"" src=""../Images/location.svg"" alt="""" srcset="""" >
                                        <span  style=""margin-bottom:10px;"" class=""text-base font-bold"">Our Branches</span><br>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>{CompanyInfo.Area1}: </span>
                                    </td>
                                    <td class=""font-semibold ps-1"">
                                        <span>
                                            {CompanyInfo.Address1}
                                        </span>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>{CompanyInfo.Area2}: </span>
                                    </td>
                                    <td class=""font-semibold ps-1"">
                                        <span>{CompanyInfo.Address2}</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>

</html>";
            System.IO.File.WriteAllBytes(footerURl, System.Text.Encoding.ASCII.GetBytes(FooterHtmlString));


            var HTMLContent = @$"<html>

<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        .w-full {{
            width: 100%;
            margin-left: auto;
            margin-right: auto;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .font-medium {{
            font-weight: 500;
        }}

        .text-base {{
            font-size: 1rem;
            line-height: 1.5rem;
        }}

        /* invoice table */
        .invoice-table {{
            margin-top: 1.5rem;
            margin-bottom: 3.5rem;
            border-collapse: collapse;
        }}

        /* invoice table header */

        .invoice-table-header {{
            color: white;
            text-align: start;
        }}

        .py-2_5 {{
            padding-top: 0.625rem;
            padding-bottom: 0.625rem;
        }}

        .px-4 {{
            padding-left: 1rem;
            padding-right: 1rem;
        }}

        .h-6 {{
            height: 1.5rem;
        }}

        .invoice-table-details {{
            margin-bottom: 1rem;
            font-weight: 400;
            font-size: 1rem;
            line-height: 1.25rem;
        }}

.text-sm{{
    font-size: 1.25rem;
}}

        .invoice-table-details tr:nth-child(odd) {{
            background-color: #efefef;
        }}

        .py-2 {{
            padding-top: 0.5rem;
            padding-bottom: 0.5rem;
        }}

        /* customer-details */

        .customer-details {{
            border-bottom: 1px solid #D4D4D4;
            font-size: 11px;
            color: rgb(75 85 99);
        }}

        .pb-8 {{
            padding-bottom: 2rem;
        }}

        .pt-5 {{
            padding-top: 1.25rem;
        }}

        .sub-total {{
            padding-right: 20px;
        }}

        .table-spacing {{
            border-collapse: separate;
            border-spacing: var(--tw-border-spacing-x) 0.25rem;
        }}

        .text-right {{
            text-align: right;
        }}

        .pt-0 {{
            padding-top: 0px;
        }}

        .text-gray-700 {{
            color: rgb(55 65 81);
        }}

        .text-gray-400 {{
            color: rgb(156 163 175);
        }}

        .table-responsive {{ overflow-x: visible !important; }}

        .webgrid-table thead {{
    display: table-header-group;
}}

.webgrid-table tfoot {{
    display: table-row-group;
}}

.webgrid-table tr {{
    page-break-inside: avoid;
}}
    </style>
</head>

<body>
    <div class=""table-responsive"">
        <table class=""w-full invoice-table webgrid-table"">
            <thead class=""uppercase"">
                <tr bgcolor=""#323232"" class=""invoice-table-header text-sm"" style=""text-align: start;"">
                    <th class=""py-2_5 px-4"">S/No</th>
                    <th class=""py-2_5 px-4"">Description</th>
                    <th class=""py-2_5 px-4"">Lot No</th>
                    <th class=""py-2_5 px-4"" colspan=""2"">Quantity</th>
                    <th class=""py-2_5 px-4""  colspan=""2"">Price Per Unit</th>
                    <th class=""py-2_5 px-4"">Amount<br>(AED)</th>
                    <th class=""py-2_5 px-4"">VAT @5%<br>(AED)</th>
                    <th class=""py-2_5 px-4"">Total Amount<br>(AED)</th>
                </tr>
                <tr class=""h-6""></tr>
            </thead>
            <tbody class=""invoice-table-details text-gray-500"">
";

            var srNo = 0;
            foreach (var item in approvalPDF.Items)
            {
                HTMLContent += @$"<tr>
                    <td  class=""py-2 px-4"">{++srNo}</td>
                    <td  class=""py-2 px-4"">{item.Description}</td>
                    <td  class=""py-2 px-4"">{item.LotNo ?? "-"}</td>
                    <td  class=""py-2 px-4"">{item.WeightCarats}</td>
                    <td  class=""py-2 px-4"">{item.QuantityUnit}</td>
                    <td  class=""py-2 px-4"">{item.PricePerUnit}</td>
                    <td  class=""py-2 px-4"">{item.PricePerUnitCurrancy}</td>
                    <td  class=""py-2 px-4"">{item.Amount}</td>
                    <td  class=""py-2 px-4"">{item.Vat}</td>
                    <td  class=""py-2 px-4"">{item.AmountIncludingVat}</td>
                </tr>";
            }


            HTMLContent += @$"</tbody>
        </table>
    </div>
    <table class=""w-full customer-details uppercase webgrid-table "">
        <tr>
            <td class=""pb-8 pt-5"">
                <table class=""table-spacing "">
                </table>
            </td>
            <td class=""pb-8 sub-total"" style=""width: 500px;"">
                <table class=""w-full table-spacing text-sm text-gray-700 uppercase"">
                    <tr>
                        <td class=""text-right font-bold pt-0"">Subtotal</td>
                        <td class=""text-right font-bold text-gray-400"">{approvalPDF.Approval.TotalAmount:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Tax</td>
                        <td class=""text-right font-bold text-gray-400"">{approvalPDF.Approval.Vat:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Total</td>
                        <td class=""text-right font-bold"">{approvalPDF.Approval.TotalAmountIncludingVat:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Total In Words</td>
                        <td class=""text-right font-bold"" style=""text-transform:capitalize; padding-left: 25px;"">{NumberToWordConvertor.NumberToWords(approvalPDF.Approval.TotalAmountIncludingVat)}</td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    </div>
</body>

</html>";


            var globalSetting = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings
                {
                    Top = 100,
                    Bottom = 70,
                    Left = 10,
                    Right = 10,
                },
                DocumentTitle = $"Approval#{approvalPDF.Approval.ApprovalNo}"
            };

            var objectSetting = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = HTMLContent,
                WebSettings = { DefaultEncoding = "UTF-8" },
                HeaderSettings = { Spacing = 5, HtmUrl = headerURl },
                FooterSettings = { Spacing = 10, HtmUrl = footerURl }
            };

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSetting,
                Objects = { objectSetting }
            };

            var bytePDF = _converter.Convert(document);

            if (File.Exists(headerURl))
            {
                File.Delete(headerURl);
            }
            if (File.Exists(footerURl))
            {
                File.Delete(footerURl);
            }

            return bytePDF;
        }

        public byte[] GenerateInvoicePDF(InvoicePDF invoicePDF)
        {
            // Create header 
            string headerURl = Path.Combine(Environment.CurrentDirectory,
                @"TempHtmlTemplate/Headers/" + "Invoice-Header-" + Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddHHmmssffff") + invoicePDF.Invoice.Id + ".html");
            var HeaderHtmlString = @$"<!DOCTYPE html>
<html>
<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        .w-full {{
            width: 100%;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        a {{
            text-decoration: none;
            color: #444142;
        }}

        .text-center {{
            text-align: center;
        }}

        .header {{
            margin: auto;
            border-collapse: collapse;
            border-bottom: 1px solid #b99a37;
        }}

        .logo-container {{
            padding-inline-end: 0px;
            width: 92px;
        }}

        .logo {{
            height: 5rem;
        }}

        .logo-text {{
            font-weight: 600;
            font-size: 2.75rem;
            line-height: 2.75rem;
            color: #B99A37;
        }}

        .logo-description {{
            font-weight: 600;
            font-size: 16px;
        }}

        .spacing {{
            display: inline-block;
            margin: 0;
             margin-left:0.06rem;
        }}

        .header-right {{
            width: 40%;
            padding: 2.5rem;
            padding-right: 0.5rem;
            color: #444142;
            text-align: right;
        }}

        .header-svg {{
            display: inline;
            margin-bottom: 0.25rem;
            padding-left: 0.375rem;
        }}

        /* Body Header */

        .body-header {{
            margin-top: 1.75rem;
            text-align: center;
            letter-spacing: 0.05em;
        }}

        .text-28 {{
            font-size: 31px;
        }}

        .text-xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .w-full {{
            width: 100%;
            margin-left: auto;
            margin-right: auto;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .font-medium {{
            font-weight: 500;
        }}

        .text-base {{
            font-size: 1.25rem;
            line-height: 1.75rem;
        }}

        /* Details */

        .details {{
            margin-top: 2rem;
            border-bottom: 2px solid #bbbbbb;
            padding-bottom: 1rem;
        }}

        .pb-6 {{
            margin-top: 2rem;
        }}

        .text-gray-500 {{
            color: rgb(107 114 128);
        }}

        .text-2xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .pb-2 {{
            padding-bottom: 0.5rem;
        }}

        .text-sm {{
            font-size: 1.20rem;
            line-height: 1.50rem;
        }}

        .text-end {{
            text-align: end;
        }}
    </style>
</head>
<body>
    <div id=""invoice"">
        <!-- Invoice Header -->
        <table class=""w-full table-fixed header"">
            <tr>
                <td>
                    <table class=""w-full uppercase"">
                        <tr>
                            <td class=""logo-container"">
                                <!-- Company Logo -->
                                <img src=""{ImageData.Logo}"" alt=""Company Logo"" class=""logo"">
                            </td>
                            <td>
                                <p class=""logo-text"">Raza Gems lLC</p>
                                <p class=""logo-description"">
                                    <span class=""spacing"">S</span>
                                    <span class=""spacing"">t</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">n</span>
                                    <span class=""spacing"">e</span>
                                    <span class=""spacing"">s</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">t</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">m</span>
                                    <span class=""spacing"">a</span>
                                    <span class=""spacing"">k</span>
                                    <span class=""spacing"">e</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">y</span>
                                    <span class=""spacing"">o</span>
                                    <span class=""spacing"">u</span>
                                    <span class=""spacing"">&nbsp;</span>
                                    <span class=""spacing"">s</span>
                                    <span class=""spacing"">h</span>
                                    <span class=""spacing"">i</span>
                                    <span class=""spacing"">n</span>
                                    <span class=""spacing"">e</span>
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
                <td class=""header-right"">
                    <table align=""right"" class=""text-sm"">
                        <tr>
                            <td>
                                <a  style=""margin-bottom:5px;"" href=""{CompanyInfo.Website}"" target=""_blank"">{CompanyInfo.Website}</a>
                            </td>
                            <td class=""text-center"">
                                <img style=""margin-bottom:-5px;"" src=""../Images/browser.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <a  style=""margin-bottom:5px;"" href=""mailto:{CompanyInfo.Email}"">{CompanyInfo.Email}</a>
                            </td>
                            <td class=""text-center"">
                                <img  style=""margin-bottom:-5px;"" src=""../Images/attherate.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span  style=""margin-bottom:5px;"" >{CompanyInfo.Area1}&nbsp;|&nbsp;{CompanyInfo.Area2}</span>
                            </td>
                            <td class=""text-center"">
                                <img  style=""margin-bottom:-5px;"" src=""../Images/location.svg"" alt="""" srcset="""" class=""header-svg"">
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <table class=""w-full uppercase body-header"">
            <tr>
                <td class=""font-bold text-28"">
                    <span>Tax Invoice</span>
                </td>
            </tr>
            <tr class=""text-xl"">
                <td>
                    <span class=""font-bold "">Trn:</span>
                    <span>{CompanyInfo.Trn}</span>
                </td>
            </tr>
        </table>
        <!-- Details -->
        <table class=""w-full details"">
            <tr>
                <td class=""pb-6"">
                    <table class=""text-gray-500"">
                        <tr>
                            <td>
                            <span class=""text-2xl font-bold"">{invoicePDF.Invoice.Customer.Name}</span>
                        </td>
                        </tr>
                        <tr class=""text-sm"">
                            <td>
                                <span style=""width:4rem"">{invoicePDF.Invoice.Customer.Address}</span>
                            </td>
                        </tr>
                        <tr class=""text-sm"">
                            <td>{invoicePDF.Invoice.Customer.Phone}</td>
                        </tr>
                    </table>
                </td>
                <td align=""right"" class=""pb-6"">
                    <table class=""text-end"">
                        <tr>
                            <td class="""">
                                <span class=""text-gray-500 text-sm"">INVOICE</span>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span class=""font-semibold text-2xl"">#{invoicePDF.Invoice.InvoiceNo}</span>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</body>
</html>";
            System.IO.File.WriteAllBytes(headerURl, System.Text.Encoding.ASCII.GetBytes(HeaderHtmlString));

            // Create header 
            string footerURl = Path.Combine(Environment.CurrentDirectory,
                @"TempHtmlTemplate/Footers/" + "Invoice-Footer-" + Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddHHmmssffff") + invoicePDF.Invoice.Id + ".html");
            var FooterHtmlString = @$"<!DOCTYPE html>
<html>

<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        a {{
            text-decoration: none;
            color: black;
        }}

        .w-full {{
            width: 100%;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .text-base {{
            font-size: 1.25rem;
            line-height: 1.75rem;
        }}

        .font-normal {{
            font-weight: 400;
        }}

        /* FOOTER */
        .footer {{
            margin: auto;
            margin-top: 6px;
            margin-bottom: 10px;
        }}

        /* section-1 */
        .footer-section1 {{
            border-bottom: 1px solid #b99a37;
        }}

        .pb-7 {{
            padding-bottom: 7px;
        }}

        .footer-border-bottom {{
            border-bottom: 1px solid #b99a37;
        }}

        .w-245 {{
            width: 350px;
        }}

        .text-2xl {{
            font-size: 1.75rem;
            line-height: 2.25rem;
        }}

        .text-17 {{
            font-size: 22px;
        }}

        .text-3xl {{
            font-size: 2rem;
            line-height: 2.25rem;
        }}

        /* Section -2 */

        .footer-section2 {{
            padding-top: 0.75rem;
        }}

        .pt-1 {{
            padding-top: 0.25rem;
        }}

        .text-xl {{
            font-size: 15rem;
            line-height: 2rem;
        }}

        .phone-icon {{
            color: #B99A37;
        }}

        .text-12 {{
            font-size: 16px;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .ps-1 {{
            padding-inline-start: 0.25rem;
        }}
    </style>
</head>

<body>
    <!-- Invoice Footer -->
    <table class=""w-full footer"" id=""footer"">
        <tr>
            <td class=""pb-7 footer-section1"">
                <table class=""w-full"">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <span class=""font-bold text-base"">Terms: </span>
                                        <span class=""font-normal text-base"">{invoicePDF.Terms}</span>
                                    </td>
                                </tr>
                                <tr style=""margin-bottom: 20px;"">
                                    <td>
                                        <span class=""font-bold text-base"">Remarks: </span>
                                        <span class=""font-normal text-base"">{invoicePDF.Remarks}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span class=""font-bold text-17"">Reciever:&nbsp;{invoicePDF.Reciver}</span><br>
                                        <span class=""font-bold text-17"">Signature:</span>
                                        <span class=""font-black text-3xl"">_________</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td class=""w-245"">
                            <span class=""font-bold text-17"">Sales Person:&nbsp;{invoicePDF.salesman}</span><br>
                            <span class=""font-bold text-17"">Signature:</span>
                            <span class=""font-black text-3xl"">_________</span>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class=""footer-section2"">
                <table class=""w-full"">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td colspan=""2"" class=""pt-1"">
                                        <img style=""margin-bottom:-5px;"" src=""../Images/phone.svg"">
                                        <span class=""text-base font-bold""
                                              style=""margin-bottom:10px;"">Helpline</span><br>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>Mob:</span>
                                    </td>
                                    <td class=""font-semibold"">
                                        <a href=""tel:{CompanyInfo.Phone1}""> {CompanyInfo.Phone1}</a>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>Mob:</span>
                                    </td>
                                    <td class=""font-semibold"">
                                        <a href=""tel:{CompanyInfo.Phone2}""> {CompanyInfo.Phone2}</a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td align=""right"">
                            <table>
                                <tr>
                                    <td colspan=""2"">
                                            <img  style=""margin-bottom:-5px;"" src=""../Images/location.svg"" alt="""" srcset="""" >
                                        <span  style=""margin-bottom:10px;"" class=""text-base font-bold"">Our Branches</span><br>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>{CompanyInfo.Area1}: </span>
                                    </td>
                                    <td class=""font-semibold ps-1"">
                                        <span>
                                            {CompanyInfo.Address1}
                                        </span>
                                    </td>
                                </tr>
                                <tr class=""text-12"">
                                    <td class=""font-bold"">
                                        <span>{CompanyInfo.Area2}: </span>
                                    </td>
                                    <td class=""font-semibold ps-1"">
                                        <span>{CompanyInfo.Address2}</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>

</html>";
            System.IO.File.WriteAllBytes(footerURl, System.Text.Encoding.ASCII.GetBytes(FooterHtmlString));


            var HTMLContent = @$"<html>

<head>
    <style>
        * {{
            margin: 0px;
            padding: 0px;
            font-family: Calibri;
        }}

        .w-full {{
            width: 100%;
            margin-left: auto;
            margin-right: auto;
        }}

        .uppercase {{
            text-transform: uppercase;
        }}

        .font-bold {{
            font-weight: 700;
        }}

        .font-semibold {{
            font-weight: 500;
        }}

        .font-medium {{
            font-weight: 500;
        }}

        .text-base {{
            font-size: 1rem;
            line-height: 1.5rem;
        }}

        /* invoice table */
        .invoice-table {{
            margin-top: 1.5rem;
            margin-bottom: 3.5rem;
            border-collapse: collapse;
        }}

        /* invoice table header */

        .invoice-table-header {{
            color: white;
            text-align: start;
        }}

        .py-2_5 {{
            padding-top: 0.625rem;
            padding-bottom: 0.625rem;
        }}

        .px-4 {{
            padding-left: 1rem;
            padding-right: 1rem;
        }}

        .h-6 {{
            height: 1.5rem;
        }}

        .invoice-table-details {{
            margin-bottom: 1rem;
            font-weight: 400;
            font-size: 1rem;
            line-height: 1.25rem;
        }}

.text-sm{{
    font-size: 1.25rem;
}}

        .invoice-table-details tr:nth-child(odd) {{
            background-color: #efefef;
        }}

        .py-2 {{
            padding-top: 0.5rem;
            padding-bottom: 0.5rem;
        }}

        /* customer-details */

        .customer-details {{
            border-bottom: 1px solid #D4D4D4;
            font-size: 11px;
            color: rgb(75 85 99);
        }}

        .pb-8 {{
            padding-bottom: 2rem;
        }}

        .pt-5 {{
            padding-top: 1.25rem;
        }}

        .sub-total {{
            padding-right: 20px;
        }}

        .table-spacing {{
            border-collapse: separate;
            border-spacing: var(--tw-border-spacing-x) 0.25rem;
        }}

        .text-right {{
            text-align: right;
        }}

        .pt-0 {{
            padding-top: 0px;
        }}

        .text-gray-700 {{
            color: rgb(55 65 81);
        }}

        .text-gray-400 {{
            color: rgb(156 163 175);
        }}

        .table-responsive {{ overflow-x: visible !important; }}

        .webgrid-table thead {{
    display: table-header-group;
}}

.webgrid-table tfoot {{
    display: table-row-group;
}}

.webgrid-table tr {{
    page-break-inside: avoid;
}}
    </style>
</head>

<body>
    <div class=""table-responsive"">
        <table class=""w-full invoice-table webgrid-table"">
            <thead class=""uppercase"">
                <tr bgcolor=""#323232"" class=""invoice-table-header text-sm"" style=""text-align: start;"">
                    <th class=""py-2_5 px-4"">S/No</th>
                    <th class=""py-2_5 px-4"">Description</th>
                    <th class=""py-2_5 px-4"">Lot No</th>
                    <th class=""py-2_5 px-4"" colspan=""2"">Quantity</th>
                    <th class=""py-2_5 px-4""  colspan=""2"">Price Per Unit</th>
                    <th class=""py-2_5 px-4"">Amount<br>(AED)</th>
                    <th class=""py-2_5 px-4"">VAT @5%<br>(AED)</th>
                    <th class=""py-2_5 px-4"">Total Amount<br>(AED)</th>
                </tr>
                <tr class=""h-6""></tr>
            </thead>
            <tbody class=""invoice-table-details text-gray-500"">
";

            var srNo = 0;
            foreach (var item in invoicePDF.Items)
            {
                HTMLContent += @$"<tr>
                    <td  class=""py-2 px-4"">{++srNo}</td>
                    <td  class=""py-2 px-4"">{item.Description}</td>
                    <td  class=""py-2 px-4"">{item.LotNo ?? "-"}</td>
                    <td  class=""py-2 px-4"">{item.WeightCarats}</td>
                    <td  class=""py-2 px-4"">{item.QuantityUnit}</td>
                    <td  class=""py-2 px-4"">{item.PricePerUnit}</td>
                    <td  class=""py-2 px-4"">{item.PricePerUnitCurrancy}</td>
                    <td  class=""py-2 px-4"">{item.Amount}</td>
                    <td  class=""py-2 px-4"">{item.Vat}</td>
                    <td  class=""py-2 px-4"">{item.AmountIncludingVat}</td>
                </tr>";
            }


            HTMLContent += @$"</tbody>
        </table>
    </div>
    <table class=""w-full customer-details uppercase webgrid-table "">
        <tr>
            <td class=""pb-8 pt-5"">
                <table class=""table-spacing text-sm "">
                    <tr>
                        <td>
                            <span class=""font-semibold"">Payment Date</span>
                        </td>
                    </tr>
                    <tr>
                        <td>";

            if (invoicePDF.Invoice.Status == InvoiceStatus.Paid)
            {
                HTMLContent += @$"<span>{invoicePDF.Invoice.PaymentDate!.Value.ToString("dd MMMM yyyy")}</span>";

            }
            else
            {
                HTMLContent += @$"<span>{InvoiceStatus.Pending}</span>";
            }

            HTMLContent += @$"</td>
                    </tr>
                </table>
            </td>
            <td class=""pb-8 sub-total"" style=""width: 500px;"">
                <table class=""w-full table-spacing text-sm text-gray-700 uppercase"">
                    <tr>
                        <td class=""text-right font-bold pt-0"">Subtotal</td>
                        <td class=""text-right font-bold text-gray-400"">{invoicePDF.Invoice.TotalAmount:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Tax</td>
                        <td class=""text-right font-bold text-gray-400"">{invoicePDF.Invoice.Vat:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Total</td>
                        <td class=""text-right font-bold"">{invoicePDF.Invoice.TotalAmountIncludingVat:N2}&nbsp;AED</td>
                    </tr>
                    <tr>
                        <td class=""text-right font-bold"">Total In Words</td>
                        <td class=""text-right font-bold"" style=""text-transform:capitalize; padding-left: 25px;"">{NumberToWordConvertor.NumberToWords(invoicePDF.Invoice.TotalAmountIncludingVat)}</td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    </div>
</body>

</html>";


            var globalSetting = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings
                {
                    Top = 100,
                    Bottom = 70,
                    Left = 10,
                    Right = 10,
                },
                DocumentTitle = $"Invoice#{invoicePDF.Invoice.InvoiceNo}"
            };

            var objectSetting = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = HTMLContent,
                WebSettings = { DefaultEncoding = "UTF-8" },
                HeaderSettings = { Spacing = 5, HtmUrl = headerURl },
                FooterSettings = { Spacing = 10, HtmUrl = footerURl }
            };

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSetting,
                Objects = { objectSetting }
            };

            var bytePDF = _converter.Convert(document);

            if (File.Exists(headerURl))
            {
                File.Delete(headerURl);
            }
            if (File.Exists(footerURl))
            {
                File.Delete(footerURl);
            }

            return bytePDF;
        }
    }
}
