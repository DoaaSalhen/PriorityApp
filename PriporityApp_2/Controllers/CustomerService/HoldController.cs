using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PriorityApp.Service.Contracts;
using PriorityApp.Service.Contracts.Comman;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using PriorityApp.Service.Models;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using PriporityApp_2.Controllers;

namespace PriorityApp.Controllers.CustomerService
{

    public class HoldController : BaseController
    {

        private readonly ILogger<HoldController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IHoldService _holdService;
        private readonly IExcelService _excelService;

        public HoldController(ILogger<HoldController> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IHoldService holdService,
            IExcelService excelService)
        {
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
            _holdService = holdService;
            _excelService = excelService;
        }
        [Authorize(Roles = "SuperAdmin, Admin, CustomerService, Sales")]
        // GET: HoldController
        public ActionResult Index()
        {
            
            return View();
        }
        [Authorize(Roles = "SuperAdmin, Admin, CustomerService, Sales")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(params DateTime[] priorityDate)
        {
            List<HoldModel> holdmodels = new List<HoldModel>();
            HoldModel holdModel = new HoldModel();
            if (ViewData["SearchPriorityDate"] != null)
            {
                holdmodels = _holdService.GetHoldBypriorityDate((DateTime)ViewData["SearchPriorityDate"]);
                holdModel.holdModels = holdmodels;
                holdModel.PriorityDate = (DateTime) ViewData["SearchPriorityDate"];

            }
            else
            {
                holdmodels = _holdService.GetHoldBypriorityDate(priorityDate[0]);
                holdModel.holdModels = holdmodels;
                holdModel.PriorityDate = priorityDate[0];
            }

            return View("index", holdModel);
        }

        // GET: HoldController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HoldController/Create
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "SuperAdmin, Admin, CustomerService")]
        // POST: HoldController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormFile postedFile)
        {
            try
            {
                string ExcelConnectionString = this._configuration.GetConnectionString("ExcelCon");
                string SqlConnectionString = this._configuration.GetConnectionString("SqlCon");

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
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }
                    DataTable dt = _excelService.ReadExcelData(filePath, ExcelConnectionString);
                    
                    dt = _holdService.prepareDataForHold(dt);
                    if(dt != null)
                    {
                        bool result = _holdService.AddQuotaFile(dt, SqlConnectionString);

                        if (result == true)
                        {
                            return RedirectToAction("index");
                        }
                    }

                }
                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("ERROR404");;
            }
        }
        [Authorize(Roles = "SuperAdmin, Admin, CustomerService")]
        // GET: HoldController/Edit/5
        public ActionResult Edit(DateTime? PriorityDate, int territoryId)
        {
            HoldModel model = _holdService.GetHold(PriorityDate, territoryId);
            return View(model);
        }

        // POST: HoldController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HoldModel model)
        {
            try
            {
                if(model.QuotaQuantity < model.ReminingQuantity)
                {
                    //model.ReminingQuantity = 
                }
                model.TempReminingQuantity = model.ReminingQuantity;
                bool result = _holdService.UpdateHold(model).Result;
                if(result == true)
                {
                    ViewData["SearchPriorityDate"] = model.PriorityDate;
                    ActionResult action = Search();

                    return action;

                }
                else
                {
                    return RedirectToAction("index");
                }
            }
            catch(Exception e)
            {
                return RedirectToAction("ERROR404");;
                //return View();
            }
        }

        // GET: HoldController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HoldController/Delete/5
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
