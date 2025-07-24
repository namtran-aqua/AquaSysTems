
using AquaSolution.Shared.Permissions;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Data;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Roles
{
    public partial class PermissionModal
    {
        [Inject] private HttpClient? Http { get; set; }
        public bool IsVisible { get; set; }
        public RoleDto Role { get; set; } = new();
        [Parameter] public EventCallback<RoleDto> OnSave { get; set; }
        private List<GroupedPermissionDto> listAllPermission = new List<GroupedPermissionDto>();
        private List<GroupedPermissionDto> listPerrmissionByRole = new List<GroupedPermissionDto>();

        private bool IsLoading = false;
        private bool HasError = false;
        private string ErrorMessage = "";
        private HashSet<Guid> SelectedPermissions = new();
        public async Task Show(RoleDto roleDto)
        {
            Role = roleDto;
            IsVisible = true;
            OnParametersSet();
            StateHasChanged();
            await GetPermission();
        }
        private async Task GetPermission()
        {
            listAllPermission = await Http.GetFromJsonAsync<List<GroupedPermissionDto>>("api/permission/get-all-permission");
            listPerrmissionByRole = await Http.GetFromJsonAsync<List<GroupedPermissionDto>>($"api/permission/get-all-permission-role/{Role.Id}");
            var selectedIds = listPerrmissionByRole
                .SelectMany(g => g.Permissions.Append(new PermissionDto { PermissionId = g.PermissionId }))
                .Select(p => p.PermissionId)
                .ToHashSet();
            foreach (var group in listAllPermission)
            {
                group.IsChecked = selectedIds.Contains(group.PermissionId);

                foreach (var perm in group.Permissions)
                {
                    perm.IsChecked = selectedIds.Contains(perm.PermissionId);
                }
            }
            SelectedPermissions = selectedIds;

        }
        private void TogglePermission(bool isChecked, Guid permissionId)
        {
            var permission = listAllPermission
                .SelectMany(g => g.Permissions)
                .FirstOrDefault(p => p.PermissionId == permissionId);

            if (permission != null)
            {
                permission.IsChecked = isChecked;
            }
            if (isChecked)
            {
                if (!SelectedPermissions.Contains(permissionId))
                    SelectedPermissions.Add(permissionId);
            }
            else
            {
                SelectedPermissions.Remove(permissionId);
            }
        }
        private async Task Save()
        {
            IsLoading = true;
            var response = await Http.PostAsJsonAsync($"api/RolePermission/update/{Role.Id}", SelectedPermissions);
            IsVisible = false;
            IsLoading = false;
        }
        private async Task Close()
        {
            IsVisible = false;
            SelectedPermissions = new();
        }

    }
}
