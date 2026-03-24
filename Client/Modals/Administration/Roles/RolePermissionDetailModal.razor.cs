using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.Roles
{
    public partial class RolePermissionDetailModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private RoleDto _role = new RoleDto();
        private Guid _roleId = Guid.Empty;
        #endregion
        #region Innit
        public async Task ShowModal(RoleDto roleDto)
        {
            _roleId = roleDto.Id;
            _role = new();
            await LoadDetail();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action and Event
        private void HandleCancel() => IsModalVisible = false;
        private async Task LoadDetail()
        {
            if (Http != null) _role = await Http.GetFromJsonAsync<RoleDto>($"api/RolePermission/get-permission-by-id/{_roleId}");
            var a = _role;
        }
        #endregion
    }
}
