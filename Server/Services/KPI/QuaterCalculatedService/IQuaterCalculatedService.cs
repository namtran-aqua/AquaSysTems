using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.QuaterCalculated;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;

namespace AquaSolution.Server.Services.KPi.QuaterCalculatedService
{
    public interface IQuaterCalculatedService
    {
        Task<List<QuaterCalculatedDto>> LoadListAsync();
        Task<bool> CreatedAsync(QuaterCalculatedDto formulaDto);
        Task<bool>UpdateAsync(QuaterCalculatedDto formulaDto);
        Task<bool>DeletedAsync(Guid Id);

    }
}
