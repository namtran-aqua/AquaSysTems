using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.KPITaskService;

public class KPIMonthlyTargetService : IKPIMonthlyTargetService
{
    private readonly IRepository<KPIMonthlyTarget> _KPIMonthlyTargetRepo;
    private readonly IRepository<UserTask> _userTaskRepo;
    private readonly IRepository<KPITask> _kPITaskRepo;
    private readonly IRepository<QuarterCalculate> _quarterCalculateRepo;

    public KPIMonthlyTargetService(IRepository<KPIMonthlyTarget> KPIMonthlyTargetRepo,
        IRepository<UserTask> userTaskRepo,
        IRepository<KPITask> kPITaskRepo,
        IRepository<QuarterCalculate> quarterCalculateRepo
        )
    {
        _KPIMonthlyTargetRepo = KPIMonthlyTargetRepo;
        _userTaskRepo = userTaskRepo;
        _kPITaskRepo = kPITaskRepo;
        _quarterCalculateRepo = quarterCalculateRepo;
    }

    //public async  Task<List<GetUserTaskAndTargetDto>> GetTargetByTask(Guid taskId, Guid userId)
    //{
    //    try
    //    {
    //        var query = from target in await _KPIMonthlyTargetRepo.GetQueryableAsync()

    //                    join userTask in await _userTaskRepo.GetQueryableAsync()
    //                    on target.UserTaskId equals userTask.Id
    //                    into target2 from tar

    //                    join kpiTask in await _kPITaskRepo.GetQueryableAsync()
    //                    on userTask.KPITaskId equals kpiTask.Id

    //                    join quarterCalculate in await _quarterCalculateRepo.GetQueryableAsync()
    //                    on kpiTask.CalculatedId equals quarterCalculate.Id
    //                    where kpiTask.Id == taskId 
    //                    select new GetUserTaskAndTargetDto
    //                    {
    //                        TaskId = kpiTask.Id,
    //                        Month = target.Month,
    //                        Year = target.Year,
    //                        TargetValue = target.TargetValue,
    //                        CreatedDate = target.CreatedDate,
    //                        UserId = target.UserId,
    //                        Index = userTask.Index,
    //                        Weight = userTask.Weight,
    //                        QuarterCalculateType = quarterCalculate.QuarterCalculateType,
    //                    };
    //        return await query.ToListAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        throw;
    //    }

    //}
    public async Task<List<GetUserTaskAndTargetDto>> GetTargetByTask(Guid taskId, Guid userId)
    {
        var taskQuery = await _kPITaskRepo.GetQueryableAsync();
        var userTaskQuery = await _userTaskRepo.GetQueryableAsync();
        var targetQuery = await _KPIMonthlyTargetRepo.GetQueryableAsync();
        var quarterQuery = await _quarterCalculateRepo.GetQueryableAsync();

        var query =
            from kpiTask in taskQuery

            join ut in userTaskQuery
                on kpiTask.Id equals ut.KPITaskId
                into userTaskGroup

            from userTask in userTaskGroup
                .DefaultIfEmpty()

            join t in targetQuery
                on userTask.Id equals t.UserTaskId
                into targetGroup
            from target in targetGroup.DefaultIfEmpty()

                // LEFT JOIN QuarterCalculate
            join qc in quarterQuery
                on kpiTask.CalculatedId equals qc.Id
                into quarterGroup
            from quarterCalculate in quarterGroup.DefaultIfEmpty()

            where kpiTask.Id == taskId

            select new GetUserTaskAndTargetDto
            {
                TaskId = kpiTask.Id,

                Month = target != null ? target.Month : null,
                Year = target != null ? target.Year : DateTime.Now.Year,
                TargetValue = target != null ? target.TargetValue : 0,
                CreatedDate = target != null ? target.CreatedDate : DateTime.Now,

                UserId = userTask != null ? userTask.UserId : userId,
                Index = userTask != null ? userTask.Index : null,
                Weight = userTask != null ? userTask.Weight : 0,

                QuarterCalculateType = quarterCalculate != null
                    ? quarterCalculate.QuarterCalculateType
                    : QuarterCalculateType.CAL1
                    ,
            };

        return await query.ToListAsync();
    }




}
