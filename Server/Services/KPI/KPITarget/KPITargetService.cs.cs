using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.KPITaskService;

public class KPIMonthlyTargetService : IKPIMonthlyTargetService
{
    private readonly IRepository<KPIMonthlyTarget> _KPIMonthlyTargetRepo;
    private readonly IRepository<UserTask> _userTaskRepo;
    private readonly IRepository<KPITask> _kPITaskRepo;
    public KPIMonthlyTargetService(IRepository<KPIMonthlyTarget> KPIMonthlyTargetRepo,
        IRepository<UserTask> userTaskRepo,
        IRepository<KPITask> kPITaskRepo
        )
    {
        _KPIMonthlyTargetRepo = KPIMonthlyTargetRepo;
        _userTaskRepo = userTaskRepo;
        _kPITaskRepo = kPITaskRepo;
    }

    public async  Task<List<GetUserTaskAndTargetDto>> GetTargetByTask(Guid taskId, Guid userId)
    {
        var query = from target in await _KPIMonthlyTargetRepo.GetQueryableAsync()
                    join userTask in await _userTaskRepo.GetQueryableAsync()
                    on target.UserTaskId equals userTask.Id
                    join kpiTask in await _kPITaskRepo.GetQueryableAsync()
                    on userTask.KPITaskId equals kpiTask.Id
                    where kpiTask.Id == taskId && target.UserId == userId && target.Month != null
                    select new GetUserTaskAndTargetDto
                    {
                        TaskId = kpiTask.Id,
                        Month = target.Month,
                        Year = target.Year,
                        TargetValue = target.TargetValue,
                        CreatedDate = target.CreatedDate,
                        UserId = target.UserId,
                        Index = userTask.Index,
                        Weight =userTask.Weight
                    };
        return await query.ToListAsync();

    }

  

}
