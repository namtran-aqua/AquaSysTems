using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.UserTask;

namespace AquaSolution.Server.Services.KPI.KPIUserTask
{
    public interface IUserTaskService
    {
        Task<List<UserTaskDto>> GetListByUserIdAsync(Guid userId);
        Task<bool> HandleUserTaskAndTarget(HandleUserTaskAndTargetDto handleUserTaskAndTarget);
    }
}
