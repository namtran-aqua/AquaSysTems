using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.KPI.KPITask;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI
{
    public partial class KPITaskManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<KPITaskDto> DataSource = new();
        private Table<KPITaskDto> tableRef;
        private bool Created { get;set; }
        private bool Edit { get;set; }
        private bool Delete { get;set; }
        private Guid PageId { get; set; }
        private UserDto CurrenUser { get; set; }
        private HandleTaskModal handleTaskModal;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadData();
     
        }
        private async Task LoadData()
        {
            //DataSource = await Http.GetFromJsonAsync<List<KPITaskDto>>("api/KPITask");
            //StateHasChanged();
        }
        private async Task GetPage()
        {

            var url = "task-management";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");

        }
        private async Task CheckPermission()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            Created = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Add);
            Edit = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Edit);
            Delete = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Delete);
        }
        #endregion
        #region Actions
        private async Task CreatedAsync()
        {

            await handleTaskModal.ShowModal(CurrenUser);
        }
        private async Task EditAsync(KPITaskDto kPITaskDto)
        {
            var handleDto = new HandleTaskDto
            {
                Id = kPITaskDto.Id,
                TaskName = kPITaskDto.TaskName ?? string.Empty,
                KPICategory = kPITaskDto.KPICategory,
                TaskDescription = kPITaskDto.TaskDescription ?? string.Empty,
                CalculatedMdethod = kPITaskDto.CalculatedMdethod ?? string.Empty,
                DataSource = kPITaskDto.DataSource ?? string.Empty,
                OwnerId = kPITaskDto.OwnerId,
                KPIIndexType = kPITaskDto.KPIIndexType,
                QuaterCalculatedId = kPITaskDto.QuaterCalculatedId,
                QuaterCalculated = kPITaskDto.QuaterCalculated ?? string.Empty,
                FormulaId = kPITaskDto.FormulaId,
                Max = kPITaskDto.Max,
                Bottom = kPITaskDto.Bottom,
                FactoryId = kPITaskDto.FactoryId,
                Factory = kPITaskDto.Factory ?? string.Empty,
                Unit = kPITaskDto.Unit ?? string.Empty,
                DepartmentId = kPITaskDto.DepartmentId,
            };
            await handleTaskModal.ShowModal(CurrenUser,true, handleDto);
        }
        private async Task ViewAsync(KPITaskDto kPITaskDto)
        {

        }
        private async Task DeleteAsync(KPITaskDto kPITaskDto)
        {

        }

        #endregion
        #region Search
        private string? TaskName { get; set; }
        private async Task Search()
        {
          
        }
        private async Task Reset()
        {

        }   
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void TaskNameInputChanged(ChangeEventArgs e)
        {
            TaskName = e.Value?.ToString();
        }
        #endregion
        #region Filter
        TableFilter<KPICategoryType>[] _kPICategoryFilter = Array.Empty<TableFilter<KPICategoryType>>();
        TableFilter<KPIIndexType>[] _kPIIndexTypeFilter = Array.Empty<TableFilter<KPIIndexType>>();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        #endregion
    }
}
