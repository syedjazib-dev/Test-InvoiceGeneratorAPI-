using AutoMapper;
using ClosedXML.Excel;
using CRM.Models;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.Utility.PDFService;
using InvoiceGenerator.StaticData;
using InvoiceGenerator.StaticData.Status;
using InvoiceGenerator.StaticData.TableFields;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using DocumentFormat.OpenXml.InkML;
using InvoiceGenerator.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IPDFService _pdfService;

        public InvoiceController(IUnitOfWork unitOfWork, IMapper mapper, IPDFService pdfService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
            _pdfService = pdfService;
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id not Provided.");
                    return _response;
                }

                var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == id, IncludeProperties: $"{InvoiceField.Customer},{InvoiceField.Salesman},{InvoiceField.InvoiceApprovals}");

                if (invoice == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invoice not found.");
                    return _response;
                }
                var invoiceApprovals = await _unitOfWork.InvoiceApprovalRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceApprovalField.Approval);
                var invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceItemField.Item);
                invoice.InvoiceApprovals = invoiceApprovals.ToList();
                invoice.InvoiceItems = invoiceItems.ToList();

                InvoiceResponseDTO invoiceResponseDTO = _mapper.Map<InvoiceResponseDTO>(invoice);

                _response.Data = invoiceResponseDTO;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] string? status, [FromQuery] int? customerId, [FromQuery] string? createDateMiliSecondsString, [FromQuery] string? search, [FromQuery] string? orderBy, [FromQuery] string? order = Order.ASC, [FromQuery] int PageSize = 0, [FromQuery] int PageNo = 1)
        {
            try
            {
                IEnumerable<Invoice> invoices = new List<Invoice>();
                RecordsResponse recordsResponse = new RecordsResponse()
                {
                    TotalRecords = 0,
                    Records = new List<Approval>()
                };

                DateTime? createDate = null;
                if (!string.IsNullOrEmpty(createDateMiliSecondsString))
                {
                    createDate = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(createDateMiliSecondsString)).Date;
                }

                if (string.IsNullOrEmpty(orderBy) || orderBy == "null")
                {
                    recordsResponse = await _unitOfWork.InvoiceRepository.GetAllWithSearchAndFilterAsync(status: status, customerId: customerId, createDate: createDate, search: search, PageSize: PageSize, PageNo: PageNo, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
                }
                else if (string.IsNullOrEmpty(order) || order == "null" || order == Order.ASC)
                {
                    /*order ASC*/
                    if (orderBy != ApprovalField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.InvoiceRepository.GetAllWithSearchAndFilterAsync(status: status, customerId: customerId, createDate: createDate, search: search, OrderBy: _unitOfWork.InvoiceRepository.CreateSelectorExpression(orderBy), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.InvoiceRepository.GetAllWithSearchAndFilterAsync(status: status, customerId: customerId, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
                    }
                }
                else
                {
                    /*order DSE*/
                    if (orderBy != ApprovalField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.InvoiceRepository.GetAllWithSearchAndFilterAsync(status: status, customerId: customerId, createDate: createDate, search: search, OrderBy: _unitOfWork.InvoiceRepository.CreateSelectorExpression(orderBy), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.InvoiceRepository.GetAllWithSearchAndFilterAsync(status: status, customerId: customerId, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
                    }
                }

                Pagination pagination = new Pagination { PageNo = PageNo, PageSize = PageSize };
                Response.Headers.Append("x-pagination", JsonConvert.SerializeObject(pagination));
                invoices = recordsResponse.Records;

                IEnumerable<InvoiceResponseDTO> invoiceResponseDTOs = invoices.Select(entity => _mapper.Map<InvoiceResponseDTO>(entity));

                _response.Data = new RecordsResponse

                {
                    TotalRecords = recordsResponse.TotalRecords,
                    Records = invoiceResponseDTOs
                };
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }



        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Create(InvoiceCreateDTO invoiceCreateDTO)
        {
            try
            {
                if (invoiceCreateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Invoice info not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                Invoice invoice = _mapper.Map<Invoice>(invoiceCreateDTO);
                invoice = await _unitOfWork.InvoiceRepository.CreateAsync(invoice);

                invoice.InvoiceNo = invoice.Id.ToString().PadLeft(5, '0');
                _unitOfWork.InvoiceRepository.Update(invoice);
                _unitOfWork.Save();

                foreach (var approvalId in invoiceCreateDTO.NewApprovalIds)
                {
                    var approval = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == approvalId);
                    if (approval != null)
                    {
                        await _unitOfWork.InvoiceApprovalRepository.CreateAsync(new InvoiceApproval
                        {
                            InvoiceId = invoice.Id,
                            ApprovalId = approvalId
                        });
                    }
                }

                InvoiceResponseDTO invoiceResponse = _mapper.Map<InvoiceResponseDTO>(invoice);
                _response.Data = invoiceResponse;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Update(InvoiceUpdateDTO invoiceUpdateDTO)
        {
            try
            {
                if (invoiceUpdateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Invoice not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                var invoiceFromDb = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceUpdateDTO.Id, IncludeProperties: $"{InvoiceField.InvoiceApprovals}");
                if (invoiceFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invoice not found.");
                    return _response;
                }

                var invoiceApprovals = await _unitOfWork.InvoiceApprovalRepository.GetAllAsync(u => u.InvoiceId == invoiceUpdateDTO.Id, IncludeProperties: InvoiceApprovalField.Approval);
                invoiceFromDb.InvoiceApprovals = invoiceApprovals.ToList();

                if (invoiceFromDb.Status == InvoiceStatus.Paid)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("You can't update paid invoice");
                    return _response;
                }

                invoiceFromDb.TotalAmount = invoiceUpdateDTO.TotalAmount;
                invoiceFromDb.Vat = invoiceUpdateDTO.Vat;
                invoiceFromDb.TotalAmountIncludingVat = invoiceUpdateDTO.TotalAmountIncludingVat;
                invoiceFromDb.SalesmanId = invoiceUpdateDTO.SalesmanId;
                invoiceFromDb.CustomerId = invoiceUpdateDTO.CustomerId;
                invoiceFromDb.Status = invoiceUpdateDTO.Status;
                invoiceFromDb.PaymentDate = invoiceUpdateDTO.PaymentDate;
                invoiceFromDb.UpdateDate = invoiceUpdateDTO.UpdateDate;

                var currentApprovals = invoiceFromDb.InvoiceApprovals.ToList();
                await _unitOfWork.InvoiceApprovalRepository.RemoveRangeAsync(currentApprovals);


                var invoiceApprovalsToCreate = new List<InvoiceApproval>();
                foreach (var approvalId in invoiceUpdateDTO.NewApprovalIds)
                {
                    var approval = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == approvalId);
                    if (approval != null)
                    {
                        invoiceApprovalsToCreate.Add(new InvoiceApproval
                        {
                            InvoiceId = invoiceFromDb.Id,
                            ApprovalId = approvalId
                        });
                    }
                }
                await _unitOfWork.InvoiceApprovalRepository.CreateRangeAsync(invoiceApprovalsToCreate);

                _unitOfWork.InvoiceRepository.Update(invoiceFromDb);
                _unitOfWork.Save();

                InvoiceResponseDTO invoiceResponse = _mapper.Map<InvoiceResponseDTO>(invoiceFromDb);
                _response.Data = invoiceResponse;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize(Roles = UserRole.Admin)]
        [HttpPut("mark/pending/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> MarkPending(int id)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Invoice id not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                var invoiceFromDb = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == id);
                if (invoiceFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invoice not found.");
                    return _response;
                }

                invoiceFromDb.Status = InvoiceStatus.Pending;

                _unitOfWork.InvoiceRepository.Update(invoiceFromDb);

                InvoiceResponseDTO invoiceResponse = _mapper.Map<InvoiceResponseDTO>(invoiceFromDb);
                _response.Data = invoiceResponse;
                _unitOfWork.Save();

                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Id not provided.");
                    return _response;
                }

                var invoiceFromDb = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == id, IncludeProperties: $"{InvoiceField.InvoiceApprovals}");

                if (invoiceFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invoice not found.");
                    return _response;
                }
                var invoiceApprovals = await _unitOfWork.InvoiceApprovalRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceApprovalField.Approval);
                var invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceItemField.Item);
                invoiceFromDb.InvoiceApprovals = invoiceApprovals.ToList();
                invoiceFromDb.InvoiceItems = invoiceItems.ToList();

                if (invoiceFromDb.Status == InvoiceStatus.Paid)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("You can't delete paid invoice.");
                    return _response;
                }

                var itemsToDelete = new List<Item>();
                var itemsToUpdate = new List<Item>();
                foreach (var invoiceItem in invoiceFromDb.InvoiceItems)
                {
                    if (invoiceItem.Item.ApprovalId != null)
                    {
                        if (invoiceItem.Item.Status == ItemStatus.Billed)
                        {
                            invoiceItem.Item.Status = ItemStatus.Pending;
                        }
                        itemsToUpdate.Add(invoiceItem.Item);
                    }
                    else
                    {
                        itemsToDelete.Add(invoiceItem.Item);
                    }
                }

                if (itemsToDelete != null && itemsToDelete.Any())
                {
                    var map = new Dictionary<int, Item>();
                    var roots = new List<Item>();
                    foreach (var item in itemsToDelete)
                    {
                        map[item.Id] = item;
                        item.SplittedItems = new List<Item>();
                    }

                    foreach (var item in itemsToDelete)
                    {
                        if (item.ParentItemId.HasValue)
                        {
                            if (map.ContainsKey(item.ParentItemId.Value))
                            {
                                map[item.ParentItemId.Value].SplittedItems!.Add(item);
                            }
                        }
                        else
                        {
                            roots.Add(item);
                        }
                    }
                    itemsToDelete = roots;

                    for (int i = 0; i < itemsToDelete.Count; i++)
                    {
                        var item = itemsToDelete[i];
                        //level - 1
                        for (int j = 0; j < item.SplittedItems!.Count; j++)
                        {
                            var splittedItem1 = item.SplittedItems![j];
                            //level - 2
                            for (int k = 0; k < splittedItem1.SplittedItems!.Count; k++)
                            {
                                var splittedItem2 = splittedItem1.SplittedItems![k];
                                if (splittedItem2.SplittedItems!.Count != 0)
                                {
                                    await _unitOfWork.ItemRepository.RemoveRangeAsync(splittedItem2.SplittedItems!);
                                }
                            }
                            if (splittedItem1.SplittedItems!.Count != 0)
                            {
                                await _unitOfWork.ItemRepository.RemoveRangeAsync(splittedItem1.SplittedItems!);
                            }
                        }
                        if (item.SplittedItems!.Count != 0)
                        {
                            await _unitOfWork.ItemRepository.RemoveRangeAsync(item.SplittedItems!);
                        }
                    }
                    await _unitOfWork.ItemRepository.RemoveRangeAsync(itemsToDelete);
                }

                var currentApprovals = invoiceFromDb.InvoiceApprovals.ToList();
                var approvalUpdateList = new List<Approval>();
                foreach (var invoiceApproval in currentApprovals)
                {
                    await _unitOfWork.InvoiceApprovalRepository.RemoveAsync(invoiceApproval);
                    if (invoiceApproval.Approval.Status == ApprovalStatus.Billed)
                    {
                        invoiceApproval.Approval.Status = ApprovalStatus.Pending;
                        approvalUpdateList.Add(invoiceApproval.Approval);
                    }
                }
                await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceFromDb.InvoiceItems.ToList());
                _unitOfWork.ItemRepository.UpdateRange(itemsToUpdate.ToList());
                _unitOfWork.ApprovalRepository.UpdateRange(approvalUpdateList.ToList());
                await _unitOfWork.InvoiceRepository.RemoveAsync(invoiceFromDb);
                _unitOfWork.Save();

                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _response.ErrorMessages.Add("Something is wrong.");
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize]
        [HttpGet("export/excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportExcel()
        {
            var Invoices = await _unitOfWork.InvoiceRepository.GetAllAsync(IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);

            DataTable dt = new DataTable();
            dt.TableName = "Invoices";
            dt.Columns.Add("Invoice No", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Customer Name", typeof(string));
            dt.Columns.Add("Subtotal", typeof(double));
            dt.Columns.Add("Vat @5%", typeof(double));
            dt.Columns.Add("Total Amount", typeof(double));
            dt.Columns.Add("Created/Updated by", typeof(string));
            dt.Columns.Add("Create Date", typeof(string));
            dt.Columns.Add("Update Date", typeof(string));

            foreach (var invoice in Invoices)
            {

                dt.Rows.Add(
                    invoice.InvoiceNo,
                    invoice.Status,
                    invoice.Customer.Name,
                    invoice.TotalAmount,
                    invoice.Vat,
                    invoice.TotalAmountIncludingVat,
                    invoice.Salesman.Name,
                    invoice.CreateDate.ToString("MMMM dd, yyyy"),
                    invoice.UpdateDate.HasValue ? invoice.UpdateDate.Value.ToString("MMMM dd, yyyy") : ""
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet(dt, "Invoices");
                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    var ExcelFile = File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Invoices.xlsx");
                    return ExcelFile;
                }
            }

        }


        [Authorize]
        [HttpGet("export/pdf/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportPDF(int id, [FromHeader] string reciever, [FromHeader] string salesman, [FromHeader] string? terms, [FromHeader] string? remarks)
        {
            Invoice invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == id, IncludeProperties: InvoiceField.Customer + "," + InvoiceField.Salesman);
            if (invoice == null)
            {
                throw new Exception("Invoice not found");
            }


            var invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceItemField.Item);
            if (invoiceItems == null)
            {
                throw new Exception("Items not found");
            }

            var invoiceApprovals = await _unitOfWork.InvoiceApprovalRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceApprovalField.Approval);
            invoice.InvoiceApprovals = invoiceApprovals.ToList();
            invoice.InvoiceItems = invoiceItems.ToList();

            IEnumerable<Item> items = invoiceItems.Select(u => u.Item);

            InvoicePDF invoicePDF = new InvoicePDF()
            {
                Invoice = invoice,
                Items = items.ToList(),
                Reciver = reciever,
                salesman = salesman,
                Terms = terms ?? "",
                Remarks = remarks ?? ""
            };

            var bytePDF = _pdfService.GenerateInvoicePDF(invoicePDF);

            return File(bytePDF, "application/pdf", $"Invoice#{invoice.InvoiceNo}");
        }
    }
}
