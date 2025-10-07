using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.FormulaService;

    public class FormulaService : IFormulaService
{
    private readonly IRepository<Formula> _formulaRepo;



    public FormulaService(IRepository<Formula> formulaRepo )
    {
        _formulaRepo = formulaRepo;
    }

    public async Task<bool> CreatedAsync(FormulaDto formulaDto)
    {
       var formula = new Formula
        {
            Id = Guid.NewGuid(),
            FormulaName = formulaDto.FormulaName,
            Description = formulaDto.Description,
            KPIFormulaType = formulaDto.KPIFormulaType,
        };
        await _formulaRepo.InsertAsync(formula);
        var saveChange = await _formulaRepo.SaveChangesAsync();
        if (saveChange > 0)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> DeletedAsync(Guid id)
    {
        var formula = await _formulaRepo.GetByIdAsync(id);
        if (formula != null)
        {
             return  await _formulaRepo.DeleteAsync(formula);
        }
        return false;
    }

    public async Task<List<FormulaDto>> LoadListAsync()
    {
       var query = from formula in await _formulaRepo.GetQueryableAsync()
                    select new FormulaDto
                    {
                        Id = formula.Id,
                        FormulaName = formula.FormulaName,
                        Description = formula.Description,
                        KPIFormulaType = formula.KPIFormulaType,
                    };
        return await query.ToListAsync();
    }

    public async Task<bool> UpdateAsync(FormulaDto formulaDto)
    {
        var formula = await _formulaRepo.GetByIdAsync(formulaDto.Id);
        if(formula == null) { return false; }
        formula.FormulaName = formulaDto.FormulaName;
        formula.Description = formulaDto.Description;
        formula.KPIFormulaType = formulaDto.KPIFormulaType;
        var update = await _formulaRepo.UpdateAsync(formula);
        return update;
    }
}
