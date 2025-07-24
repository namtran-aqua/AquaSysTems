using AntDesign;
using AquaSolution.Client.Components.Users;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components
{
    public partial class HeaderComponent
    {
        private bool isOpen = false;
        [Parameter] public CurrentUserInfo? users { get; set; }
        private UserDto ? userDto { get; set; }
        private UserDetailModal userDetailModal = new UserDetailModal();
        private ChangePassModal changePassModal = new ChangePassModal();
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private MessageService Message { get; set; } = default!;
        private async Task Logout()
        {
            try
            {
                await Http.PostAsync("api/auth/logout", null);
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                Navigation.NavigateTo("/login", true);
                await Message.Success("Đăng xuất thành công!", 2);
            }
            catch (Exception ex)
            {
                await Message.Error("Đăng xuất thất bại: " + ex.Message);
            }
        }
        private async Task DetailMyAccount()
        {

                userDto = await Http.GetFromJsonAsync<UserDto>($"api/user/get-curernUser/{users.UserId}");
                await userDetailModal.ShowModal(userDto);
        }
        private void  ChangePassWord()
        {

            changePassModal.ShowModal(users.UserId);
        }


    }
}
