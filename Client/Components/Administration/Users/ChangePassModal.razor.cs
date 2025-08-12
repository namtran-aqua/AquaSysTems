using AntDesign;
using AquaSolution.Shared.AuthModels;
using AquaSolution.Shared.CommonDto;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration.Users
{
    public partial class ChangePassModal
    {
        [Inject] private HttpClient Http { get; set; }
        private bool _visible;
        private ChangePassRequest _model = new();
        private Form<ChangePassRequest> formRef;
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
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse>();
                await Message.Error(error?.message);
            }
        
        }
    }
}
