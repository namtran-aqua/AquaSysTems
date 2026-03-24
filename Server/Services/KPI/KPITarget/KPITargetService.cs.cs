using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.KPITarget;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.KPITaskService;

public class KPIMonthlyTargetService : IKPIMonthlyTargetService
{
    private readonly IRepository<KPIMonthlyTarget> _KPIMonthlyTargetRepo;
    private readonly IRepository<UserTask> _userTaskRepo;
    private readonly IRepository<KPITask> _kPITaskRepo;
    private readonly IRepository<QuarterCalculate> _quarterCalculateRepo;
    private readonly IRepository<Formula> _formulaRepo;
    private readonly IRepository<Factory> _factoryRepo;
    private readonly IRepository<Department> _departmentRepo;

    public KPIMonthlyTargetService(IRepository<KPIMonthlyTarget> KPIMonthlyTargetRepo,
        IRepository<UserTask> userTaskRepo,
        IRepository<KPITask> kPITaskRepo,
        IRepository<QuarterCalculate> quarterCalculateRepo,
        IRepository<Formula> formulaRepo,
        IRepository<Factory> factoryRepo,
        IRepository<Department> departmentRepo
        )
    {
        _KPIMonthlyTargetRepo = KPIMonthlyTargetRepo;
        _userTaskRepo = userTaskRepo;
        _kPITaskRepo = kPITaskRepo;
        _quarterCalculateRepo = quarterCalculateRepo;
        _formulaRepo = formulaRepo;
        _factoryRepo = factoryRepo;
        _departmentRepo = departmentRepo;
    }


    public async Task<List<GetUserTaskAndTargetDto>> GetTargetByTask(Guid taskId, Guid userId)
    {
        var taskQuery = await _kPITaskRepo.GetQueryableAsync();
        var userTaskQuery = await _userTaskRepo.GetQueryableAsync();
        var targetQuery = await _KPIMonthlyTargetRepo.GetQueryableAsync();
        var quarterQuery = await _quarterCalculateRepo.GetQueryableAsync();

        var query =
            from kpiTask in taskQuery

            join userTask in userTaskQuery
                on kpiTask.Id equals userTask.KPITaskId

            join target in targetQuery
                on userTask.Id equals target.UserTaskId

            join qc in quarterQuery
                on kpiTask.CalculatedId equals qc.Id
                into quarterGroup

            from quarterCalculate in quarterGroup.DefaultIfEmpty()

            where kpiTask.Id == taskId && userTask.UserId == userId

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

    public async Task<List<TargetDto>> GetTargetByUser(Guid userId)
     {
        try
        {
            var userTaskQuery = await _userTaskRepo.GetQueryableAsync();
            var targetQuery = await _KPIMonthlyTargetRepo.GetQueryableAsync();
            var taskQuery = await _kPITaskRepo.GetQueryableAsync();
            var formulaQuery = await _formulaRepo.GetQueryableAsync();
            var calculateQuery = await _quarterCalculateRepo.GetQueryableAsync();
            var factoryQuery = await _factoryRepo.GetQueryableAsync();
            var departmentQuery = await _departmentRepo.GetQueryableAsync();
            var rawData = await (
                from usertask in userTaskQuery
                join task in taskQuery on usertask.KPITaskId equals task.Id
                join target in targetQuery on usertask.Id equals target.UserTaskId
                join formula in formulaQuery on task.FormulaId equals formula.Id
                join calculate in calculateQuery on task.CalculatedId equals calculate.Id
                join factory in factoryQuery on task.FactoryId equals factory.Id
                join department in departmentQuery on task.DepartmentId equals department.Id
                where usertask.UserId == userId && usertask.IsActive
                select new
                {

                    target.Month,
                    target.Quarter,
                    target.HalfYear,
                    target.Year,
                    usertask.Index,
                    task.TaskName,
                    task.KPIIndexType,
                    task.KPICategory,
                    task.Max,
                    task.Bottom,
                    usertask.Weight,
                    task.Unit,
                    task.PIC,
                    task.DataSource,
                    Formula = formula.FormulaName,
                    Calculated = calculate.QuarterCalculated,
                    Factory = factory.Name,
                    Department = department.Name,
                    usertask.UserId,
                    usertask.CreatedDate,
                    calculate.QuarterCalculateType,
                    target.TargetValue
                }
            ).ToListAsync();

            var result = rawData
                .GroupBy(x => new
                {
                    x.UserId,
                    x.TaskName,
                    x.KPIIndexType,
                    x.KPICategory,
                    x.Max,
                    x.Bottom,
                    x.Weight,
                    x.Unit,
                    x.PIC,
                    x.DataSource,
                    x.Formula,
                    x.Calculated,
                    x.QuarterCalculateType,
                    x.Year,
                    x.Index,
                    x.CreatedDate,
                    x.Factory,
                    x.Department
                })
                .Select(g =>
                {
                    var dto = new TargetDto
                    {
                        UserId = g.Key.UserId,
                        TaskName = g.Key.TaskName,
                        KPIIndexType = g.Key.KPIIndexType,
                        KPICategory = g.Key.KPICategory,
                        Max = g.Key.Max,
                        Bottom = g.Key.Bottom,
                        Weight = g.Key.Weight,
                        Unit = g.Key.Unit,
                        PIC = g.Key.PIC,
                        DataSource = g.Key.DataSource,
                        Formula = g.Key.Formula,
                        Calculated = g.Key.Calculated,
                        QuarterCalculateType = g.Key.QuarterCalculateType,
                        Year = g.Key.Year,
                        Index = g.Key.Index,
                        CreatedDate = g.Key.CreatedDate,
                        Factory = g.Key.Factory,
                        Department = g.Key.Department

                    };

                    foreach (var item in g)
                    {
                        if (item.Month.HasValue)
                        {
                            switch (item.Month.Value)
                            {
                                case 1: dto.TargetValue1 = item.TargetValue; break;
                                case 2: dto.TargetValue2 = item.TargetValue; break;
                                case 3: dto.TargetValue3 = item.TargetValue; break;
                                case 4: dto.TargetValue4 = item.TargetValue; break;
                                case 5: dto.TargetValue5 = item.TargetValue; break;
                                case 6: dto.TargetValue6 = item.TargetValue; break;
                                case 7: dto.TargetValue7 = item.TargetValue; break;
                                case 8: dto.TargetValue8 = item.TargetValue; break;
                                case 9: dto.TargetValue9 = item.TargetValue; break;
                                case 10: dto.TargetValue10 = item.TargetValue; break;
                                case 11: dto.TargetValue11 = item.TargetValue; break;
                                case 12: dto.TargetValue12 = item.TargetValue; break;
                            }
                        }

                        if (item.Quarter.HasValue)
                        {
                            switch (item.Quarter.Value)
                            {
                                case 1: dto.TargetQarter1 = item.TargetValue; break;
                                case 2: dto.TargetQarter2 = item.TargetValue; break;
                                case 3: dto.TargetQarter3 = item.TargetValue; break;
                                case 4: dto.TargetQarter4 = item.TargetValue; break;
                            }
                        }

                        // HALF YEAR
                        if (item.HalfYear.HasValue)
                        {
                            if (item.HalfYear == 1)
                                dto.TargetHaftYear1 = item.TargetValue;
                            else if (item.HalfYear == 2)
                                dto.TargetHaftYear2 = item.TargetValue;
                        }

                        // YEAR
                        if (!item.Month.HasValue &&
                            !item.Quarter.HasValue &&
                            !item.HalfYear.HasValue)
                        {
                            dto.TargetYear = item.TargetValue;
                        }
                    }

                    return dto;
                })
                .OrderBy(x => x.Index)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
}
