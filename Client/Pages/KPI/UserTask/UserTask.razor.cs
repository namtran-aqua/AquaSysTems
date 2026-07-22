using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Client.Modals.KPI.Target;
using AquaSolution.Client.Modals.KPI.UserTask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.CeilingLevel;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.UserTask
{
    public partial class UserTask
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<UserDto> users = new();
        private List<UserDto> userFilter = new();
        private UserTaskModal _userTaskModal = new();
        private TargetModal targetModal = new();
        private int Month { get; set; }
        private int Year { get; set; }
        private Guid PageId { get; set; }
        private UserDto? CurrenUser { get; set; }
        public bool Loading { get; set; }
        private bool TaskManagement { get; set; } = false;
        private bool IsLock { get; set; }
        private HubConnection? _hubConnection;
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadCurrenUser();
            await GetPage();
            await CheckPermission();
            Month = DateTime.Now.Month;
            Year = DateTime.Now.Year;
            await CheckLock();
            await LoadData();
            await LoadDataFilterAsync();
            await ReloadIsLock();
        }

        private async Task ReloadIsLock()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                .Build();
            _hubConnection.On<Guid>("IsLockSystem", async pageId =>
            {
                if (pageId == PageId)
                {
                    await CheckLock();
                    await InvokeAsync(StateHasChanged);
                }
            });
            await _hubConnection.StartAsync();
        }

        private async Task GetPage()
        {
            var url = "task-user-management";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }

        private async Task CheckLock()
        {
            if (CurrenUser != null && CurrenUser.Roles.Any(x => x.Name == "Admin"))
            {
                IsLock = false;
                return;
            }
            IsLock = await Http.GetFromJsonAsync<bool>($"api/systemLock/check-lock/{PageId}");
        }

        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }

        private async Task CheckPermission()
        {
            TaskManagement = await permissionService.HasPermissionAsync(PageId, PermissionActionType.TaskManagement);
        }

        private async Task LoadData()
        {
            try
            {
                Loading = true;
                StateHasChanged();
                var data = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                var result = data.Where(x => x.IsActive == true && x.PositionId != null).ToList();

                foreach (var user in result)
                {
                    user.FullName ??= string.Empty;
                    user.WorkDayId ??= string.Empty;
                    user.DepartmentName ??= string.Empty;
                    user.FactoryName ??= string.Empty;
                    user.PositionName ??= string.Empty;
                }
                var filtered = new List<UserDto>();

                if (result is not null)
                {
                    if (CurrenUser.Roles.Any(x => x.Name == "Admin") || CurrenUser.Roles.Any(x => x.Name == "HR"))
                    {
                        filtered = result;
                    }
                    else if (CurrenUser.Roles.Any(x => x.Name == "KPIUSER_DepartmentViewer"))
                    {
                        filtered = result
                            .Where(x => x.DepartmentId == CurrenUser.DepartmentId)
                            .ToList();
                    }
                    else
                    {
                        if (CurrenUser.DepartmentId != null && CurrenUser.FactoryId != null)
                        {
                            filtered = result
                                .Where(x => x.FactoryId == CurrenUser.FactoryId
                                         && x.DepartmentId == CurrenUser.DepartmentId)
                                .ToList();
                        }
                        else if (CurrenUser.FactoryId != null)
                        {
                            filtered = result
                                .Where(x => x.FactoryId == CurrenUser.FactoryId)
                                .ToList();
                        }
                    }
                    users = filtered;
                }
                userFilter = users;
                await Search();
                Loading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        #endregion

        #region Actions
        private async Task ViewTarget(UserDto user)
        {
            await targetModal.ShowModal(user);
        }

        private async Task EditTask(UserDto user)
        {
            await _userTaskModal.ShowModal(user);
        }

        #endregion

        #region Search
        private string? WorkDayId { get; set; }
        private string? FullName { get; set; }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }

        private void WorkDayIdInputChanged(ChangeEventArgs e)
        {
            WorkDayId = e.Value?.ToString();
        }

        private void FullNameInputChanged(ChangeEventArgs e)
        {
            FullName = e.Value?.ToString();
        }

        private async Task Search()
        {
            try
            {
                var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                var fullName = StringHelper.NormalizeText(FullName?.Trim());

                var filtered = users
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                        (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.FullName) && StringHelper.NormalizeText(x.FullName).Contains(fullName)))
                    )
                    .ToList();

                if (string.IsNullOrWhiteSpace(workDayId) && string.IsNullOrWhiteSpace(fullName))
                {
                    filtered = users;
                }

                userFilter = filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }
        }

        private async Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            userFilter = users;
            tableRef?.ReloadData();
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }

        private Table<UserDto> tableRef;
        private List<DepartmentDto> ListDepartment = new();
        private List<FactoryDto> ListFactory = new();
        private List<PositionDto> ListPosition = new();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _positionFilter = Array.Empty<TableFilter<string>>();

        private async Task LoadDataFilterAsync()
        {
            ListDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ?? new List<DepartmentDto>();
            _departmentFilter = ListDepartment
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            ListFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ?? new List<FactoryDto>();
            _factoryFilter = ListFactory
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            ListPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all") ?? new List<PositionDto>();
            _positionFilter = ListPosition
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            foreach (var user in users)
            {
                user.FactoryName ??= string.Empty;
                user.DepartmentName ??= string.Empty;
                user.PositionName ??= string.Empty;
            }
        }
        #endregion



        #region Helper
        private async Task<T?> SafeGetFromJsonAsync<T>(string url) where T : class
        {
            try
            {
                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null") return null;

                return System.Text.Json.JsonSerializer.Deserialize<T>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SafeGetFromJsonAsync ERROR] {url} -> {ex.Message}");
                return null;
            }
        }
        #endregion

    }
}
