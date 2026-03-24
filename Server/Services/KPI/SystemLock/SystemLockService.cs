using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Administration.SystemLock;
using AquaSolution.Shared.Administration.SystemLock;


namespace AquaSolution.Server.Services.KPi.FormulaService;

public class SystemLockService : ISystemLockService
{
    private readonly IRepository<SystemLock> _systemLockRepo;
    private readonly IRepository<Page> _pageRepo;
    private readonly IRepository<User> _userRepo;

    public SystemLockService(IRepository<SystemLock> systemLockRepo,
       IRepository<Page> pageRepo,
        IRepository<User> userRepo)
    {
        _systemLockRepo = systemLockRepo;
        _pageRepo = pageRepo;
        _userRepo = userRepo;
    }

    public async Task<bool> CheckLock(Guid pageId)
    {
       var systemLock = await _systemLockRepo.FirstOrDefaultAsync(x => x.PageId == pageId && x.IsLocket);
        return systemLock != null;
    }

    public async Task<bool> CreatedAsync(SystemLockDto systemLockDto)
    {
        var systemLock = new SystemLock
        {
            Id = Guid.NewGuid(),
            PageId = systemLockDto.PageId ?? Guid.Empty,
            IsLocket = systemLockDto.IsLocket,
            LockedBy = systemLockDto.LockedBy,
            LockedDate = DateTime.UtcNow
        };
        await _systemLockRepo.InsertAsync(systemLock);
        return await _pageRepo.SaveChangesAsync() > 0;

    }

    public async Task<List<SystemLockDto>> LoadDataAsync()
    {
        var query = from systemlock in await _systemLockRepo.GetQueryableAsync()
                    join page in await _pageRepo.GetQueryableAsync() on systemlock.PageId equals page.Id
                    join user in await _userRepo.GetQueryableAsync() on systemlock.LockedBy equals user.Id
                    select new SystemLockDto
                    {
                        Id = systemlock.Id,
                        PageId = systemlock.PageId,
                        PageName = page.Name,
                        IsLocket = systemlock.IsLocket,
                        LockedBy = systemlock.LockedBy,
                        LockedDate = systemlock.LockedDate,
                        LockedByName = user.FirstName
                    };
        if (query == null)
        {
            return new List<SystemLockDto>();
        }
        return query.ToList();
    }

    public async Task<bool> UpdateStatus(Guid systemLockId, bool isLocket)
    {

        var systemLock = _systemLockRepo.GetByIdAsync(systemLockId).Result;
        if (systemLock == null)
        {
            return false;
        }
        systemLock.IsLocket = isLocket;
        return await _systemLockRepo.UpdateAsync(systemLock);
    }
}
