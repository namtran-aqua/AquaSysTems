using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.UserTask
{
    public partial class UserTaskModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<KPITaskDto> DataSource = new();
        private bool IsModalVisible = false;
        private Guid UserId { get; set; }
        private AddTaskByUserModal _addTaskByUserModal = new();
        private List<UserTaskDto> UserTasks = new();
        #endregion
        #region Innit
        public async Task ShowModal(Guid userId)
        {
            UserId = userId;
            await LoadData();
            IsModalVisible = true;
            StateHasChanged();
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<KPITaskDto>>("api/KPITask/get-list");
            UserTasks = await Http.GetFromJsonAsync<List<UserTaskDto>>($"api/UserTask/list-taskIds/{UserId}");
            if (result is not null && UserTasks is not null)
            {
                DataSource = (from task in result
                              join userTask in UserTasks
                              on task.Id equals userTask.KPITaskId
                              orderby userTask.Index
                              select task)
                             .ToList();
            }
            else
            {
                DataSource = new();
            }
            StateHasChanged();

        }
        #endregion
        #region Action
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        private async Task ViewAsync(KPITaskDto kPITask)
        {
            var userTask = UserTasks.FirstOrDefault(x => x.KPITaskId == kPITask.Id && x.UserId == UserId);
        }
        private async Task UpdateUserTask()
        {
           await _addTaskByUserModal.ShowModal(UserId);
        }
        #endregion
    }
}
