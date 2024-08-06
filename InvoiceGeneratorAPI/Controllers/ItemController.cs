using AutoMapper;
using CRM.Models;
using InvoiceGenerator.DataAccess.Repository.IRepositoy;
using InvoiceGenerator.Model.Models;
using InvoiceGenerator.Model.Models.DTOs;
using InvoiceGenerator.StaticData.Status;
using InvoiceGenerator.StaticData.TableFields;
using InvoiceGenrator.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InvoiceGenratorAPI.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private APIResponse _response;
        private readonly IMapper _mapper;

        public ItemController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
        }


        [Authorize]
        [HttpGet("approval/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetAllByApproval(int id, [FromQuery] List<string>? statusToExclude, [FromQuery] int? invoiceId)
        {
            try
            {
                if (id == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Approval id not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                RecordsResponse recordsResponse = await _unitOfWork.ItemRepository.GetAllWithSearchAndFilterAsync(approvalId: id, statusToExclude: statusToExclude);
                IEnumerable<Item> items = recordsResponse.Records;

                IEnumerable<ItemResponseDTO> itemResponses = items.Select(entity => _mapper.Map<ItemResponseDTO>(entity));
                List<ItemResponseDTO> itemResponseDTOs = new List<ItemResponseDTO>();

                foreach (ItemResponseDTO item in itemResponses)
                {
                        IEnumerable<InvoiceItem> invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == item.Id, IncludeProperties: InvoiceItemField.Invoice);
                        if (!invoiceItems.Any())
                        {
                            itemResponseDTOs.Add(item);
                            continue;
                        }
                        if (invoiceId != null)
                        {
                            var flag = false;
                            foreach (var invoiceItem in invoiceItems)
                            {
                                if (invoiceItem.InvoiceId == invoiceId || item.Status == ItemStatus.Pending)
                                {
                                    flag = true; break;
                                }
                            }
                            if (flag)
                            {
                                item.InvoiceItems = invoiceItems.ToList();
                                itemResponseDTOs.Add(item);
                            }
                        }
                        else
                        {
                            item.InvoiceItems = invoiceItems.ToList();
                            itemResponseDTOs.Add(item);
                        }
                }
                _response.Data = itemResponseDTOs;
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
        [HttpGet("invoice/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetAllByInvoice(int id)
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

                IEnumerable<InvoiceItem> invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.InvoiceId == id, IncludeProperties: InvoiceItemField.Item);
                List<Item> items = new List<Item>();
                foreach (InvoiceItem invoiceItem in invoiceItems)
                {
                    items.Add(invoiceItem.Item);
                }

                foreach (Item item in items)
                {
                    var InvoiceItemsByItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == item.Id, IncludeProperties: InvoiceItemField.Invoice);
                    foreach (var invoiceItem in InvoiceItemsByItems)
                    {

                        invoiceItem.Item = null;
                        invoiceItem.Invoice.InvoiceItems = [];
                    }
                    item.InvoiceItems = InvoiceItemsByItems.ToList();
                }

                IEnumerable<ItemResponseDTO> itemResponseDTOs = items.Select(entity => _mapper.Map<ItemResponseDTO>(entity));
                _response.Data = itemResponseDTOs;
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
        public async Task<ActionResult<APIResponse>> Create(ItemCreateDTO itemCreateDTO)
        {
            try
            {
                if (itemCreateDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Item info not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }


                Item item = _mapper.Map<Item>(itemCreateDTO);
                var itemFromDb = await _unitOfWork.ItemRepository.CreateAsync(item);


                if (itemCreateDTO.NewInvoiceIds != null && itemCreateDTO.NewInvoiceIds.Any())
                {
                    var invoiceItemToCreate = new List<InvoiceItem>();
                    foreach (var invoiceId in itemCreateDTO.NewInvoiceIds)
                    {
                        var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceId);
                        if (invoice != null)
                        {
                            invoiceItemToCreate.Add(new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                ItemId = itemFromDb.Id,
                            });
                        }
                    }
                    if (invoiceItemToCreate.Any())
                    {
                        await _unitOfWork.InvoiceItemRepository.CreateRangeAsync(invoiceItemToCreate);
                    }
                }

                ItemResponseDTO response = _mapper.Map<ItemResponseDTO>(item);
                _response.Data = response;
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
        [HttpPost("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> CreateRange(List<ItemCreateDTO> itemCreateDTOList)
        {
            try
            {
                if (itemCreateDTOList == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Items info not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                var invoiceItemToCreate = new List<InvoiceItem>();
                IEnumerable<Item> items = itemCreateDTOList.Select(entity => _mapper.Map<Item>(entity));
                List<Item> itemList = items.ToList();
                List<Item> DBItemList = await _unitOfWork.ItemRepository.CreateRangeAsync(itemList);
                foreach (Item item in DBItemList)
                {
                    if (item.NewInvoiceIds != null && item.NewInvoiceIds.Any())
                    {
                        foreach (var invoiceId in item.NewInvoiceIds)
                        {
                            var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceId);
                            if (invoice != null)
                            {
                                invoiceItemToCreate.Add(new InvoiceItem
                                {
                                    InvoiceId = invoiceId,
                                    ItemId = item.Id,
                                });
                            }
                        }

                    }
                }
                //level - 0
                for (int i = 0; i < itemList.Count; i++)
                {
                    //level - 1
                    if (itemList[i].Status == ItemStatus.Splitted)
                    {
                        List<Item> splittedItemList1 = itemList[i].SplittedItems!;
                        foreach (Item item in splittedItemList1)
                        {
                            item.ParentItemId = DBItemList[i].Id;
                            item.CreateDate = DateTime.UtcNow;
                        }
                        List<Item> DBSplitedItemList1 = await _unitOfWork.ItemRepository.CreateRangeAsync(splittedItemList1);

                        //itemInvoice - level - 1
                        foreach (Item item in DBSplitedItemList1)
                        {
                            if (item.NewInvoiceIds != null && item.NewInvoiceIds.Any())
                            {
                                foreach (var invoiceId in item.NewInvoiceIds)
                                {
                                    var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceId);
                                    if (invoice != null)
                                    {
                                        invoiceItemToCreate.Add(new InvoiceItem
                                        {
                                            InvoiceId = invoiceId,
                                            ItemId = item.Id,
                                        });
                                    }
                                }

                            }
                        }
                        for (int j = 0; j < splittedItemList1.Count; j++)
                        {
                            //level - 2
                            if (splittedItemList1[j].Status == ItemStatus.Splitted)
                            {
                                List<Item> splittedItemList2 = splittedItemList1[j].SplittedItems!;
                                foreach (Item item in splittedItemList2)
                                {
                                    item.ParentItemId = DBSplitedItemList1[j].Id;
                                    item.CreateDate = DateTime.UtcNow;
                                }
                                List<Item> DBSplitedItemList2 = await _unitOfWork.ItemRepository.CreateRangeAsync(splittedItemList2);
                                //itemInvoice - level - 2
                                foreach (Item item in DBSplitedItemList2)
                                {
                                    if (item.NewInvoiceIds != null && item.NewInvoiceIds.Any())
                                    {
                                        foreach (var invoiceId in item.NewInvoiceIds)
                                        {
                                            var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceId);
                                            if (invoice != null)
                                            {
                                                invoiceItemToCreate.Add(new InvoiceItem
                                                {
                                                    InvoiceId = invoiceId,
                                                    ItemId = item.Id,
                                                });
                                            }
                                        }

                                    }
                                }
                                for (int k = 0; k < splittedItemList2.Count; k++)
                                {
                                    //level - 3
                                    if (splittedItemList2[k].Status == ItemStatus.Splitted)
                                    {
                                        List<Item> splittedItemList3 = splittedItemList2[k].SplittedItems!;
                                        foreach (Item item in splittedItemList3)
                                        {
                                            item.ParentItemId = DBSplitedItemList2[k].Id;
                                            item.CreateDate = DateTime.UtcNow;
                                        }

                                        List<Item> DBSplitedItemList3 = await _unitOfWork.ItemRepository.CreateRangeAsync(splittedItemList3);
                                        //itemInvoice - level - 3
                                        foreach (Item item in DBSplitedItemList3)
                                        {
                                            if (item.NewInvoiceIds != null && item.NewInvoiceIds.Any())
                                            {
                                                foreach (var invoiceId in item.NewInvoiceIds)
                                                {
                                                    var invoice = await _unitOfWork.InvoiceRepository.GetAsync(u => u.Id == invoiceId);
                                                    if (invoice != null)
                                                    {
                                                        invoiceItemToCreate.Add(new InvoiceItem
                                                        {
                                                            InvoiceId = invoiceId,
                                                            ItemId = item.Id,
                                                        });
                                                    }
                                                }

                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }

                if (invoiceItemToCreate.Any())
                {
                    await _unitOfWork.InvoiceItemRepository.CreateRangeAsync(invoiceItemToCreate);
                }
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
        [HttpDelete("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeleteRange(List<Item> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Items list not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    IEnumerable<InvoiceItem> invoiceItem = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == item.Id);
                    if (invoiceItem != null)
                    {
                        await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceItem.ToList());
                    }
                    //level - 1
                    if (item.SplittedItems != null && item.SplittedItems.Count > 0)
                    {
                        for (int j = 0; j < item.SplittedItems!.Count; j++)
                        {
                            var splittedItem1 = item.SplittedItems![j];

                            IEnumerable<InvoiceItem> invoiceItem1 = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == splittedItem1.Id);
                            if (invoiceItem1 != null)
                            {
                                await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceItem1.ToList());
                            }
                            if (splittedItem1.SplittedItems != null && splittedItem1.SplittedItems.Count > 0)
                            {
                                //level - 2
                                for (int k = 0; k < splittedItem1.SplittedItems!.Count; k++)
                                {
                                    var splittedItem2 = splittedItem1.SplittedItems![k];

                                    IEnumerable<InvoiceItem> invoiceItem2 = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == splittedItem2.Id);
                                    if (invoiceItem2 != null)
                                    {
                                        await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceItem2.ToList());
                                    }
                                    if (splittedItem2.SplittedItems != null && splittedItem2.SplittedItems.Count > 0)
                                    {
                                        foreach (var splittedItem3 in splittedItem2.SplittedItems!)
                                        {
                                            IEnumerable<InvoiceItem> invoiceItem3 = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == splittedItem3.Id);
                                            if (invoiceItem3 != null)
                                            {
                                                await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceItem3.ToList());
                                            }
                                        }
                                        await _unitOfWork.ItemRepository.RemoveRangeAsync(splittedItem2.SplittedItems!);
                                    }
                                }
                                await _unitOfWork.ItemRepository.RemoveRangeAsync(splittedItem1.SplittedItems!);

                            }
                        }
                        await _unitOfWork.ItemRepository.RemoveRangeAsync(item.SplittedItems!);

                    }
                }
                await _unitOfWork.ItemRepository.RemoveRangeAsync(items);
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
        public async Task<ActionResult<APIResponse>> UpdateRange(List<ItemUpdateDTO> itemUpdateDTOs)
        {
            try
            {
                if (itemUpdateDTOs == null || !itemUpdateDTOs.Any())
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Items list not provided.");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                var invoiceItemToCreate = new List<InvoiceItem>();
                var invoiceItemToDelete = new List<InvoiceItem>();
                IEnumerable<Item> items = itemUpdateDTOs.Select(entity => _mapper.Map<Item>(entity));
                foreach (var item in items)
                {
                    IEnumerable<InvoiceItem> invoiceItems = await _unitOfWork.InvoiceItemRepository.GetAllAsync(u => u.ItemId == item.Id);
                    if (invoiceItems.Any())
                    {
                        invoiceItemToDelete.AddRange(invoiceItems);
                    }

                    if (item.NewInvoiceIds != null && item.NewInvoiceIds.Count != 0)
                    {
                        foreach (var invoiceId in item.NewInvoiceIds)
                        {
                            invoiceItemToCreate.Add(new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                ItemId = item.Id,
                            });
                        }
                    }
                }
                await _unitOfWork.InvoiceItemRepository.RemoveRangeAsync(invoiceItemToDelete);
                _unitOfWork.ItemRepository.UpdateRange(items.ToList());
                await _unitOfWork.InvoiceItemRepository.CreateRangeAsync(invoiceItemToCreate);
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


    }
}
