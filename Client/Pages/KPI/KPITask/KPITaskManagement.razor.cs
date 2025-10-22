using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.KPI.KPITask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.KPITask
{
    public partial class KPITaskManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<KPITaskDto> DataSource = new();
        private List<KPITaskDto> DataFilter = new();
        private Table<KPITaskDto> tableRef;
        private bool Created { get;set; }
        private bool Edit { get;set; }
        private bool Delete { get;set; }
        private Guid PageId { get; set; }
        private UserDto CurrenUser { get; set; }
        private HandleTaskModal handleTaskModal;
        private KPITaskDetailModal KPITaskDetailModal;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadData();
            await LoadDataFilter();


        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<KPITaskDto>>("api/KPITask/get-list");

            if (result is not null)
            {
                DataSource = result;
            }
            else
            {
                DataSource = new();
            }
            DataFilter = DataSource;
            StateHasChanged();
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
                FormulaId = kPITaskDto.FormulaId,
                Max = kPITaskDto.Max,
                Bottom = kPITaskDto.Bottom,
                FactoryId = kPITaskDto.FactoryId,
                Unit = kPITaskDto.Unit ?? string.Empty,
                DepartmentId = kPITaskDto.DepartmentId,
            };
            await handleTaskModal.ShowModal(CurrenUser,true, handleDto);
        }
        private async Task ViewAsync(KPITaskDto kPITaskDto)
        {
            await KPITaskDetailModal.ShowModal(kPITaskDto);
        }
        private async Task DeleteAsync(KPITaskDto kPITaskDto)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            var response = await Http.DeleteAsync($"api/KPITask/delete/{kPITaskDto.Id}");

            if (response.IsSuccessStatusCode)
            {
                await LoadData();
                await Message.Success("Deleted successfully");
            }
            else
            {
                await Message.Error("An unexpected error occurred");
            }
        }

        #endregion
        #region Search
        private string? TaskName { get; set; }
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                DataFilter = DataSource;
            }
            else
            {
                var keyword = TaskName.Trim().ToLower();
                DataFilter = DataSource
                    .Where(x => x.TaskName != null && x.TaskName.ToLower().Contains(keyword))
                    .ToList();
            }
        }
        private async Task Reset()
        {
            TaskName =null;
            tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
            DataFilter = DataSource;
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
        private List<DepartmentDto> _listDepartment = new();
        private List<FactoryDto> _listFactory = new();
        private async Task LoadDataFilter()
        {
            if (Http != null)
            {
                _listDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ??
                                  new List<DepartmentDto>();
                _departmentFilter = _listDepartment
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();

                _listFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ??
                               new List<FactoryDto>();
                _factoryFilter = _listFactory
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();
                _kPICategoryFilter = Enum.GetValues(typeof(KPICategoryType))
                  .Cast<KPICategoryType>()
                  .Select(e => new TableFilter<KPICategoryType>
                  {
                      Text = EnumHelper.GetDisplayName(e),
                      Value = e,
                      Selected = false
                  })
                  .ToArray();
                _kPIIndexTypeFilter = Enum.GetValues(typeof(KPIIndexType))
                     .Cast<KPIIndexType>()
                     .Select(e => new TableFilter<KPIIndexType>
                     {
                         Text = EnumHelper.GetDisplayName(e),
                         Value = e,
                         Selected = false
                     })
                     .ToArray();
            }

        }
        #endregion
    }
}
