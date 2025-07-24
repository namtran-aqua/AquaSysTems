using AquaSolution.Client.Components.Roles;
using AquaSolution.Shared.Roles;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class RoleManagement
    {
        List<RoleDto> Roles = new();
        private PermissionModal permissionModal;
        [Inject] private HttpClient Http { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }
        private  async Task LoadRoles()
        {

            try
            {
                Roles = await Http.GetFromJsonAsync<List<RoleDto>>("api/roles/get-all");
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
            var response = await Http.DeleteAsync($"api/roles/delete-role/{role.Id}");
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

       private async Task  ShowPermissionModal(RoleDto role)
        {
          await  permissionModal.Show(role);
        }

        #region Modal Add Role
        public bool IsVisible { get; set; }
        private HandleRoleDto createdRoleDto { get; set; } = new HandleRoleDto();
        private async Task Save()
        {
            var response = await Http.PostAsJsonAsync($"api/roles/create", createdRoleDto);
            if (response.IsSuccessStatusCode)
            {
                await LoadRoles();
                await Message.Success("Role created successfully.");
            }
            else
            {
                await Message.Error("Failed to create role.");
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
