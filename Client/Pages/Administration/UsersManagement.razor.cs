
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class UsersManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<UserDto> users = new();
        private bool isLoading = true;
        private RoleManagerDialog roleManagerDialog;
        private UserDto selectedUser;
        private HasPermission hasPermission = new();
        private UserModal userModal;
        private UserDto CurrenUser {  get; set; }
        private bool Created {  get; set; }
        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            var uri = new Uri(Navigation.Uri);
            var currentPath = uri.AbsolutePath.ToString();
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl{currentPath}");
            await CheckPermission();
            await LoadUsers();
            isLoading = false;
        }
        private async Task LoadUsers()
        {
            try
            {
                users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
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
                Manager= user.ManagerId,
                PhoneNumber = user.PhoneNumber,
                GroupId = user.GroupId,
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
           var message = $"Bạn có muốn xóa user \" {user.FullName} \" không?";
           var confirm =  await MessageBox.Confirm(modal,message.ToString());
            if(confirm)
            {
                var response = await Http.DeleteAsync($"api/user/Delete/{user.Id}");
                await LoadUsers();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                  await  Message.Success(content?.message ?? "Xóa thành công");
                }
                else
                {
                    await Message.Error(content?.message ?? "Có lỗi xảy ra");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Handle Data
        private async Task HandleSaved()
        {
            await LoadUsers();
        }

        #endregion
    }
}
