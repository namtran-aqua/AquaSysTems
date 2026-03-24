using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.CeilingLevel;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.CeilingLevelService;

public class CeilingLevelService : ICeilingLevelService
{
    private readonly IRepository<CeilingLevel> _ceilingLevelRepo;
    private readonly IRepository<User> _userRepo;

    public CeilingLevelService(IRepository<CeilingLevel> ceilingLevelRepo,
        IRepository<User> userRepo)
    {
        _ceilingLevelRepo = ceilingLevelRepo;
        _userRepo = userRepo;
    }

    public async Task<CeilingLevelDto> CeilingLevelByUserId(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        var ceilingLevel = await _ceilingLevelRepo.GetAllAsync();
        var data = ceilingLevel.Where(x => x.FactoryId == user.FactoryId && x.IsActive).FirstOrDefault();
        if (data == null)
        {
            return new CeilingLevelDto();
        }
        return new CeilingLevelDto
        {
            Id = data.Id,
            FactoryId = data.FactoryId,
            CeilingLevelValue = data.CeilingLevelValue,
            IsActive = data.IsActive,
            Description = data.Description,
            CreatedBy = data.CreatedBy,
            CreatedDate = data.CreatedDate
        };
    }
}
