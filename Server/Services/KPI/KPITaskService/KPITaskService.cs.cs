using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.KPITasks;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.KPi.KPITaskService;

public class KPITaskService : IKPITaskService
{
    private readonly IRepository<KPITask> _kPITaskRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Formula> _formulaRepo;
    private readonly IRepository<Department> _departmentRepo;
    private readonly IRepository<Factory> _factoryIdRepo;
    private readonly IRepository<QuarterCalculate> _quarterCalculateRepo;



    public KPITaskService(IRepository<KPITask> kPITaskRepo,
        IRepository<User> userRepo,
        IRepository<Formula> formulaRepo,
        IRepository<Department> departmentRepo,
        IRepository<Factory> factoryIdRepo,
        IRepository<QuarterCalculate> quarterCalculateRepo
        )
    {
        _kPITaskRepo = kPITaskRepo;
        _userRepo = userRepo;
        _formulaRepo = formulaRepo;
        _departmentRepo = departmentRepo;
        _factoryIdRepo = factoryIdRepo;
        _quarterCalculateRepo = quarterCalculateRepo;
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
            PIC = formulaDto.PIC,
            CalculatedId = formulaDto.CalculatedId,
            KPIIndexType = formulaDto.KPIIndexType,
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
                    join formula in await _formulaRepo.GetQueryableAsync() on kpiTask.FormulaId equals formula.Id
                    join quarterCalculate in await _quarterCalculateRepo.GetQueryableAsync() on kpiTask.CalculatedId equals quarterCalculate.Id

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
                        PIC = kpiTask.PIC,
                        KPIIndexType = kpiTask.KPIIndexType,
                        FormulaId = kpiTask.FormulaId,
                        Formula = formula.FormulaName,
                        Max = kpiTask.Max,
                        Bottom = kpiTask.Bottom,
                        FactoryId = kpiTask.FactoryId,
                        Factory = factory.Name,
                        DepartmentId = kpiTask.DepartmentId,
                        Department = department.Name,
                        Unit = kpiTask.Unit,
                        Calculated = quarterCalculate.QuarterCalculated,
                        CalculatedId = quarterCalculate.Id,
                        CreatedById = kpiTask.CreatedById,
                        CreatedByName = createdBy.FullName,
                        CreatedDate = kpiTask.CreatedDate,
                        KPIFormulaType = formula.KPIFormulaType,
                        QuarterCalculateType = quarterCalculate.QuarterCalculateType
                    };
        var result = await query.ToListAsync();
        if (result == null || result.Count == 0)
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
        entity.PIC = formulaDto.PIC;
        entity.CalculatedId = formulaDto.CalculatedId;
        entity.KPIIndexType = formulaDto.KPIIndexType;
        entity.FormulaId = formulaDto.FormulaId;
        entity.Max = formulaDto.Max;
        entity.Bottom = formulaDto.Bottom;
        entity.FactoryId = formulaDto.FactoryId;
        entity.Unit = formulaDto.Unit;
        entity.DepartmentId = formulaDto.DepartmentId;
        return await _kPITaskRepo.UpdateAsync(entity);
    }
}
