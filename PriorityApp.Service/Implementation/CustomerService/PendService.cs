﻿using AutoMapper;
using Data.Repository;
using @enum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PriorityApp.Models.Models;
using PriorityApp.Service.Contracts.CustomerService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityApp.Service.Implementation.CustomerService
{
    public class PendService : IPendService
    {
        private readonly IRepository<Order, long> _repository;
        private readonly ILogger<PendService> _logger;
        private readonly IMapper _mapper;

        public enum Zones
        {
            KEF = 1,
            CAI = 2,
            ASW = 3,
            MAS = 4,
            MTM = 5,
            NAG = 6,
            BDR = 7
        }
        public PendService(IRepository<Order, long> repository,
                            ILogger<PendService> logger,
                            IMapper mapper)
        {
            _repository = repository;
            logger = _logger;
            mapper = _mapper;
        }
        List<Order> IPendService.GetPend()
        {
            List<Order> PendOrders = _repository.Find(o => o.Dispatched == false).ToList();
            return PendOrders;
        }
        public Task<bool> ClearPend()
        {
            try
            {
                //List<Order> PendOrders = _repository.Find(x => x.Dispatched == false).ToList();
                List<Order> PendOrders = _repository.Find(x => x.PriorityId == (int)CommanData.Priorities.No).ToList(); //priority No
                //Find(x => x.Dispatched).ToList();
                foreach (Order o in PendOrders)
                {
                    _repository.Delete(o);
                }
                return Task<bool>.FromResult<bool>(true);
            }
            catch (Exception e)
            {
                //return Task<bool>.FromResult<bool>(false);
                _logger.LogError(e.ToString());
            }
            return Task<bool>.FromResult<bool>(false);

        }
        public DataTable ReadExcelData(string filePath, string excelConnectionString)
        {
            try
            {
                DataTable dt = new DataTable();
                excelConnectionString = string.Format(excelConnectionString, filePath);

                using (OleDbConnection connExcel = new OleDbConnection(excelConnectionString))
                {
                    using (OleDbCommand cmdExcel = new OleDbCommand())
                    {
                        using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                        {
                            cmdExcel.Connection = connExcel;

                            //Get the name of First Sheet.
                            connExcel.Open();
                            DataTable dtExcelSchema;
                            dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            connExcel.Close();

                            //Read Data from First Sheet.
                            connExcel.Open();
                            cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                            odaExcel.SelectCommand = cmdExcel;
                            odaExcel.Fill(dt);
                            connExcel.Close();
                        }
                    }
                }
                return dt;
            }
            catch (Exception e)
            {

            }
            return null;
        }


        bool IPendService.WriteDataToSql(DataTable dt, string SqlConnectionString)
        {
            try
            {
                dt.CaseSensitive = false;
                using (SqlConnection con = new SqlConnection(SqlConnectionString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        //Set the database table name.
                        sqlBulkCopy.DestinationTableName = "dbo.Orders";

                        //[OPTIONAL]: Map the Excel columns with that of the database table.
                        sqlBulkCopy.ColumnMappings.Add("PriorityDATE", "PriorityDate");     //1

                        sqlBulkCopy.ColumnMappings.Add("SDPA8", "CustomerId");        //22

                        sqlBulkCopy.ColumnMappings.Add("SDDOCO", "OrderNumber");          //6
                        sqlBulkCopy.ColumnMappings.Add("SDDCTO", "OrderDocument");       //7
                        sqlBulkCopy.ColumnMappings.Add("SDLNID", "LineID");             //8
                        sqlBulkCopy.ColumnMappings.Add("DATE", "OrderDate");         //9  
                        sqlBulkCopy.ColumnMappings.Add("SDITM", "ItemId");           //10

                        sqlBulkCopy.ColumnMappings.Add("SDSHAN", "PODNumber");            //11
                        sqlBulkCopy.ColumnMappings.Add("ABALPH01", "PODName");             //12
                        sqlBulkCopy.ColumnMappings.Add("ABAC08", "PODZoneName");        //13
                        sqlBulkCopy.ColumnMappings.Add("STATE_D01", "PODZoneState");      //16
                        sqlBulkCopy.ColumnMappings.Add("ALADD1", "PODZoneAddress");      //20
                        sqlBulkCopy.ColumnMappings.Add("QNTY", "OrderQuantity");                 //17
                        sqlBulkCopy.ColumnMappings.Add("ABAC02", "CustomerType");            //18
                        //sqlBulkCopy.ColumnMappings.Add("STATUS", "Status");                     //19
                        sqlBulkCopy.ColumnMappings.Add("ORGQTY", "OrginalQuantity");         //25
                        sqlBulkCopy.ColumnMappings.Add("OrderCategoryId", "OrderCategoryId");
                        con.Open();
                        sqlBulkCopy.WriteToServer(dt);
                        con.Close();
                    }
                }
                return true;
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public bool FixDuplication()
        {
            try
            {
                var orders = _repository.Findlist().Result.GroupBy(x => x.OrderNumber).Where(x => x.Count() > 1).Select(x => x.Where(x => x.SavedBefore == false && x.OrderCategoryId == (int)CommanData.OrderCategory.Delivery)).ToList();
                //var orders = _repository.Findlist().Result.Where(x => x.SavedBefore == false && x.OrderCategoryId == (int)CommanData.OrderCategory.Delivery).GroupBy(x => x.OrderNumber).Where(x => x.Count() > 1).ToList();
                foreach (var order in orders.ToList())
                {
                    var x = order.Select(o => o.Id).Any();
                    if (x == true)
                    {
                        long id = (long)order.Where(o => o.SavedBefore == false).Select(x => x.Id).FirstOrDefault();
                        _repository.DeleteById(id);
                    }

                }
                return true;
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public DataTable Preprocess(DataTable dt)
        {

            try
            {
                dt.Columns.Add(new DataColumn("OrderCategoryId"));

                for (int index = 0; index < dt.Columns.Count; index++)
                {
                    dt.Columns[index].ColumnName = dt.Columns[index].ColumnName.Trim();
                }

                foreach (DataRow row in dt.Rows)
                {
                    var customerNumber = row[21];
                    if (customerNumber.ToString() != "")
                    {
                        row["OrderCategoryId"] = (int)CommanData.OrderCategory.Delivery;
                        string z = row["QNTY"].ToString();
                        // var customerNumber = row["SDPA8"];
                        if (row["STATUS"].ToString() == "Cancelled")
                        {
                            row.Delete();
                        }
                        else if (z == "1")
                        {
                            row.Delete();
                        }
                        else if (row["ABAC02"].ToString() == "EG04")
                        {
                            row["ABAC02"] = 2;
                        }
                        else if (row["ABAC02"].ToString() == "EG01" || row["ABAC02"].ToString() == "EG02" || row["ABAC02"].ToString() == "EG03")
                        {
                            row["ABAC02"] = 1;
                        }
                        if (customerNumber.ToString().Contains("P"))
                        {
                            customerNumber = customerNumber.ToString().Replace("P", "1");
                            row["SDPA8"] = Convert.ToInt64(customerNumber);
                        }
                    }
                }
                ////////////////////////////// replace ZoneCode by zoneId
                return dt;
            }
            catch (Exception e)
            {

            }
            return dt;
        }


    }
}
