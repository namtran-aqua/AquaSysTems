using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.UserTask;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace AquaSolution.Client.Modals.KPI.UserTask
{
    public partial class UserTaskModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<KPITaskDto> DataSource = new();
        private List<KPITaskDto> ListTaskAll = new();

        private bool IsModalVisible = false;
        [Parameter]public UserDto? CurrenUser { get; set; }
        private UserDto User { get; set; }
        private AddTaskByUserModal _addTaskByUserModal = new();
        private List<UserTaskDto> UserTasks = new();
        private string TitleModal = string.Empty;
        #endregion
        #region Innit
        public async Task ShowModal(UserDto user)
        {
            TitleModal = "Title";
            User = user;
            TitleModal = user.FullName;
            await LoadData();
            IsModalVisible = true;
            StateHasChanged();
        }
        private async Task LoadData()
        {
            ListTaskAll = await Http.GetFromJsonAsync<List<KPITaskDto>>("api/KPITask/get-list");
            UserTasks = await Http.GetFromJsonAsync<List<UserTaskDto>>($"api/UserTask/list-taskIds/{User.Id}");
            if (ListTaskAll is not null && UserTasks is not null)
            {
                DataSource = (from task in ListTaskAll
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
            var userTask = UserTasks.FirstOrDefault(x => x.KPITaskId == kPITask.Id && x.UserId == User.Id);
        }
        private async Task UpdateUserTask()
        {
           await _addTaskByUserModal.ShowModal(User, ListTaskAll);
        }
        #endregion
    }
}
