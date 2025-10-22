using AquaSolution.Shared.KPI.DealineManagement;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;

namespace AquaSolution.Server.Services.KPi.FormulaService
{
    public interface IDealineManagementService
    {
        Task<List<DealineManagementDto>> GetDealineManagement();

    }
}
