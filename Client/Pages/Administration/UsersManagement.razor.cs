
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class UsersManagement
    {

        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<UserDto>? _users = new();
        private List<UserDto>? _userFilter = new();
        private bool _isLoading = true;
        private RoleManagerDialog? _roleManagerDialog;
        private UserModal? _userModal;
        private UserDto? CurrenUser { get; set; }
        private UserDetailModal? _detailModal;
        private bool Created { get; set; }
        private bool Edit { get; set; }
        private bool Delete { get; set; }
        private bool EditRole { get; set; }
        public bool Loading { get; set; }
        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadData();
            await LoadDataFilterAsync();
            _isLoading = false;
        }
        private async Task GetPage()
        {
            var url = "user-management";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task LoadData()
        {
            try
            {
                Loading = true;
                if (Http != null) _users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                _userFilter = _users;
                await Search();
                Loading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        private async Task CheckPermission()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }

            EditRole =await permissionService.HasPermissionAsync(PageId, PermissionActionType.EditRole);
            Created = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Add);
            Edit = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Edit);
            Delete = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Delete);
        }
        #endregion
        #region Action
        private async Task AddUserDialog()
        {
            if (CurrenUser != null) await _userModal?.ShowModelAsync(false, new CreatedAndUpdateUserDto(), CurrenUser)!;
        }

        private async Task EditUser(UserDto user)
        {
            var updateDto = new CreatedAndUpdateUserDto
            {
                Id = user.Id,
                WorkDayId = user.WorkDayId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ManagerId = user.ManagerId,
                PhoneNumber = user.PhoneNumber,
                GroupId = user.GroupId,
                DepartmentId = user.DepartmentId,
                FactoryId = user.FactoryId,
                PositionId = user.PositionId,
                IsActive = user.IsActive
            };
            if (CurrenUser != null) await _userModal?.ShowModelAsync(true, updateDto, CurrenUser)!;
        }

        private Task ShowRoleDialog(UserDto user)
        {
            _roleManagerDialog?.Show(user);
            return Task.CompletedTask;
        }

        private async Task DeleteAsync(UserDto user)
        {
            var message = $"Are you sure you want to delete the user \" {user.FullName} \" ?";
            var confirm = await MessageBox.Confirm(modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/user/Delete/{user.Id}")!;
                await LoadData();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Deleted successfully");
                }
                else
                {
                    await Message.Error(content?.message ?? "An unexpected error occurred");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        private async Task DetailUser(UserDto user)
        {
            await _detailModal?.ShowModal(user, new CurrentUserInfo(), false)!;
        }
        #endregion
        #region Handle Data
        private async Task HandleSaved()
        {
            await LoadData();
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
        private Task Search()
        {
            try
            {
                var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                var fullName = StringHelper.NormalizeText(FullName?.Trim());

                if (_users != null)
                {
                    var filtered = _users
                        .Where(x =>
                            (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                            (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.FullName) && StringHelper.NormalizeText(x.FullName).Contains(fullName)))
                        )
                        .ToList();

                    if (string.IsNullOrWhiteSpace(workDayId) &&
                        string.IsNullOrWhiteSpace(fullName))
                    {
                        filtered = _users;
                    }

                    _userFilter = filtered;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }

            return Task.CompletedTask;
        }
        private async Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            _userFilter = _users;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        private Table<UserDto>? _tableRef;
        private List<DepartmentDto> _listDepartment = new();
        private List<FactoryDto> _listFactory = new();
        private List<PositionDto> _listPosition = new();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _positionFilter = Array.Empty<TableFilter<string>>();

        private async Task LoadDataFilterAsync()
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

                _listPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all") ??
                                new List<PositionDto>();
            }

            _positionFilter = _listPosition
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();
            if (_users != null)
                foreach (var user in _users)
                {
                    user.FactoryName ??= string.Empty;
                    user.DepartmentName ??= string.Empty;
                    user.PositionName ??= string.Empty;
                }
        }
        #endregion
        #region Import
        //private async Task HandleFileSelected(InputFileChangeEventArgs e)
        //{

        //    try
        //    {
        //        var file = e.File;
        //        if (file == null) return;
        //        using var ms = await file.ToMemoryStreamAsync();
        //        CreatedUser = await ExcelImportHelper.ReadFromExcelAsync<CreatedAndUpdateUserDto>(ms, (sheet, row) =>
        //        {
        //            var dto = new CreatedAndUpdateUserDto
        //            {
        //                Id = Guid.NewGuid(),
        //                FirstName = sheet.Cells[row, 1].Text?.Trim() ?? string.Empty, // cột A
        //                LastName = sheet.Cells[row, 2].Text?.Trim() ?? string.Empty, // cột B
        //                FullName = sheet.Cells[row, 3].Text?.Trim() ?? string.Empty, // cột C
        //                WorkDayId = sheet.Cells[row, 4].Text?.Trim() ?? string.Empty, // cột D
        //                Email = sheet.Cells[row, 5].Text?.Trim() ?? string.Empty, // cột E

        //                CreatedTime = DateTime.Now,
        //                IsActive = true,
        //                CreatedBy = "Admin",

        //            };
        //            return dto;
        //        });
        //        foreach (var item in CreatedUser)
        //        {
        //            var response = await Http.PostAsJsonAsync($"api/user/create", item);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                await Message.Success("Created successfully.");
        //            }
        //            else
        //            {
        //                var error = await response.Content.ReadAsStringAsync();
        //                await Message.Error($"Lỗi: {error}");
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw ex;
        //    }

        //}
        #endregion

    }
}
