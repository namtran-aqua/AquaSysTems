using AntDesign;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components
{
    public partial class HeaderComponent
    {
        [Parameter] public UserDto userDto { get; set; } = new UserDto();
        [Parameter] public CurrentUserInfo? CurrentUser { get; set; }
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
                var baseUri = Navigation.BaseUri.TrimEnd('/');
                Navigation.NavigateTo($"{baseUri}/login", true);
                await Message.Success("Đăng xuất thành công!", 2);
            }
            catch (Exception ex)
            {
                await Message.Error("Đăng xuất thất bại: " + ex.Message);
            }
        }
        private async Task DetailMyAccount()
        {
                await userDetailModal.ShowModal(userDto, CurrentUser,true);
        }
        private void  ChangePassWord()
        {
            changePassModal.ShowModal(userDto.Id);
        }
        #region Notify
        private bool isDropdownVisible { get; set; } = false;
        private async Task HandleNotificationClick()
        {
            //await actionModal.ShowModal(notify, false);
            //int statusNotifiCation = 2;
            //if (notify.StatusNotifiCation == StatusNotifiCation.NoConfirm)
            //{
            //    statusNotifiCation = 0;
            //}
            //if (notify.StatusNotifiCation == StatusNotifiCation.TechnicalConfirm)
            //{
            //    statusNotifiCation = 1;
            //}
            //var url = $"SysRequest/UpdateStatusNotifi?id={notify.Id}&statusNotifiCation={statusNotifiCation}";
            //var success = await HttpService.PostJson(url, 1);
        }
        private void ToggleDropdown()
        {
            isDropdownVisible = !isDropdownVisible;
        }
        #endregion
    }
}
