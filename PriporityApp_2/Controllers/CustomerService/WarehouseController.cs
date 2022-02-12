using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PriorityApp.Models.Models.MasterModels;
using PriorityApp.Service.Contracts;
using PriorityApp.Service.Contracts.Dispatch;
using PriorityApp.Service.Models;
using PriorityApp.Service.Models.MasterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PriorityApp.Service.Contracts.Comman;
using Microsoft.AspNetCore.SignalR;
using @enum;
using PriorityApp.Service.Models.Dispatch;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using PriporityApp_2.Controllers;

namespace PriorityApp.Controllers.CustomerService
{
    [Authorize(Roles = "SuperAdmin, Admin, CustomerService, Sales")]
    public class WarehouseController : BaseController
    {

        public readonly IRegionService _regionService;
        private readonly IStateService _stateService;
        private readonly IOrderService _orderService;
        private readonly ITerritoryService _territoryService;
        private readonly IZoneService _zoneService;
        private readonly IDeliveryCustomerService _deliveryCustomerService;
        private readonly IHoldService _holdService;
        private readonly IItemService _itemService;
        private readonly IPriorityService _priorityService;
        private readonly IWarehouseService _warehouseService;
        private readonly ISubmitNotificationService _submitNotificationService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IExcelService _excelService;
        private readonly ILogger<CSDeliveryOrderController> _logger;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IEmailSender _emailSender;


        public WarehouseController(
                                IRegionService regionService,
                                IStateService stateService,
                                IOrderService orderService,
                                ITerritoryService territoryService,
                                IZoneService zoneService,
                                IDeliveryCustomerService deliveryCustomerService,
                                IHoldService holdService,
                                IItemService itemService,
                                IPriorityService priorityService,
                                IWarehouseService warehouseService,
                                ISubmitNotificationService submitNotificationService,
                                IUserNotificationService userNotificationService,
                               UserManager<AspNetUser> userManager,
                               IExcelService excelService,
                               ILogger<CSDeliveryOrderController> logger,
                               IHubContext<NotificationHub> hub,
                               IEmailSender emailSender)
        {
            _regionService = regionService;
            _stateService = stateService;
            _orderService = orderService;
            _territoryService = territoryService;
            _zoneService = zoneService;
            _deliveryCustomerService = deliveryCustomerService;
            _holdService = holdService;
            _itemService = itemService;
            _priorityService = priorityService;
            _warehouseService = warehouseService;
            _submitNotificationService = submitNotificationService;
            _userNotificationService = userNotificationService;
            _userManager = userManager;
            _excelService = excelService;
            _logger = logger;
            _hub = hub;
        }

        //CSDeliveryOrderController SDeliveryOrderController = new CSDeliveryOrderController(
        //        _regionService,
        //    _stateService,
        //    _orderService,
        //    _territoryService,
        //    _zoneService,
        //    _deliveryCustomerService,
        //    _holdService,
        //    _itemService,
        //    _submitNotificationService,
        //    _userNotificationService,
        //    _userManager,
        //    _excelService,
        //    _logger,
        //    _hub);

        // GET: WarehouseController

        public ActionResult AssignWarehouseOrder()
        {
            try
            {
                WarehouseOrderModel warehouseOrderModel = new WarehouseOrderModel();
                List<ItemModel> itemModels = new List<ItemModel>();
                List<WarehouseModel2> warehouseModel2s = new List<WarehouseModel2>();


                var subRegionModels = _regionService.GetAllISubRegions().Result;
                subRegionModels.Insert(0, new SubRegionModel { Id = -1, Name = "select SubRegion" });
                warehouseOrderModel.SubRegions = subRegionModels;
                itemModels = _itemService.GetAllItems().Result;
                warehouseOrderModel.SubRegionSelectedId = -1;

                warehouseOrderModel.SelectedPriorityDate = DateTime.Today;
                warehouseOrderModel.WarehouseModel2 = warehouseModel2s;
                warehouseOrderModel.Items = itemModels;
                warehouseOrderModel.HoldModel = null;
                return View(warehouseOrderModel);
            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");
                _logger.LogError(e.ToString());
            }
            //return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignWarehouseOrder(WarehouseOrderModel model)
        {
            try
            {
                HoldModel holdModel = _holdService.GetHold(model.SelectedPriorityDate, model.TerritorySelectedId);
                if (holdModel != null)
                {
                    model.HoldModel = holdModel;
                    model.WarehouseModels = _warehouseService.GetAllWarehouse().Result.ToList();
                    List<WarehouseModel2> warehouseModel2s = new List<WarehouseModel2>();
                    for (int index = 0; index < model.WarehouseModels.Count; index++)
                    {
                        WarehouseModel2 warehouseModel2 = new WarehouseModel2();
                        warehouseModel2.itemModels = _itemService.GetAllItems().Result.ToList();
                        warehouseModel2.priorityModels = _priorityService.GetAllPriorities().Result.ToList();
                        warehouseModel2.WarehouseModels = _warehouseService.GetAllWarehouse().Result.ToList();

                        warehouseModel2s.Add(warehouseModel2);
                    }
                    model.WarehouseModel2 = warehouseModel2s;
                    model.Items = _itemService.GetAllItems().Result.ToList();
                    model.SubRegions = _regionService.GetAllISubRegions().Result;

                    model.SubRegions.Insert(0, new Service.Models.MasterModels.SubRegionModel { Id = -2, Name = "Select SubRegion" });
                    model.States = _stateService.GetStatesBySubRegionId(model.SubRegionSelectedId).Result;
                    model.States.Insert(0, new StateModel { Id = -1, Name = "Select State" });

                    model.Territories = _territoryService.GetAllTerritoriesByStateId(model.StateSelectedId).Result;
                    model.Territories.Insert(0, new TerritoryModel { Id = -1, Name = "Select Territory" });

                    return View(model);
                }
                else
                {
                    return View();
                }
            }
            catch(Exception e)
            {
                return RedirectToAction("ERROR404");
            }
            
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrder(WarehouseOrderModel model)
        {
            try
            {
                float totalQuantity = 0;
                string Message = "";
                bool addResult = false;
                bool enoughQuantity = true;
                int submitOderCount = 0;
                AspNetUser applicationUser = await _userManager.GetUserAsync(User);
                OrderModel2 orderModel2 = new OrderModel2();
                var hold = _holdService.GetHold(model.HoldModel.PriorityDate, model.HoldModel.territoryId);
                if (hold != null)
                {
                    int lastSubmitNumber = _orderService.getLastSubmitNumberForToday(DateTime.Now);
                    string status = "";
                    if (lastSubmitNumber >= 1 || lastSubmitNumber == (int)CommanData.NonExistSubmitNumber.ThereOneSubmitForThisPriorityDate)
                    {
                        lastSubmitNumber = lastSubmitNumber + 1;
                        status = "N" + lastSubmitNumber;
                    }
                    else if (lastSubmitNumber == (int)CommanData.NonExistSubmitNumber.NoSubmitBeforeForThisPriorityDate)
                    {
                        status = " ";
                        lastSubmitNumber = 0;
                    }
                    else
                    {
                        Message = "there is an error";
                    }
                    orderModel2.OrderNumber = 0;
                    //orderModel2.CustomerId = model.
                    orderModel2.PriorityDate = model.SelectedPriorityDate;
                    orderModel2.WHSavedID = applicationUser.Id;
                    orderModel2.SavedBefore = true;
                    orderModel2.WHSubmittedID = applicationUser.Id;
                    orderModel2.SubmitTime = DateTime.Now;
                    orderModel2.Submitted = true;
                    orderModel2.SubmitNumber = lastSubmitNumber;
                    orderModel2.Status = status;
                    orderModel2.OrderCategoryId = (int)CommanData.OrderCategory.Warehouse;
                    foreach (var warehouseOrder in model.WarehouseModel2.Where(w=>w.PrioritySelectedId != 2))
                     {
                        orderModel2.PriorityId = warehouseOrder.PrioritySelectedId;
                        orderModel2.Comment = warehouseOrder.Comment;
                        orderModel2.CustomerId = warehouseOrder.WarehouseSelectedId;
                        if (warehouseOrder.PrioritySelectedId != 4)
                        {
                            totalQuantity = warehouseOrder.itemModels.Sum(w => w.Quantity);
                            if (totalQuantity > hold.ReminingQuantity)
                            {
                                enoughQuantity = false;
                            }
                        }
                        if(enoughQuantity)
                        {
                            foreach (var item in warehouseOrder.itemModels.Where(i => i.Quantity != 0))
                            {
                                //if(item.Quantity != 0)
                                //{
                                orderModel2.ItemId = item.Id;
                                orderModel2.PriorityQuantity = item.Quantity;
                                hold.ReminingQuantity = hold.ReminingQuantity - item.Quantity;
                                hold.TempReminingQuantity = hold.TempReminingQuantity - item.Quantity;
                                addResult = await _orderService.CreateOrder(orderModel2, hold);
                                submitOderCount = addResult == true ? submitOderCount++ : submitOderCount;
                            }
                        }     
                        
                    }
                    if (addResult)
                    {
                        SubmitNotificationModel submitNotificationModel = new SubmitNotificationModel();
                        submitNotificationModel.NumberOfSubmittedOrders = submitOderCount;
                        submitNotificationModel.Message = "There are " + submitOderCount + " new submitted order";
                        submitNotificationModel.CreatedDate = DateTime.Now;
                        submitNotificationModel.UpdatedDate = DateTime.Now;
                        submitNotificationModel.Seen = false;
                        SubmitNotificationModel NewsubmitNotificationModel = _submitNotificationService.CreateSubmitNotification(submitNotificationModel);
                        if (NewsubmitNotificationModel != null)
                        {
                            List<UserNotificationModel> userNotificationModels = new List<UserNotificationModel>();
                            List<AspNetUser> users = _userManager.GetUsersInRoleAsync("Dispatch").Result.ToList();
                            foreach (var user in users)
                            {
                                UserNotificationModel userNotificationModel = new UserNotificationModel();
                                userNotificationModel.submitNotificationId = NewsubmitNotificationModel.Id;
                                userNotificationModel.Seen = false;
                                userNotificationModel.userId = user.Id;
                                userNotificationModels.Add(userNotificationModel);
                            }
                            await _userNotificationService.CreateUserNotification(userNotificationModels);
                            //foreach (var id in submittedOrderIdsList)
                            //{
                            //    OrderNotificationModel orderNotificationModel = new OrderNotificationModel();
                            //    orderNotificationModel.submitNotificationId = NewsubmitNotificationModel.Id;
                            //    orderNotificationModel.OrderId = id;
                            //    orderNotificationModel.CreatedDate = DateTime.Now;
                            //    orderNotificationModel.UpdatedDate = DateTime.Now;
                            //    bool addOrderNotification = _submitNotificationService.CreateOrderNotification(orderNotificationModel).Result;

                            //}
                        }
                        //List<SubmitNotificationModel> submitNotificationModels = _submitNotificationService.GetUnseenNotifications();
                        await _hub.Clients.All.SendAsync("SubmitNotification", "There are new submitted  orders", 1, NewsubmitNotificationModel.Id);
                        var testMail = await Send("doaa.abdel@ext.cemex.com");
                    }

                }
                return View("AssignWarehouseOrder");
            }
            catch(Exception e)
            {
                return RedirectToAction("ERROR404");
            }
           // return View("AssignWarehouseOrder");
        }
        public async Task<IActionResult> Send(string toAddress)
        {
            var subject = "test mail";
            var body = "new order submit";
            await _emailSender.SendEmailAsync(toAddress, subject, body);
            return View();
        }




















        public ActionResult Index()
        {
            return View();
        }

        // GET: WarehouseController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: WarehouseController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: WarehouseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WarehouseController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: WarehouseController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WarehouseController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: WarehouseController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
