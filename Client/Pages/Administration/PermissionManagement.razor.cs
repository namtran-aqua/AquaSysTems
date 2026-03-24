using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Permissions;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class PermissionManagement
    {
        #region Declaration

        private List<MenuDto>? _menus = new();
        private CreatedPermissionModal? _createdPermissionModal;
        #endregion
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            var menuTree = await HttpClient.GetFromJsonAsync<List<MenuDto>>($"api/permission/get-all");
            _menus = menuTree;
        }
        private async Task CreatedPermissionAsync()
        {
            await _createdPermissionModal?.ShowMidaleAsync()!;
        }
        private async Task DeleteAsync(HandlePermissionDto handlePermissionDto)
        {
            var message = $"Are you sure you want to delete the permission \" {handlePermissionDto.Action} \" ?";
            var confirm = await MessageBox.Confirm(Modal, message);
            if(confirm)
            {
                var response = await HttpClient.DeleteAsync
                    ($"api/permission/delete/{handlePermissionDto.Id}");
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await LoadData();
                    await Message.Success(content?.message ?? "Deleted successfully");
                }
                else
                {
                    await Message.Error(content?.message ?? "An unexpected error occurred");
                }
                await InvokeAsync(StateHasChanged);
            }    

        }
    }
}
