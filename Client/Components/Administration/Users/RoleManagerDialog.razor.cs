using AquaSolution.Shared.Roles;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration.Users
{
    public partial class RoleManagerDialog
    {
        [Parameter] public EventCallback OnSave { get; set; }

        // State
        private List<RoleDto> Roles { get; set; } = new();
        private bool IsVisible { get; set; }
        private bool IsLoading { get; set; }
        private bool HasError { get; set; }
        private string ErrorMessage { get; set; }
        private Guid UserId { get; set; }
        private string Username { get; set; }

        [Inject] private HttpClient Http { get; set; }

        public void Show(UserDto user)
        {
            UserId = user.Id;
            Username = user.FullName;
            LoadRoles(user.Roles);
            IsVisible = true;
            StateHasChanged();
        }

        private async void LoadRoles(List<RoleDto> userRoles)
        {
            IsLoading = true;
            HasError = false;
            Roles = new List<RoleDto>();
            try
            {
                Roles = await Http.GetFromJsonAsync<List<RoleDto>>("api/roles/get-all");
                foreach (var role in Roles)
                {
                    role.IsSelected = userRoles.Select(x=>x.Name).Contains(role.Name);
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Không thể tải danh sách role";
                Console.WriteLine($"Error loading roles: {ex.Message}");
            }

            IsLoading = false;
            StateHasChanged();
        }

        private void ToggleRole(RoleDto role)
        {
            role.IsSelected = !role.IsSelected;
        }

        private void Close()
        {
            IsVisible = false;
            Roles = new List<RoleDto>();
            UserId = Guid.Empty;
            Username = null;
        }

        private async Task Save()
        {
            IsLoading = true;
            try
            {
                var selectedRoles = new List<RoleDto>();
                var listRoleUpdate = new List<UpdateUserRoleDto>();
                selectedRoles = Roles
                    .Where(r => r.IsSelected)
                    .ToList();
                if (selectedRoles.Count > 0)
                {
                    foreach (var item in selectedRoles)
                    {
                        listRoleUpdate.Add(new UpdateUserRoleDto
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Permissions =item.Permissions,
                            IsSelected = item.IsSelected
                        });
                    }
                }



                var response = await Http.PostAsJsonAsync(
                        $"api/roles/{UserId}/Update-user-role",
                        selectedRoles
                    );
                if (response.IsSuccessStatusCode)
                {
                    Close();
                    await OnSave.InvokeAsync();
                }
                else
                {
                    ErrorMessage = "Cập nhật không thành công";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi khi cập nhật role";
                HasError = true;
                Console.WriteLine($"Error saving roles: {ex.Message}");
            }

            IsLoading = false;
        }
    }
}


