using AutoMapper;
using ClosedXML.Excel;
using CRM.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using InvoiceGenerator.StaticData.TableFields;
using InvoiceGenerator.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net;
using System.Reflection;
using InvoiceGenerator.Utility.PDFService;
using InvoiceGenrator.Model.Models;
using InvoiceGenerator.StaticData.Status;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/approval")]
    [ApiController]
    public class ApprovalController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPDFService _pdfService;

        public ApprovalController(IUnitOfWork unitOfWork, IMapper mapper, IConverter converter, IWebHostEnvironment webHostEnvironment, IPDFService pdfService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
            _converter = converter;
            _webHostEnvironment = webHostEnvironment;
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

                var approval = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == id, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman + "," + ApprovalField.InvoiceApprovals);
                if (approval == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Approval not found.");
                    return _response;
                }


                ApprovalResponseDTO approvalResponseDTO = _mapper.Map<ApprovalResponseDTO>(approval);

                _response.Data = approvalResponseDTO;
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
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] string? status, [FromQuery] List<string>? statusToExclude, [FromQuery] int? customerId, [FromQuery] int? invoiceId, [FromQuery] string? createDateMiliSecondsString, [FromQuery] string? search, [FromQuery] string? orderBy, [FromQuery] string? order = Order.ASC, [FromQuery] int PageSize = 0, [FromQuery] int PageNo = 1)
        {
            try
            {
                IEnumerable<Approval> approvals = new List<Approval>();
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
                    recordsResponse = await _unitOfWork.ApprovalRepository.GetAllWithSearchAndFilterAsync(status: status, statusToExclude: statusToExclude, invoiceId: invoiceId, customerId: customerId, createDate: createDate, search: search, PageSize: PageSize, PageNo: PageNo, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
                }
                else if (string.IsNullOrEmpty(order) || order == "null" || order == Order.ASC)
                {
                    /*order ASC*/
                    if (orderBy != ApprovalField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.ApprovalRepository.GetAllWithSearchAndFilterAsync(status: status, statusToExclude: statusToExclude, invoiceId: invoiceId, customerId: customerId, createDate: createDate, search: search, OrderBy: _unitOfWork.ApprovalRepository.CreateSelectorExpression(orderBy), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.ApprovalRepository.GetAllWithSearchAndFilterAsync(status: status, statusToExclude: statusToExclude, invoiceId: invoiceId, customerId: customerId, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
                    }
                }
                else
                {
                    /*order DSE*/
                    if (orderBy != ApprovalField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.ApprovalRepository.GetAllWithSearchAndFilterAsync(status: status, statusToExclude: statusToExclude, invoiceId: invoiceId, customerId: customerId, createDate: createDate, search: search, OrderBy: _unitOfWork.ApprovalRepository.CreateSelectorExpression(orderBy), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.ApprovalRepository.GetAllWithSearchAndFilterAsync(status: status, statusToExclude: statusToExclude, invoiceId: invoiceId, customerId: customerId, createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
                    }
                }

                Pagination pagination = new Pagination { PageNo = PageNo, PageSize = PageSize };
                Response.Headers.Append("x-pagination", JsonConvert.SerializeObject(pagination));
                approvals = recordsResponse.Records;

                IEnumerable<ApprovalResponseDTO> ApprovalResponseDTO = approvals.Select(entity => _mapper.Map<ApprovalResponseDTO>(entity));

                _response.Data = new RecordsResponse

                {
                    TotalRecords = recordsResponse.TotalRecords,
                    Records = ApprovalResponseDTO
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
        public async Task<ActionResult<APIResponse>> Create(ApprovalCreateDTO approvalCreateDTO)
        {
            try
            {
                if (approvalCreateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Approval info not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                Approval approval = _mapper.Map<Approval>(approvalCreateDTO);
                approval = await _unitOfWork.ApprovalRepository.CreateAsync(approval);
                approval.ApprovalNo = approval.Id.ToString().PadLeft(5, '0');
                _unitOfWork.ApprovalRepository.Update(approval);
                _unitOfWork.Save();

                ApprovalResponseDTO approvalResponse = _mapper.Map<ApprovalResponseDTO>(approval);
                _response.Data = approvalResponse;
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
        public async Task<ActionResult<APIResponse>> Update(ApprovalUpdateDTO approvalUpdateDTO)
        {
            try
            {
                if (approvalUpdateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Approval not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                var approvalFromDb = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == approvalUpdateDTO.Id);
                if (approvalFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Approval not found.");
                    return _response;
                }

                if (approvalFromDb.Status == ApprovalStatus.Billed)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("You can't update billed approval");
                    return _response;
                }

                approvalFromDb.TotalAmount = approvalUpdateDTO.TotalAmount;
                approvalFromDb.Vat = approvalUpdateDTO.Vat;
                approvalFromDb.TotalAmountIncludingVat = approvalUpdateDTO.TotalAmountIncludingVat;
                approvalFromDb.SalesmanId = approvalUpdateDTO.SalesmanId;
                approvalFromDb.CustomerId = approvalUpdateDTO.CustomerId;
                approvalFromDb.Status = approvalUpdateDTO.Status;
                approvalFromDb.UpdateDate = approvalUpdateDTO.UpdateDate;

                _unitOfWork.ApprovalRepository.Update(approvalFromDb);

                ApprovalResponseDTO approvalResponse = _mapper.Map<ApprovalResponseDTO>(approvalFromDb);
                _response.Data = approvalResponse;
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
        [HttpPut("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateRange(List<Approval> approvals)
        {
            async Task<Approval> GetApprovalWithStatusAndTotal(Approval approval)
            {
                var approvalStatus = ApprovalStatus.Returned;
                var isPending = false;
                var isBilled = false;


                RecordsResponse response = await _unitOfWork.ItemRepository.GetAllWithSearchAndFilterAsync(approvalId: approval.Id, statusToExclude: [ItemStatus.Splitted]);
                List<Item> items = response.Records;


                //get status
                foreach (Item item in items)
                {
                    if (item.Status == ItemStatus.Billed)
                    {
                        isBilled = true;
                    }
                    if (item.Status == ItemStatus.Pending)
                    {
                        isPending = true;
                    }
                }

                if (isPending)
                {
                    approvalStatus = ApprovalStatus.Pending;
                }
                else if (isBilled)
                {
                    approvalStatus = ApprovalStatus.Billed;
                }


                //Get total
                double total = 0;
                if (approvalStatus != ApprovalStatus.Returned)
                {
                    foreach (Item item in items)
                    {
                        if (item.Status != ItemStatus.Returned)
                        {
                            total += item.Amount;
                        }
                    }
                }
                else
                {
                    foreach (Item item in items)
                    {
                        total += item.Amount;
                    }
                }

                approval.Status = approvalStatus;
                approval.TotalAmount = Math.Round(total, 2);
                approval.Vat = Math.Round(((approval.TotalAmount * Vat.VatRate) / 100), 2);
                approval.TotalAmountIncludingVat = approval.TotalAmount + approval.Vat;
                approval.UpdateDate = DateTime.UtcNow;
                approval.InvoiceApprovals = [];

                return approval;
            }

            try
            {
                if (approvals == null || !approvals.Any())
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Approvals list not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                //List<Approval> approvals = approvalUpdateDTOs.Select(entity => _mapper.Map<Approval>(entity)).ToList();
                List<Approval> approvalsToUpdate = new List<Approval>();

                foreach (Approval approval in approvals)
                {
                    approvalsToUpdate.Add(await GetApprovalWithStatusAndTotal(approval));
                }

                _unitOfWork.ApprovalRepository.UpdateRange(approvalsToUpdate);
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

                var approvalFromDb = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == id);
                if (approvalFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Approval not found.");
                    return _response;
                }

                if (approvalFromDb.Status == ApprovalStatus.Billed)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("You can't delete billed approval.");
                    return _response;
                }


                var items = (await _unitOfWork.ItemRepository.GetAllAsync(u => u.ApprovalId == approvalFromDb.Id)).ToList();
                if (items != null && items.Any())
                {
                    var map = new Dictionary<int, Item>();
                    var roots = new List<Item>();
                    foreach (var item in items)
                    {
                        map[item.Id] = item;
                        item.SplittedItems = new List<Item>();
                    }

                    foreach (var item in items)
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
                    items = roots;

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
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
                    await _unitOfWork.ItemRepository.RemoveRangeAsync(items);
                }
                await _unitOfWork.ApprovalRepository.RemoveAsync(approvalFromDb);
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
            var approvals = await _unitOfWork.ApprovalRepository.GetAllAsync(IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);

            DataTable dt = new DataTable();
            dt.TableName = "Approvals";
            dt.Columns.Add("Approval No", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Customer Name", typeof(string));
            dt.Columns.Add("Subtotal", typeof(double));
            dt.Columns.Add("Vat @5%", typeof(double));
            dt.Columns.Add("Total Amount", typeof(double));
            dt.Columns.Add("Created/Updated by", typeof(string));
            dt.Columns.Add("Create Date", typeof(string));
            dt.Columns.Add("Update Date", typeof(string));

            foreach (var approval in approvals)
            {

                dt.Rows.Add(
                    approval.ApprovalNo,
                    approval.Status,
                    approval.Customer.Name,
                    approval.TotalAmount,
                    approval.Vat,
                    approval.TotalAmountIncludingVat,
                    approval.Salesman.Name,
                    approval.CreateDate.ToString("MMMM dd, yyyy"),
                    approval.UpdateDate.HasValue ? approval.UpdateDate.Value.ToString("MMMM dd, yyyy") : ""
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet(dt, "Approvals");
                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    var ExcelFile = File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Approvals.xlsx");
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
            Approval approval = await _unitOfWork.ApprovalRepository.GetAsync(u => u.Id == id, IncludeProperties: ApprovalField.Customer + "," + ApprovalField.Salesman);
            if (approval == null)
            {
                throw new Exception("Approval not found");
            }

            var items = await _unitOfWork.ItemRepository.GetAllAsync(u => u.ApprovalId == id);
            if (items == null)
            {
                throw new Exception("Items not found");
            }

            ApprovalPDF approvalPDF = new ApprovalPDF()
            {
                Approval = approval,
                Items = items.ToList(),
                Reciver = reciever,
                salesman = salesman,
                Terms = terms ?? "",
                Remarks = remarks ?? ""
            };

            var bytePDF = _pdfService.GenerateApprovalPDF(approvalPDF);

            return File(bytePDF, "application/pdf", $"Approval#{approval.ApprovalNo}");
        }

    }
}
