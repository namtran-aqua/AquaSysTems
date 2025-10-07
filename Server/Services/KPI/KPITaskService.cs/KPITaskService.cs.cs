using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.KPITasks;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.KPITaskService;

public class KPITaskService : IKPITaskService
{
    private readonly IRepository<KPITask> _kPITaskRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<QuaterCalculated> _quaterCalculatedRepo;
    private readonly IRepository<Formula> _formulaRepo;
    private readonly IRepository<Department> _departmentRepo;
    private readonly IRepository<Factory> _factoryIdRepo;



    public KPITaskService(IRepository<KPITask> kPITaskRepo,
        IRepository<User> userRepo,
        IRepository<QuaterCalculated> quaterCalculatedRepo,
        IRepository<Formula> formulaRepo,
        IRepository<Department> departmentRepo,
        IRepository<Factory> factoryIdRepo
        )
    {
        _kPITaskRepo = kPITaskRepo;
        _userRepo = userRepo;
        _quaterCalculatedRepo = quaterCalculatedRepo;
        _formulaRepo = formulaRepo;
        _departmentRepo = departmentRepo;
        _factoryIdRepo = factoryIdRepo;
    }

    public async Task<bool> CreatedAsync(HandleTaskDto formulaDto)
    {
        var entity = new KPITask
        {
            Id = Guid.NewGuid(),
            TaskName = formulaDto.TaskName,
            KPICategory = formulaDto.KPICategory,
            TaskDescription = formulaDto.TaskDescription,
            CalculatedMdethod = formulaDto.CalculatedMdethod,
            DataSource = formulaDto.DataSource,
            OwnerId = formulaDto.OwnerId,
            KPIIndexType = formulaDto.KPIIndexType,
            QuaterCalculatedId = formulaDto.QuaterCalculatedId,
            FormulaId = formulaDto.FormulaId,
            Max = formulaDto.Max,
            Bottom = formulaDto.Bottom,
            FactoryId = formulaDto.FactoryId,
            Unit = formulaDto.Unit,
            DepartmentId = formulaDto.DepartmentId,
            CreatedById = formulaDto.CreatedById,
            CreatedDate = DateTime.Now
        };
        await _kPITaskRepo.InsertAsync(entity);
        return await _kPITaskRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeletedAsync(Guid Id)
    {
        var entity = await _kPITaskRepo.GetByIdAsync(Id);
        if (entity == null)
            return false;
        return await _kPITaskRepo.DeleteAsync(entity);
    }

    public async Task<List<KPITaskDto>> LoadListAsync()
    {
        var user = await _userRepo.GetQueryableAsync();
        var query = from kpiTask in await _kPITaskRepo.GetQueryableAsync()
                    join owner in user on kpiTask.OwnerId equals owner.Id
                    join quater in await _quaterCalculatedRepo.GetQueryableAsync() on kpiTask.QuaterCalculatedId equals quater.Id
                    join formula in await _formulaRepo.GetQueryableAsync() on kpiTask.FormulaId equals formula.Id
                    join department in await _departmentRepo.GetQueryableAsync() on kpiTask.DepartmentId equals department.Id
                    join factory in await _factoryIdRepo.GetQueryableAsync() on kpiTask.FactoryId equals factory.Id
                    join createdBy in user on kpiTask.CreatedById equals createdBy.Id
                    select new KPITaskDto
                    {
                        Id = kpiTask.Id,
                        TaskName = kpiTask.TaskName,
                        KPICategory = kpiTask.KPICategory,
                        TaskDescription = kpiTask.TaskDescription,
                        CalculatedMdethod = kpiTask.CalculatedMdethod,
                        DataSource = kpiTask.DataSource,
                        OwnerId = kpiTask.OwnerId,
                        OwnerName = owner.FullName,
                        KPIIndexType = kpiTask.KPIIndexType,
                        QuaterCalculatedId = kpiTask.QuaterCalculatedId,
                        QuaterCalculated = quater.Calculated,
                        FormulaId = kpiTask.FormulaId,
                        Formula = formula.FormulaName,
                        Max = kpiTask.Max,
                        Bottom = kpiTask.Bottom,
                        FactoryId = kpiTask.FactoryId,
                        Factory = factory.Name,
                        DepartmentId = kpiTask.DepartmentId,
                        Department = department.Name,
                        Unit = kpiTask.Unit,
                        CreatedById = kpiTask.CreatedById,
                        CreatedByName = createdBy.FullName,
                        CreatedDate = kpiTask.CreatedDate
                    };
                    var result = await query.ToListAsync(); 
        if(result == null || result.Count == 0)
            return new List<KPITaskDto>();  
        return result;

    }
    public async Task<bool> UpdateAsync(HandleTaskDto formulaDto)
    {
       var entity = await _kPITaskRepo.GetByIdAsync(formulaDto.Id);
        if (entity == null)
            return false;
        entity.TaskName = formulaDto.TaskName;
        entity.KPICategory = formulaDto.KPICategory;
        entity.TaskDescription = formulaDto.TaskDescription;
        entity.CalculatedMdethod = formulaDto.CalculatedMdethod;
        entity.DataSource = formulaDto.DataSource;
        entity.OwnerId = formulaDto.OwnerId;
        entity.KPIIndexType = formulaDto.KPIIndexType;
        entity.QuaterCalculatedId = formulaDto.QuaterCalculatedId;
        entity.FormulaId = formulaDto.FormulaId;
        entity.Max = formulaDto.Max;
        entity.Bottom = formulaDto.Bottom;
        entity.FactoryId = formulaDto.FactoryId;
        entity.Unit = formulaDto.Unit;
        entity.DepartmentId = formulaDto.DepartmentId;
        return await _kPITaskRepo.UpdateAsync(entity);
    }
}
