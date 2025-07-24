using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Users
{
    public partial class UserEditDialog
    {

        [Parameter] public EventCallback OnSave { get; set; }

        // State
        private UserDto User { get; set; } = new();
        private string Password { get; set; }
        private bool IsVisible { get; set; }
        private bool IsLoading { get; set; }
        private bool IsEditing => User.Id !=Guid.Empty;
        private string Title => IsEditing ? "Sửa người dùng" : "Thêm người dùng";

        [Inject] private HttpClient Http { get; set; }

        public void Show(UserDto user)
        {
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FullName,
                Email = user.Email
            };
            Password = string.Empty;
            IsVisible = true;
            StateHasChanged();
        }

        private void Close()
        {
            IsVisible = false;
        }

        private async Task Save()
        {
            IsLoading = true;
            try
            {
                HttpResponseMessage response;

                if (IsEditing)
                {
                    response = await Http.PutAsJsonAsync($"api/users/{User.Id}", User);
                }
                else
                {
                    var newUser = new
                    {
                        User.FullName,
                        User.Email,
                        Password
                    };
                    response = await Http.PostAsJsonAsync("api/users", newUser);
                }

                if (response.IsSuccessStatusCode)
                {
                    Close();
                    await OnSave.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user: {ex.Message}");
            }
            IsLoading = false;
        }
    }

}

