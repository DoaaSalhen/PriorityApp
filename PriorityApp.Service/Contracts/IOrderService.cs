﻿using PriorityApp.Models.Auth;
using PriorityApp.Models.Models;
using PriorityApp.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriorityApp.Service.Contracts
{
     public interface IOrderService
    {
        Task<List<OrderModel2>> GetAllOrders();
        Task<bool> CreateOrder(OrderModel2 model);
        Task<bool> UpdateOrder(OrderModel2 model);
        bool DeleteOrder(int id);
        OrderModel2 GetOrder(long orderNumber);
        Task<List<OrderModel2>> GetOdersByListOfZoneId(List<int> ZoneId);
        Task<List<OrderModel2>> GetOdersByListOfCustomerNumbers(List<long> CustomerNumbers, DateTime selectedPriorityDate);

        Task<List<OrderModel2>> GetSubmittedOdersByListOfCustomerNumbers(List<long> CustomerNumbers, DateTime selectedPriorityDate);


        Task<bool> UpdateOrder2(OrderModel2 model, HoldModel holdModel);

        Task<List<OrderModel2>> GetAllUnSubmittedOrdersByRole(List<string> roles, bool submitted);

        int getLastSubmitNumberForToday(DateTime date);

        Task<List<OrderModel2>> GetOdersListWithCustomerAndSubRegion(List<long> OrderIds);
        Task<List<OrderModel2>> GetSubmittedOdersByPriorityDate(DateTime selectedPriorityDate);
        Task<List<OrderModel2>> GetAllUnSubmittedOrdersByUserId(string userId, bool submitted);
        Task<bool> CreateOrder(OrderModel2 model, HoldModel holdModel);





    }
}
