using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Roles;
using AquaSolution.Shared.Roles;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class RoleManagement
    {
        private List<RoleDto>? _roles = new();
        private PermissionModal? _permissionModal;
        [Inject] private HttpClient? Http { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }
        private  async Task LoadRoles()
        {

            try
            {
                if (Http != null) _roles = await Http.GetFromJsonAsync<List<RoleDto>>("api/roles/get-all");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error loading roles: {ex.Message}");
            }
            StateHasChanged();
        }
        private void ShowAddRole()
        {
            IsVisible = true;
        }
        private async Task  DeleteRoleAsync(RoleDto role)
        {

            var message = $"Are you sure you want to delete the department \"{role.Name}\"?";
            var confirm = await MessageBox.Confirm(Modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/roles/delete-role/{role.Id}")!;
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Role deleted successfully.");
                    await LoadRoles();
                    StateHasChanged();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Failed to delete: {error}");
                }
            }

            
        }
        private RolePermissionDetailModal rolePermissionDetailModal = new();
        private async Task  ShowPermissionModal(RoleDto role)
        {
          await  _permissionModal?.Show(role)!;
        }
        private async Task DetailPermissionRole(RoleDto role)
        {
            await rolePermissionDetailModal.ShowModal(role);
        }
        #region Modal Add Role
        public bool IsVisible { get; set; }
        private HandleRoleDto CreatedRoleDto { get; set; } = new HandleRoleDto();
        private async Task Save()
        {
            if (Http != null)
            {
                var response = await Http.PostAsJsonAsync($"api/roles/create", CreatedRoleDto);
                if (response.IsSuccessStatusCode)
                {
                    await LoadRoles();
                    await Message.Success("Role created successfully.");
                }
                else
                {
                    await Message.Error("Failed to create role.");
                }
            }

            IsVisible = false;
        }
        private void Close()
        {
            IsVisible = false;
        }
        #endregion
    }
}
