using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;

namespace AquaSolution.Server.Services.KPi.FormulaService
{
    public interface IFormulaService
    {
        Task<List<FormulaDto>> LoadListAsync();
        Task<bool> CreatedAsync(FormulaDto formulaDto);
        Task<bool>UpdateAsync(FormulaDto formulaDto);
        Task<bool>DeletedAsync(Guid Id);

    }
}
