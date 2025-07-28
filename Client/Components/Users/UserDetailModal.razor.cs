using AntDesign;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace AquaSolution.Client.Components.Users
{
    public partial class UserDetailModal
    {
        private bool Visible { get; set; }
        private UserDto User { get; set; }
        [Inject] private HttpClient Http { get; set; }
        private string UserAvatarUrl { get; set; }
        private Upload uploadRef;
        private bool IsUpLoadAvata { get; set; } = false;
        public async Task ShowModal(UserDto userDto, CurrentUserInfo currentUserInfo,bool myDetail)
        {
            if (myDetail)
            {
                if (userDto.Id == currentUserInfo.UserId)
                {
                    IsUpLoadAvata = true;
                }
            }
    
            UserAvatarUrl = string.Empty;
            User = userDto;
            Visible = true;
            UserAvatarUrl = User.Avatar != null ? User.Avatar : string.Empty;
            StateHasChanged();
        }

        private void Cancel()
        {
            Visible = false;
        }

        private async Task OnUploadChanged(UploadInfo info)
        {
            if (info.File.State == UploadState.Success)
            {
                var url = info.File.Response?.ToString();
                if (!string.IsNullOrEmpty(url))
                {
                    UserAvatarUrl = url;
                }
                if(User.Avatar != null)
                {
                    var encodedUrl = Uri.EscapeDataString(User.Avatar);
                    var response = await Http.DeleteAsync($"api/upload/delete-avatar?avatarUrl={encodedUrl}");
                }
                var avata = new AvataDto();
                avata.UserId = User.Id;
                avata.URLAvatarNew = UserAvatarUrl;
                var response2 = await Http.PutAsJsonAsync("api/user/update-avatar", avata);
                if (response2.IsSuccessStatusCode)
                {
                    await Message.Success("Cập nhật thành công!");
                }
                else
                {
                    var error = await response2.Content.ReadAsStringAsync();
                    await Message.Error($"Lỗi: {error}");
                }
            }
        }

        private class UploadResult
        {
            public string Url { get; set; }
        }
        private bool VisibleZoom {  get; set; }

        private async Task Zoom()
        {
            VisibleZoom = true;
            await Task.Delay(100);
            await JS.InvokeVoidAsync("zoomPanHandler.enableDrag");
            await InvokeAsync(StateHasChanged);
        }
     
        private async Task CloseZoom()
        {
            VisibleZoom = false;
            await Task.Delay(100);
            await JS.InvokeVoidAsync("zoomPanHandler.reset");
            StateHasChanged();

        }
    }
}
