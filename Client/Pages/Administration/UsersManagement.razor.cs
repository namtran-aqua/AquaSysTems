
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class UsersManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<UserDto> users = new();
        private List<UserDto> userFilter = new();
        private bool isLoading = true;
        private RoleManagerDialog roleManagerDialog;
        private UserDto selectedUser;
        private HasPermission hasPermission = new();
        private UserModal userModal;
        private List<CreatedAndUpdateUserDto> CreatedUser = new();
        private UserDto CurrenUser { get; set; }
        private UserDetailModal detailModal;
        private bool Created { get; set; }
        private bool Edit { get; set; }
        private bool Delete { get; set; }
        private bool EditRole { get; set; }

        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadUsers();
            isLoading = false;
        }
        private async Task GetPage()
        {

            var url = "user-management";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");

        }
        private async Task LoadUsers()
        {
            try
            {
                users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                userFilter = users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        private async Task CheckPermission()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            Created = await hasPermission.CheckPermissions(PageId, PermissionActionType.Add.ToString(), CurrenUser);

            Edit = await hasPermission.CheckPermissions(PageId, PermissionActionType.Edit.ToString(), CurrenUser);

            Delete = await hasPermission.CheckPermissions(PageId, PermissionActionType.Delete.ToString(), CurrenUser);

            EditRole = await hasPermission.CheckPermissions(PageId, PermissionActionType.EditRole.ToString(), CurrenUser);

        }
        #endregion
        #region Action
        private async Task AddUserDialog()
        {
            await userModal.ShowModelAsync(false, new CreatedAndUpdateUserDto(), CurrenUser);
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
            await userModal.ShowModelAsync(true, updateDto, CurrenUser);
        }

        private Task ShowRoleDialog(UserDto user)
        {

            selectedUser = user;
            roleManagerDialog.Show(user);
            return Task.CompletedTask;
        }

        private async Task DeleteAsync(UserDto user)
        {
            selectedUser = user;
            var message = $"Are you sure you want to delete the user \" {user.FullName} \" ?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/user/Delete/{user.Id}");
                await LoadUsers();
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
            await detailModal.ShowModal(user, new CurrentUserInfo(), false);
        }
        #endregion
        #region Handle Data
        private async Task HandleSaved()
        {
            await LoadUsers();
        }

        #endregion
        #region Search
        private string? WorkDayId { get; set; }
        private string? FullName { get; set; }
        private string? Email { get; set; }
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
        private void EmailInputChanged(ChangeEventArgs e)
        {
            Email = e.Value?.ToString();
        }
        private async Task Search()
        {
            try
            {
                //var workDayId = WorkDayId?.Trim().ToLower();
                //var fullName = FullName?.Trim().ToLower();
                //var email = Email?.Trim().ToLower();

                //var filtered = users
                //    .Where(x =>
                //        (string.IsNullOrWhiteSpace(workDayId) || (x.WorkDayId != null && x.WorkDayId.ToLower().Contains(workDayId))) &&
                //        (string.IsNullOrWhiteSpace(fullName) || (x.FullName != null && x.FullName.ToLower().Contains(fullName))) &&
                //        (string.IsNullOrWhiteSpace(email) || (x.Email != null && x.Email.ToLower().Contains(email)))
                //    )
                //    .ToList();
                var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                var fullName = StringHelper.NormalizeText(FullName?.Trim());
                var email = StringHelper.NormalizeText(Email?.Trim());

                var filtered = users
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                        (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.FullName) && StringHelper.NormalizeText(x.FullName).Contains(fullName))) &&
                        (string.IsNullOrWhiteSpace(email) || (!string.IsNullOrEmpty(x.Email) && StringHelper.NormalizeText(x.Email).Contains(email)))
                    )
                    .ToList();

                if (string.IsNullOrWhiteSpace(workDayId) &&
                    string.IsNullOrWhiteSpace(fullName) &&
                    string.IsNullOrWhiteSpace(email))
                {
                    filtered = users;
                }

                userFilter = filtered;
                StateHasChanged();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }
        }
        private Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            Email = null;
            userFilter = users;
            StateHasChanged();
            return Task.CompletedTask;
        }
        #endregion
        #region Import
        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {

            try
            {
                var file = e.File;
                if (file == null) return;
                using var ms = await file.ToMemoryStreamAsync();
                CreatedUser = await ExcelImportHelper.ReadFromExcelAsync<CreatedAndUpdateUserDto>(ms, (sheet, row) =>
                {
                    var dto = new CreatedAndUpdateUserDto
                    {
                        Id = Guid.NewGuid(),
                        FirstName = sheet.Cells[row, 1].Text?.Trim() ?? string.Empty, // cột A
                        LastName = sheet.Cells[row, 2].Text?.Trim() ?? string.Empty, // cột B
                        FullName = sheet.Cells[row, 3].Text?.Trim() ?? string.Empty, // cột C
                        WorkDayId = sheet.Cells[row, 4].Text?.Trim() ?? string.Empty, // cột D
                        Email = sheet.Cells[row, 5].Text?.Trim() ?? string.Empty, // cột E

                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        CreatedBy = "Admin",

                    };
                    return dto;
                });
                foreach (var item in CreatedUser)
                {
                    var response = await Http.PostAsJsonAsync($"api/user/create", item);
                    if (response.IsSuccessStatusCode)
                    {
                        await Message.Success("Created successfully.");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        await Message.Error($"Lỗi: {error}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
        #endregion
    }
}
