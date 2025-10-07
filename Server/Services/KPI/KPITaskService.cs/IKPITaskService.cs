using AquaSolution.Shared.KPI.KPITasks;


namespace AquaSolution.Server.Services.KPi.KPITaskService
{
    public interface IKPITaskService
    {
        Task<List<KPITaskDto>> LoadListAsync();
        Task<bool> CreatedAsync(HandleTaskDto formulaDto);
        Task<bool>UpdateAsync(HandleTaskDto formulaDto);
        Task<bool>DeletedAsync(Guid Id);
    }
}
