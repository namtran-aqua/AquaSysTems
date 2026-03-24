using AntDesign;
using AquaSolution.Shared.AuthModels;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.Users
{
    public partial class ResetPassModal
    {
        [Inject] private HttpClient Http { get; set; }
        private bool _visible;
        private ResetPassword _model = new();
        private Form<ResetPassword> formRef;
        private string Title { get;set; }
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        private UserDto User { get; set; }
        public async Task ShowModal(UserDto user)
        {
            User = user;
            Title = $"Reset Password for {User.FullName}";
            _visible = true;
            StateHasChanged();
        }
        private void HandleCancel() => _visible = false;

        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            _model.UserId = User.Id;
            var response = await Http.PostAsJsonAsync("api/auth/reset-password", _model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                _visible = false;
                await Message.Success(result?.message);
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse>();
                await Message.Error(error?.message);
            }
        
        }
      
    }
}
