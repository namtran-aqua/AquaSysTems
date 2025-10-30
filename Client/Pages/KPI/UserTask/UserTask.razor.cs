using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Client.Components.KPI.UserTask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        public bool Loading { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            //await GetPage();
            //await CheckPermission();
            await LoadData();
            await LoadDataFilterAsync();
        }
        private async Task LoadData()
        {
            try
            {
                Loading = true;
                users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                userFilter = users;
                await Search();
                Loading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        #endregion
        #region Actions

        private async Task EditTask(UserDto user)
        {
          await _userTaskModal.ShowModal(user.Id);
        }
        private async Task ViewAsync(UserDto user)
        {
          //  await KPITaskDetailModal.ShowModal(kPITaskDto);
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

                if (string.IsNullOrWhiteSpace(workDayId) &&
                    string.IsNullOrWhiteSpace(fullName))
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
                .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
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

    }
}
