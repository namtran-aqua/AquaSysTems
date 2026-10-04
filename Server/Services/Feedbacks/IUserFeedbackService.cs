using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AquaSolution.Shared.Feedbacks;

namespace AquaSolution.Server.Services.Feedbacks
{
    public interface IUserFeedbackService
    {
        Task EnsureDatabaseTablesAsync();
        Task<UserFeedbackDto> SubmitFeedbackAsync(UserFeedbackCreateDto dto);
        Task<FeedbackPagedResult<UserFeedbackDto>> GetPagedAsync(UserFeedbackFilterDto filter);
        Task<UserFeedbackDto?> GetByIdAsync(Guid id);
        Task<bool> UpdateStatusAsync(UserFeedbackStatusUpdateDto dto, string currentUserName);
        Task<bool> DeleteAsync(Guid id);
        Task<FeedbackDashboardStatsDto> GetDashboardStatsAsync();
        Task<List<string>> GetDepartmentListAsync();
        Task<List<string>> GetFactoryListAsync();
    }
}
