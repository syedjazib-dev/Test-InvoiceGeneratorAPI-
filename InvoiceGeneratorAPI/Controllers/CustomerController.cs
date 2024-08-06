using AutoMapper;
using Azure;
using ClosedXML.Excel;
using CRM.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData;
using InvoiceGenerator.StaticData.TableFields;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;
        private APIResponse _response;
        private readonly IMapper _mapper;

        public CustomerController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
        }


        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] string? createDateMiliSecondsString, [FromQuery] string? search, [FromQuery] string? orderBy, [FromQuery] string? order = Order.ASC, [FromQuery] int PageSize = 0, [FromQuery] int PageNo = 1)
        {
            try
            {
                IEnumerable<Customer> customers = new List<Customer>();
                RecordsResponse recordsResponse = new RecordsResponse()
                {
                    TotalRecords = 0,
                    Records = new List<Customer>()
                };

                DateTime? createDate = null;
                if (!string.IsNullOrEmpty(createDateMiliSecondsString))
                {
                    createDate = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(createDateMiliSecondsString)).Date;
                }

                if (string.IsNullOrEmpty(orderBy) || orderBy == "null")
                {
                    recordsResponse = await _unitOfWork.CustomerRepository.GetAllWithSearchAndFilterAsync(createDate: createDate, search: search, PageSize: PageSize, PageNo: PageNo);
                }
                else if (string.IsNullOrEmpty(order) || order == "null" || order == Order.ASC)
                {
                    /*order ASC*/
                    if (orderBy != CustomerField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.CustomerRepository.GetAllWithSearchAndFilterAsync(createDate: createDate, search: search, OrderBy: _unitOfWork.CustomerRepository.CreateSelectorExpression(orderBy), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.CustomerRepository.GetAllWithSearchAndFilterAsync(createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.ASC, PageSize: PageSize, PageNo: PageNo);
                    }
                }
                else
                {
                    /*order DSE*/
                    if (orderBy != CustomerField.CreateDate)
                    {
                        recordsResponse = await _unitOfWork.CustomerRepository.GetAllWithSearchAndFilterAsync(createDate: createDate, search: search, OrderBy: _unitOfWork.CustomerRepository.CreateSelectorExpression(orderBy), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo);
                    }
                    else
                    {
                        recordsResponse = await _unitOfWork.CustomerRepository.GetAllWithSearchAndFilterAsync(createDate: createDate, search: search, OrderBy: u => u.CreateDate.ToString(), Order: Order.DESC, PageSize: PageSize, PageNo: PageNo);
                    }
                }

                Pagination pagination = new Pagination { PageNo = PageNo, PageSize = PageSize };
                Response.Headers.Append("x-pagination", JsonConvert.SerializeObject(pagination));
                customers = recordsResponse.Records;

                IEnumerable<CustomerResponseDTO> customerResponseDTO = customers.Select(entity => _mapper.Map<CustomerResponseDTO>(entity));

                _response.Data = new RecordsResponse
                {
                    TotalRecords = recordsResponse.TotalRecords,
                    Records = customerResponseDTO
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
        public async Task<ActionResult<APIResponse>> Create(CustomerCreateDTO customerCreateDTO)
        {
            try
            {
                if (customerCreateDTO == null) {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Customer info not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                Customer customer = _mapper.Map<Customer>(customerCreateDTO);
                await _unitOfWork.CustomerRepository.CreateAsync(customer);

                CustomerResponseDTO customerResponse = _mapper.Map<CustomerResponseDTO>(customer);
                _response.Data = customerResponse;
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
        public async Task<ActionResult<APIResponse>> Update(CustomerUpdateDTO customerUpdateDTO)
        {
            try
            {
                if (customerUpdateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Update Info not Provided.");
                    return _response;
                }



                var customerFromDb = await _unitOfWork.CustomerRepository.GetAsync(u => u.Id == customerUpdateDTO.Id);
                if (customerFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Customer not found.");
                    return _response;
                }

                customerFromDb.Name = customerUpdateDTO.Name;
                customerFromDb.UpdateDate = customerUpdateDTO.UpdateDate;
                customerFromDb.Email = customerUpdateDTO.Email;
                customerFromDb.TRN = customerUpdateDTO.TRN;
                customerFromDb.Phone = customerUpdateDTO.Phone;
                customerFromDb.Address = customerUpdateDTO.Address;


                _unitOfWork.CustomerRepository.Update(customerFromDb);
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

                var customerFromDb = await _unitOfWork.CustomerRepository.GetAsync(u => u.Id == id);
                if (customerFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Customer not found.");
                    return _response;
                }

                var approvals = await _unitOfWork.ApprovalRepository.GetAllAsync(u=>u.CustomerId == id);
                if(approvals != null && approvals.Any())
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Customer have associated approvals, It can't be delete.");
                    return _response;
                }

                var invoices = await _unitOfWork.InvoiceRepository.GetAllAsync(u => u.CustomerId == id);
                if (invoices != null && invoices.Any())
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Customer have associated Invoices, It can't be delete.");
                    return _response;
                }


                await _unitOfWork.CustomerRepository.RemoveAsync(customerFromDb);
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
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync();

            DataTable dt = new DataTable();
            dt.TableName = "Customers";
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("TRN", typeof(string));
            dt.Columns.Add("Update Date", typeof(string));

            foreach (var customer in customers)
            {

                dt.Rows.Add(customer.Name, customer.Email,  customer.CreateDate.ToString("MMMM dd, yyyy"), customer.Address, customer.TRN,
                    customer.UpdateDate.HasValue ? customer.UpdateDate.Value.ToString("MMMM dd, yyyy") : ""
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet(dt, "Customers");
                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    var ExcelFile = File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customers.xlsx");
                    return ExcelFile;
                }
            }

        }
    }
}
