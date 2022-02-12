using AutoMapper;
using Data.Repository;
using @enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PriorityApp.Models.Auth;
using PriorityApp.Models.Models;
using PriorityApp.Models.Models.MasterModels;
using PriorityApp.Service.Contracts;
using PriorityApp.Service.Contracts.Auth;
using PriorityApp.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PriorityApp.Service.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order, long> _repository;
        private readonly IRepository<Order, Hold> _repository2;
        private readonly IUserService _userService;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AspNetUser> _userManager;

      

        public OrderService(IRepository<Order, long> repository,
                            IRepository<Order, Hold> repository2,
                            ILogger<OrderService> logger,
                            IMapper mapper,
                            IUserService userService,
                            UserManager<AspNetUser> userManager)
        {
            _repository = repository;
            _repository2 = repository2;
            _logger = logger;
            _mapper = mapper;
            _userService = userService;
            _userManager = userManager;

        }

        async Task<bool> IOrderService.CreateOrder(OrderModel2 model)
        {
            try
            {
                Order order = _mapper.Map<Order>(model);
                 _repository.Add(order);
                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return false;
        }

        bool IOrderService.DeleteOrder(int id)
        {
            throw new NotImplementedException();
        }

        async Task<List<OrderModel2>> IOrderService.GetAllOrders()
        {
            try
            {
                var items = _repository.GetAll().ToList();
                var models = new List<OrderModel2>();
                models = _mapper.Map<List<OrderModel2>>(items);
                return models;
            }
            catch (Exception e)

            {
                _logger.LogError(e.ToString());
            }
            return null;
        }

        OrderModel2 IOrderService.GetOrder(long id)
        {
            try
            {
                var order = _repository.Find(o => o.Id == id).FirstOrDefault();
                OrderModel2 model = new OrderModel2();
                model = _mapper.Map <OrderModel2>(order);
                return model;
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;
        }

        async Task<List<OrderModel2>> IOrderService.GetOdersByListOfZoneId(List<int> ZoneIds)
        {
            //try
            //{
            //    var orders = _repository.Findlist().Result.Where(o => ZoneIds.Contains(o.ZoneId ?? 0));
            //    var models = new List<OrderModel>();
            //    models = _mapper.Map<List<OrderModel>>(orders);
            //    return models;
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e.ToString());
            //}
            return null;
        }

        public async Task<List<OrderModel2>> GetOdersByListOfCustomerNumbers(List<long> CustomerNumbers, DateTime selectedPriorityDate)
        {
            try
            {
                //var orders = _repository.Findlist().Result.Where(o => CustomerNumbers.Contains(o.CustomerId ??0)).Where(o=>o.PriorityDate == selectedPriorityDate);
                var orders = _repository.Find(o=>o.PriorityDate == selectedPriorityDate, false,o=>o.Customer, o=>o.Item).Where(o => CustomerNumbers.Contains(o.CustomerId ?? 0));

                var models = new List<OrderModel2>();
                models = _mapper.Map<List<OrderModel2>>(orders);
                return models;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;
        }


        public async Task<List<OrderModel2>> GetSubmittedOdersByListOfCustomerNumbers(List<long> CustomerNumbers, DateTime selectedPriorityDate)
        {
            try
            {
                //var orders = _repository.Findlist().Result.Where(o => CustomerNumbers.Contains(o.CustomerId ??0)).Where(o=>o.PriorityDate == selectedPriorityDate);
                var orders = _repository.Find(o => o.PriorityDate == selectedPriorityDate, false, o => o.Customer, o => o.Item,o=>o.Priority, o=>o.Customer.zone.Territory.State).Where(o => CustomerNumbers.Contains(o.CustomerId ?? 0) && o.SubmitTime != null);

                var models = new List<OrderModel2>();
                models = _mapper.Map<List<OrderModel2>>(orders);
                return models;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;
        }

        public async Task<bool> UpdateOrder(OrderModel2 model)
        {
            try
            {
                Order order = new Order();
                order = _mapper.Map<Order>(model);
                var response = _repository.Update(order);
                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return false;

        }

        public async Task<bool> CreateOrder(OrderModel2 model,HoldModel holdModel)
        {
            
            try
            {
                Order order = new Order();
                order = _mapper.Map<Order>(model);
                Hold hold = new Hold();
                hold = _mapper.Map<Hold>(holdModel);
                bool createItem = model.OrderNumber == 0 ? true : false;
              
                var response =  _repository2.UpdateTwoEntities(order, hold, createItem);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return false;

        }


        public async Task<bool> UpdateOrder2(OrderModel2 model, HoldModel holdModel)
        {

            try
            {
                Order order = new Order();
                order = _mapper.Map<Order>(model);
                Hold hold = new Hold();
                hold = _mapper.Map<Hold>(holdModel);

                var response = _repository2.UpdateTwoEntities(order, hold);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return false;

        }

        async Task<List<OrderModel2>> IOrderService.GetAllUnSubmittedOrdersByRole(List<string> roles, bool submitted)
        {
            List<OrderModel> selectedOrders = new List<OrderModel>();

            try
            {
                List<AspNetUser> users = new List<AspNetUser>();
                foreach (string role in roles)
                {
                    var x = _userManager.GetUsersInRoleAsync(role).Result;
                    users.AddRange(x);
                }
                List<string> userIds = users.Select(u => u.Id).ToList();
                List<Order> orders = _repository.Find(o => o.SavedBefore == true && o.Submitted == submitted, false, o=>o.Customer.zone.Territory).Where(o => userIds.Contains(o.WHSavedID)).ToList();

                List<OrderModel2> selectedorderModels2 = new List<OrderModel2>();
                selectedorderModels2 = _mapper.Map<List<OrderModel2>>(orders);

                return selectedorderModels2;


            }
            catch(Exception e)
            {
                e.ToString();
            }
            return null;
        }

        public int getLastSubmitNumberForToday(DateTime date)
        {
            try
            {

                var orders = _repository.Findlist().Result.Where(o=>o.SubmitTime.ToString().Contains(date.Date.ToShortDateString())).ToList();   //tTime == date.Date  && o.Submitted == true).ToList();
                int LastSubmitNumber;
                if (orders.Count != 0)
                {
                   
                    var SubmitNumber = _repository.Findlist().Result.Where(o => o.SubmitTime.ToString().Contains(date.Date.ToShortDateString())).ToList().OrderBy(o => o.SubmitNumber).Select(o => o.SubmitNumber).Last();
                    if (SubmitNumber == 0)
                    {
                        LastSubmitNumber = (int)CommanData.NonExistSubmitNumber.ThereOneSubmitForThisPriorityDate;
                    }
                    else
                    {
                        LastSubmitNumber = SubmitNumber;
                    }
                }
                else
                {
                    LastSubmitNumber = (int)CommanData.NonExistSubmitNumber.NoSubmitBeforeForThisPriorityDate;
                }
                return LastSubmitNumber;
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return -2;
        }

        public async Task<List<OrderModel2>> GetOdersListWithCustomerAndSubRegion(List<long> OrderIds)
        {
            try
            {
                var orders = _repository.Find(o => OrderIds.Contains(o.Id), false, o => o.Customer, o => o.Customer.zone.Territory.State, o=>o.Priority);//.zone.Territory.State);

                var models = new List<OrderModel2>();
                models = _mapper.Map<List<OrderModel2>>(orders);
                return models;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null;
        }

        public  async Task<List<OrderModel2>> GetSubmittedOdersByPriorityDate(DateTime selectedPriorityDate)
        {
            try
            {

                var orders = _repository.Find(o => o.PriorityDate == selectedPriorityDate && o.Submitted == true, false, o => o.Customer, o => o.Customer.zone.Territory.State, o => o.Priority).ToList();
                List<OrderModel2> orderModel2s = _mapper.Map<List<OrderModel2>>(orders);
                return orderModel2s;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null; 
        }

        public async Task<List<OrderModel2>> GetAllUnSubmittedOrdersByUserId(string userId, bool submitted)
        {
            try
            {

                var orders = _repository.Find(o=>o.Submitted == submitted && o.WHSavedID == userId, false).ToList();
                List<OrderModel2> orderModel2s = _mapper.Map<List<OrderModel2>>(orders);
                return orderModel2s;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return null; ;
        }
    }
}
