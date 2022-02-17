using @enum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PriorityApp.Models.Models.MasterModels;
using PriorityApp.Service.Contracts;
using PriorityApp.Service.Contracts.Comman;
using PriorityApp.Service.Models;
using PriorityApp.Service.Models.MasterModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using PriporityApp_2.Controllers;

namespace PriorityApp.Controllers.CustomerService
{
    [Authorize(Roles = "SuperAdmin, Admin, CustomerService, Sales")]
    public class PickUpOrdersController : BaseController
    {

        private readonly IRegionService _regionService;
        private readonly IStateService _stateService;
        private readonly IOrderService _orderService;
        private readonly ITerritoryService _territoryService;
        private readonly IZoneService _zoneService;
        private readonly IDeliveryCustomerService _deliveryCustomerService;
        private readonly IHoldService _holdService;
        private readonly IItemService _itemService;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<PickUpOrdersController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IExcelService _excelService;
        private readonly IPickUpCustomerService _pickUpCustomerService;

        public PickUpOrdersController(IRegionService regionService,
                                IStateService stateService,
                                IOrderService orderService,
                                ITerritoryService territoryService,
                                IZoneService zoneService,
                                IDeliveryCustomerService deliveryCustomerService,
                                IHoldService holdService,
                                IItemService itemService,
                               UserManager<AspNetUser> userManager,
                                ILogger<PickUpOrdersController> logger,
                                IWebHostEnvironment environment,
                                IConfiguration configuration,
                                IExcelService excelService,
                                IPickUpCustomerService pickUpCustomerService
                                )
        {
            _regionService = regionService;
            _stateService = stateService;
            _orderService = orderService;
            _territoryService = territoryService;
            _zoneService = zoneService;
            _deliveryCustomerService = deliveryCustomerService;
            _holdService = holdService;
            _itemService = itemService;
            _userManager = userManager;
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
            _excelService = excelService;
            _pickUpCustomerService = pickUpCustomerService;
        }
        // GET: PickUpOrdersController
        public ActionResult Index()
        {
            FilterModel filterModel = new FilterModel();
            filterModel.SubRegionSelectedId = -1;
            filterModel.StateSelectedId = -1;
            filterModel.TerritorySelectedId = -1;
            filterModel.ZoneSelectedId = -1;

            AddPickUpOrderModel addPickUpOrderModel = new AddPickUpOrderModel();

            var subRegionModels = _regionService.GetAllISubRegions().Result;
            subRegionModels.Insert(0, new SubRegionModel { Id = -1, Name = "select SubRegion" });
            addPickUpOrderModel.SubRegions = subRegionModels;
            addPickUpOrderModel.SubRegionSelectedId = -1;
            addPickUpOrderModel.SelectedPriorityDate = DateTime.Today;

            return View(addPickUpOrderModel);
        }

        public JsonResult StateFilter(int id)
        {
            var stateModels = new List<StateModel>();
            if (id == -1)
            {
                //stateModels = _stateService.GetAllStates().Result;
                stateModels = null;
            }
            else
            {
                stateModels = _stateService.GetStatesBySubRegionId(id).Result;
                stateModels.Insert(0, new StateModel { Id = -1, Name = "select State" });

            }
            return Json(new SelectList(stateModels, "Id", "Name"));
        }

        public JsonResult TerritoryFilter(int id)
        {
            var territoryModels = new List<TerritoryModel>();
            if (id == -1)
            {
                //stateModels = _stateService.GetAllStates().Result;
                territoryModels = null;
            }
            else
            {
                territoryModels = _territoryService.GetAllTerritoriesByStateId(id).Result;

                territoryModels.Insert(0, new TerritoryModel { Id = -1, Name = "select Territory" });
            }
            return Json(new SelectList(territoryModels, "Id", "Name"));

        }

        public JsonResult ZoneFilter(int id)
        {
            var zoneModels = new List<ZoneModel>();
            if (id == -1)
            {
                //stateModels = _stateService.GetAllStates().Result;
                zoneModels = null;
            }
            else
            {
                zoneModels = _zoneService.GetListOfZonesByTerritoryId(id);

                zoneModels.Insert(0, new ZoneModel { Id = -1, Name = "select Zone" });
            }
            return Json(new SelectList(zoneModels, "Id", "Name"));

        }


        public JsonResult CustomerFilter(int id)
        {
            var customerModels = new List<CustomerModel>();
            if (id == -1)
            {
                //stateModels = _stateService.GetAllStates().Result;
                customerModels = null;

            }
            else
            {
                customerModels = _deliveryCustomerService.GetCutomersByZoneId(id).Result;

                customerModels.Insert(0, new CustomerModel { Id = -1, CustomerName = "select Customer" });
            }
            return Json(new SelectList(customerModels, "Id", "CustomerNamer"));

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchForCustomers(AddPickUpOrderModel Model)
        {
            try
            {
                List<CustomerModel> customerModels = new List<CustomerModel>();
                List<PickUpOrder> pickUpOrders = new List<PickUpOrder>();
                DateTime selectedPriorityDate = Model.SelectedPriorityDate.Date;

                if (Model.ZoneSelectedId != -1)
                {

                    customerModels = _deliveryCustomerService.GetCutomersByZoneId(Model.ZoneSelectedId).Result;
                    Model.Zones = _zoneService.GetListOfZonesByTerritoryId(Model.TerritorySelectedId);
                    Model.Zones.Insert(0, new ZoneModel { Id = -1, Name = "select Zone" });
                }
                else
                {
                    List<ZoneModel> zoneModels = _zoneService.GetListOfZonesByTerritoryId(Model.TerritorySelectedId);
                    List<int> zoneIds = zoneModels.Select(z => z.Id).ToList();

                    customerModels = _deliveryCustomerService.GetCutomersByListOfZoneIds(zoneIds).Result;
                    Model.Zones = zoneModels;
                    Model.Zones.Insert(0, new ZoneModel { Id = -1, Name = "select Zone" });
                }
                foreach (var customer in customerModels)
                {
                    PickUpOrder pickUpOrder = new PickUpOrder();
                    pickUpOrder.Customer = customer;
                    pickUpOrder.PriorityId = 2;
                    pickUpOrder.ItemSelectedId = 1;
                    pickUpOrders.Add(pickUpOrder);
                }
                Model.pickUpOrders = pickUpOrders;

                TerritoryModel territoryModel = _territoryService.GetTerritory(Model.TerritorySelectedId);
                Model.HoldModel = _holdService.GetHold(Model.SelectedPriorityDate.Date, territoryModel.userId);

                Model.SubRegions = _regionService.GetAllISubRegions().Result;

                Model.SubRegions.Insert(0, new Service.Models.MasterModels.SubRegionModel { Id = -2, Name = "Select SubRegion" });
                Model.States = _stateService.GetStatesBySubRegionId(Model.SubRegionSelectedId).Result;
                Model.States.Insert(0, new StateModel { Id = -1, Name = "Select State" });

                Model.Territories = _territoryService.GetAllTerritoriesByStateId(Model.StateSelectedId).Result;
                Model.Territories.Insert(0, new TerritoryModel { Id = -1, Name = "Select Territory" });
                Model.Items = _itemService.GetItemsByType("Bags").Result;
                return View("index", Model);
            }
            catch (Exception e)

            {
                return RedirectToAction("ERROR404");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrders(AddPickUpOrderModel Model)
        {
            AspNetUser applicationUser = await _userManager.GetUserAsync(User);
            try
            {
                bool result = CheckStatisfiedQuantity(Model);
                bool updateOrderResult = false;
                int SavedOrderCount = 0;
                if (result == true)
                {
                    foreach (var pickUpOrder in Model.pickUpOrders.Where(o => o.PriorityId != (int)CommanData.Priorities.No))
                    {
                        OrderModel2 updateModel = new OrderModel2();

                        if (pickUpOrder.OrderNumber != 0)
                        {
                            updateModel = _orderService.GetOrder((long)pickUpOrder.OrderNumber);
                            if (updateModel == null)
                            {
                                updateModel = new OrderModel2();
                                updateModel.OrderNumber = 0;
                            }
                        }
                        else
                        {
                            updateModel.OrderNumber = 0;

                        }

                        updateModel.CustomerId = pickUpOrder.Customer.Id;
                        updateModel.ItemId = (int)pickUpOrder.ItemSelectedId;
                        updateModel.LineID = pickUpOrder.LineID;
                        updateModel.PriorityDate = Model.SelectedPriorityDate;
                        updateModel.PriorityId = pickUpOrder.PriorityId;
                        updateModel.PriorityQuantity = pickUpOrder.PriorityQuantity;
                        updateModel.OrderQuantity = pickUpOrder.OrderQuantity;
                        updateModel.SavedBefore = true;
                        updateModel.WHSavedID = applicationUser.Id;
                        updateModel.Submitted = false;
                        updateModel.Dispatched = false;


                        //updateModel.OrderNumber = orderModel.OrderNumber;


                        Model.HoldModel.ReminingQuantity = Model.HoldModel.ReminingQuantity - (int)pickUpOrder.PriorityQuantity;
                        updateOrderResult = _orderService.CreateOrder(updateModel, Model.HoldModel).Result;
                        if (updateOrderResult == true)
                        {
                            SavedOrderCount = SavedOrderCount + 1;
                        }

                    }
                    TempData["SubmittedOrdersCount"] = SavedOrderCount;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message("Failed Request:There is no enough quantity");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");
            }
            return RedirectToAction("Index");
        }



        public bool CheckStatisfiedQuantity(AddPickUpOrderModel model)
        {
            bool result = false;
            int totalAssignedQuantity = (int)model.pickUpOrders.Where(o => o.PriorityId == (int)CommanData.Priorities.Norm).Select(o => o.PriorityQuantity).Sum();//.Select(o=>o.Sum(o=>o.PriorityQuantity));//.ToList();//.Select(o=>o.pr)
            if (model.HoldModel.ReminingQuantity >= totalAssignedQuantity)
            {
                result = true;

            }
            else
            {
                float minusQuantity = totalAssignedQuantity - model.HoldModel.ReminingQuantity;
            }
            return result;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitOrders()
        {
            AspNetUser applicationUser = _userManager.GetUserAsync(User).Result;
            try
            {
                List<string> roles = (List<string>)_userManager.GetRolesAsync(applicationUser).Result;
                List<OrderModel2> unSubmittedOrders = _orderService.GetAllUnSubmittedOrdersByRole(roles, false).Result;
                List<long> customerIds = unSubmittedOrders.Select(o => o.CustomerId).ToList();
                List<int> zoneIds = _deliveryCustomerService.GetZoneIdsByListOfCustomerIds(customerIds);
                List<int> territoryIds = _zoneService.GetListOfTerritoryIdsByZoneIds(zoneIds);
                var territoryModels = _territoryService.GetAllTeritories().Result.Where(t => territoryIds.Contains(t.Id)).GroupBy(t => t.Id).ToList();
                SubmittInfo info = new SubmittInfo();
                List<SubmittedOrdersTerritories> submittedOrdersTerritories = new List<SubmittedOrdersTerritories>();
                foreach (var territoryModel in territoryModels)
                {
                    SubmittedOrdersTerritories item = new SubmittedOrdersTerritories();
                    item.territorryModel = territoryModel.First();
                    submittedOrdersTerritories.Add(item);
                }
                info.ordersTosubmit = unSubmittedOrders;
                info.submittedOrdersTerritories = submittedOrdersTerritories;
                info.OrdersCount = unSubmittedOrders.Count();

                return View("SubmitView", info);

            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");
            }
            //return null;
        }

        public IActionResult AddPickupOrders()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPickupOrders(IFormFile postedFile)
        {
            try
            {
                string ExcelConnectionString = this._configuration.GetConnectionString("ExcelCon");
                string SqlConnectionString = this._configuration.GetConnectionString("SqlCon");
                bool addResult = false;
                if (postedFile != null)
                {
                    //Create a Folder.
                    string path = Path.Combine(this._environment.WebRootPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    //Save the uploaded Excel file.
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string filePath = Path.Combine(path, fileName);
                    AspNetUser applicationUser = _userManager.GetUserAsync(User).Result;
                    List<string> roles = (List<string>)_userManager.GetRolesAsync(applicationUser).Result;
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    DataTable dt = _excelService.ReadExcelData(filePath, ExcelConnectionString);

                    if (dt.Rows.Count != 0)
                    {
                        DataTable newDT;
                        if (roles.Contains("Sales"))
                        {

                            newDT = _pickUpCustomerService.PreprocessInsertedPickOrders(dt, 8);
                        }
                        else
                        {
                            newDT = _pickUpCustomerService.PreprocessInsertedPickOrders(dt, 6);
                        }
                        string Message = "";
                        if (newDT != null)
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
                            foreach (DataRow row in newDT.Rows)
                            {
                                CustomerModel customerModel = _deliveryCustomerService.GetDeliveryCustomer(Convert.ToInt64(row["CustomerNumber"]));
                                if (roles.Contains("Sales"))
                                {
                                    if (customerModel.zone.Territory.userId == applicationUser.Id)
                                    {
                                        addResult = await AddNewPickUpOrder(row, applicationUser, status, lastSubmitNumber);
                                    }
                                }
                                else
                                {
                                    addResult = await AddNewPickUpOrder(row, applicationUser, status, lastSubmitNumber);
                                }

                            }
                            if (addResult == true)
                            {
                                ViewBag.Message = "your File uploaded successfully";
                                return View();
                            }
                            else
                            {
                                ViewBag.Message = "your File not uploaded";
                                return View();
                            }
                        }
                        //else
                        //{
                        //    ViewBag.Message = "there is an error in your file";
                        //    return View();
                        //}
                    }
                    else
                    {
                        ViewBag.Message = "there is an error in your file";
                        return View();
                    }
                }
                ViewBag.Message = "please select valid file";
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");
            }


        }


        public HoldModel GetHoldQuota(DateTime priorityDate, long customerNumber)
        {

            try
            {
                CustomerModel customerModel = _deliveryCustomerService.GetDeliveryCustomer(customerNumber);
                ZoneModel zoneModel = _zoneService.GetZone(customerModel.ZoneId);
                HoldModel holdModel = _holdService.GetHold(priorityDate, zoneModel.Territory.userId);
                return holdModel;

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;
        }


        public async Task<bool> AddNewPickUpOrder(DataRow row, AspNetUser applicationUser, string status, int lastSubmitNumber)
        {
            DateTime priorityDate = DateTime.Parse(row["PriorityDate"].ToString());
            bool addResult;
            OrderModel2 orderModel2 = new OrderModel2();
            orderModel2.OrderNumber = 0;
            orderModel2.CustomerId = Convert.ToInt64(row["CustomerNumber"]);
            orderModel2.ItemId = Convert.ToInt64(row["ItemNumber"]);
            orderModel2.PriorityDate = priorityDate;
            orderModel2.PriorityQuantity = Convert.ToInt32(row["Quantity"]);
            orderModel2.PriorityId = Convert.ToInt32(row["Priority"]);
            orderModel2.Comment = row["Comment"].ToString();
            orderModel2.WHSavedID = applicationUser.Id;
            orderModel2.SavedBefore = true;
            orderModel2.Submitted = false;
            //orderModel2.WHSubmittedID = applicationUser.Id;
            //orderModel2.SubmitTime = DateTime.Now;
            //orderModel2.Submitted = true;
            //orderModel2.SubmitNumber = lastSubmitNumber;
            //orderModel2.Status = status;
            orderModel2.OrderCategoryId = (int)CommanData.OrderCategory.Pickup;

            if (Convert.ToInt32(row["Priority"]) == (Int32)CommanData.Priorities.Norm)
            {

                HoldModel holdModel = GetHoldQuota(priorityDate, Convert.ToInt64(row["CustomerNumber"]));

                if (holdModel.ReminingQuantity >= Convert.ToInt32(row["Quantity"]))
                {
                    holdModel.ReminingQuantity = holdModel.ReminingQuantity - (int)orderModel2.PriorityQuantity;
                    holdModel.TempReminingQuantity = holdModel.ReminingQuantity;
                    addResult = await _orderService.CreateOrder(orderModel2, holdModel);
                    if (!addResult)
                    {
                        ViewBag.Message = "There is an error in the order with customer number =" + orderModel2.CustomerId + " and item Number = " + orderModel2.ItemId;
                        return false;
                    }

                }
                else
                {
                    ViewBag.Message= "There is no enough quantity";
                    return false;
                }
            }
            else
            {
                addResult = await _orderService.CreateOrder(orderModel2);
                if (!addResult)
                {
                    ViewBag.Message= "There is an error in the order with customer number =" + orderModel2.CustomerId + " and item Number = " + orderModel2.ItemId;
                    return false;
                }
            }
            return true;
        }

        public ActionResult EditPickUpOrders()
        {
            GeoFilterModel geoFilterModel = new GeoFilterModel();
            geoFilterModel = StartData();
            return View(geoFilterModel);
        }


        GeoFilterModel StartData()
        {
            try
            {
                GeoFilterModel geoFilterModel = new GeoFilterModel { };
                List<ItemModel> itemModels = new List<ItemModel>();

                var subRegionModels = _regionService.GetAllISubRegions().Result;
                subRegionModels.Insert(0, new SubRegionModel { Id = -1, Name = "select SubRegion" });
                geoFilterModel.SubRegions = subRegionModels;
                itemModels = _itemService.GetItemsByType("Bags").Result;

                itemModels.Insert(0, new ItemModel { Id = -1, Name = "All" });
                geoFilterModel.Items = itemModels;
                geoFilterModel.ItemSelectedId = -1;

                geoFilterModel.SubRegions = subRegionModels;
                geoFilterModel.SubRegionSelectedId = -1;
                geoFilterModel.SelectedPriorityDate = DateTime.Today;
                return geoFilterModel;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadAddPickUpTemplate(int id)
        {
            try
            {
                AspNetUser applicationUser = _userManager.GetUserAsync(User).Result;
                List<string> roles = (List<string>)_userManager.GetRolesAsync(applicationUser).Result;
                List<ItemModel> items = _itemService.GetItemsByType("Bags").Result.ToList();
                MemoryStream memoryStream;
                if (roles.Contains("Sales"))
                {
                    TerritoryModel territoryModel = _territoryService.GetTerritoryByUserId(applicationUser.Id);

                    List<CustomerModel> customers = _deliveryCustomerService.GetAllDeliveryCustomer().Result.ToList();
                    List<CustomerModel> territoryCustomers = customers.Where(c => c.zone.TerritoryId == territoryModel.Id).ToList();

                    memoryStream = _excelService.WritePickUpTemplateToExcel(items, territoryCustomers);
                }
                else
                {
                    memoryStream = _excelService.WritePickUpTemplateToExcel(items, null);

                }

                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PickupTemplate.xlsx");
            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");
                _logger.LogError(e.ToString());
            }
            //return null;
        }


        // GET: PickUpOrdersController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PickUpOrdersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PickUpOrdersController/Create
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

        // GET: PickUpOrdersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PickUpOrdersController/Edit/5
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

        // GET: PickUpOrdersController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PickUpOrdersController/Delete/5
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
