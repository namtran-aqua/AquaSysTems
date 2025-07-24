using AntDesign;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace AquaSolution.Client.Components.Users
{
    public partial class UserDetailModal
    {
        private bool Visible { get; set; }
        private UserDto User { get; set; }

        private string UserAvatarUrl { get; set; } = "https://via.placeholder.com/150"; // Default or from DB
        private Upload uploadRef;
        public async Task ShowModal(UserDto userDto)
        {
            User = userDto;
            Visible = true;
            StateHasChanged();
        }
        private async Task SaveAsync()
        {
            // Gọi API để cập nhật thông tin người dùng hoặc avatar
            //await Notification.Open(new NotificationConfig()
            //{
            //    Message = "Lưu thành công",
            //    Description = $"{User.FullName} đã được cập nhật.",
            //    Type = NotificationType.Success
            //});

        }

        private void Cancel()
        {
            Visible = false;
        }

        private void OnUploadChanged(UploadInfo info)
        {
            if (info.File.State == UploadState.Success)
            {
                var url = info.File.Response?.ToString();
                if (!string.IsNullOrEmpty(url))
                {
                    UserAvatarUrl = url;
                }
            }
        }

        private class UploadResult
        {
            public string Url { get; set; }
        }

    }
}
