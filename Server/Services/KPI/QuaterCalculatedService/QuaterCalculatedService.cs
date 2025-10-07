using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.QuaterCalculated;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.QuaterCalculatedService;

    public class QuaterCalculatedService : IQuaterCalculatedService
{
    private readonly IRepository<QuaterCalculated> _quaterCalculatedRepo;



    public QuaterCalculatedService(IRepository<QuaterCalculated> quaterCalculatedRepo)
    {
        _quaterCalculatedRepo = quaterCalculatedRepo;
    }

    public async Task<bool> CreatedAsync(QuaterCalculatedDto QuaterCalculatedDto)
    {
       var quaterCalculated = new QuaterCalculated
       {
            Id = Guid.NewGuid(),
            Calculated = QuaterCalculatedDto.Calculated,
           KPIQuarterCalculateType = QuaterCalculatedDto.KPIQuarterCalculateType
        };
        await _quaterCalculatedRepo.InsertAsync(quaterCalculated);
        var saveChange = await _quaterCalculatedRepo.SaveChangesAsync();
        if (saveChange > 0)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> DeletedAsync(Guid id)
    {
        var quaterCalculated = await _quaterCalculatedRepo.GetByIdAsync(id);
        if (quaterCalculated != null)
        {
             return  await _quaterCalculatedRepo.DeleteAsync(quaterCalculated);
        }
        return false;
    }

    public async Task<List<QuaterCalculatedDto>> LoadListAsync()
    {
       var query = from quaterCalculated in await _quaterCalculatedRepo.GetQueryableAsync()
                    select new QuaterCalculatedDto
                    {
                        Id = quaterCalculated.Id,
                        Calculated = quaterCalculated.Calculated,
                        KPIQuarterCalculateType = quaterCalculated.KPIQuarterCalculateType
                    };
        return await query.ToListAsync();
    }

    public async Task<bool> UpdateAsync(QuaterCalculatedDto QuaterCalculatedDto)
    {
        var quaterCalculated = await _quaterCalculatedRepo.GetByIdAsync(QuaterCalculatedDto.Id);
        if(quaterCalculated == null) { return false; }
        quaterCalculated.Calculated = QuaterCalculatedDto.Calculated;
        quaterCalculated.KPIQuarterCalculateType = QuaterCalculatedDto.KPIQuarterCalculateType;
        var update = await _quaterCalculatedRepo.UpdateAsync(quaterCalculated);
        return update;
    }
}
