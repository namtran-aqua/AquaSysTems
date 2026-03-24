using AntDesign;
using AquaSolution.Shared.AuthModels;
using AquaSolution.Shared.CommonDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.Users
{
    public partial class ChangePassModal
    {
        [Inject] private HttpClient Http { get; set; }
        private bool _visible;
        private ChangePassRequest _model = new();
        private Form<ChangePassRequest> formRef;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        private Guid UserId { get; set; }
        public void ShowModal(Guid userId)
        {
            UserId = userId;
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
            _model.UserId = UserId;
            var response = await Http.PostAsJsonAsync("api/auth/change-password", _model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                _visible = false;
                await Message.Success(result?.message);
                await Logout();
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse>();
                await Message.Error(error?.message);
            }
        
        }
        private async Task Logout()
        {
            await Http.PostAsync("api/auth/logout", null);
            await JSRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            var baseUri = Nav.BaseUri.TrimEnd('/');
            Nav.NavigateTo($"{baseUri}/login", true);
        }
    }
}
