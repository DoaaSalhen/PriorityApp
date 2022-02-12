using PriorityApp.Service.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace PriorityApp.Service.Contracts
{
    public interface IHoldService
    {
        Task<List<HoldModel>> GetAllHolds();
        Task<bool> CreateHold(HoldModel model);
        HoldModel GetHold(DateTime? priorityDate, int? territoryId);

        List<HoldModel> GetHoldBypriorityDate(DateTime? priorityDate);

        List<HoldModel> GetLastHoldByTerritoryId(int? territoryId);
        Task<bool> UpdateHold(HoldModel model);

        bool AddQuotaFile(DataTable dt, string SqlConnectionString);

        DataTable  prepareDataForHold (DataTable dt);

    }
}
