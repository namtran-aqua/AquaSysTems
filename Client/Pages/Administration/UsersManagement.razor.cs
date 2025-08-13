
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
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
        private UserDto CurrenUser {  get; set; }
        private UserDetailModal detailModal;
        private bool Created {  get; set; }
        private bool Edit {  get; set; }
        private bool Delete {  get; set; }
        private bool EditRole {  get; set; }

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
            var baseUri = new Uri(Navigation.BaseUri);
            var uri = new Uri(Navigation.Uri);

            var basePath = baseUri.AbsolutePath.TrimEnd('/');
            var fullPath = uri.AbsolutePath;

            string currentPath;
            if (!string.IsNullOrEmpty(basePath))
                currentPath = fullPath.Replace(basePath, "");
            else
                currentPath = fullPath;

            currentPath = currentPath.TrimStart('/');

            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{currentPath}");

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
        private  async Task CheckPermission()
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
          await userModal.ShowModelAsync(false,new CreatedAndUpdateUserDto(), CurrenUser);
        }

        private async Task EditUser(UserDto user)
        {
            var updateDto = new CreatedAndUpdateUserDto{
                Id =user.Id,
                WorkDayId = user.WorkDayId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ManagerId= user.ManagerId,
                PhoneNumber = user.PhoneNumber,
                GroupId = user.GroupId,
                DepartmentId = user.DepartmentId,
                FactoryId = user.FactoryId,
                PositionId = user.PositionId,
                IsActive =user.IsActive
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
           var confirm =  await MessageBox.Confirm(modal,message.ToString());
            if(confirm)
            {
                var response = await Http.DeleteAsync($"api/user/Delete/{user.Id}");
                await LoadUsers();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                  await  Message.Success(content?.message ?? "Deleted successfully");
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
            await detailModal.ShowModal(user, new CurrentUserInfo(),false);
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
                var workDayId = WorkDayId?.Trim().ToLower();
                var fullName = FullName?.Trim().ToLower();
                var email = Email?.Trim().ToLower();

                var filtered = users
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(workDayId) || (x.WorkDayId != null && x.WorkDayId.ToLower().Contains(workDayId))) &&
                        (string.IsNullOrWhiteSpace(fullName) || (x.FullName != null && x.FullName.ToLower().Contains(fullName))) &&
                        (string.IsNullOrWhiteSpace(email) || (x.Email != null && x.Email.ToLower().Contains(email)))
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
    }
}
