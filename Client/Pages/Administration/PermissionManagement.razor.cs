using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Pages;
using AquaSolution.Shared.Permissions;
using System.Net.Http;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class PermissionManagement
    {
        #region Declaration
        List<MenuDto>? Menus = new();
        private CreatedPermissionModal createdPermissionModal;
        #endregion
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            var menuTree = await httpClient.GetFromJsonAsync<List<MenuDto>>($"api/permission/get-all");
            Menus = menuTree;
        }
        private async Task CreatedPermissionAsync()
        {
            await createdPermissionModal.ShowMidaleAsync();
        }
        private async Task DeleteAsync(HandlePermissionDto HandlePermissionDto)
        {
            var message = $"Bạn có muốn xóa permission \" {HandlePermissionDto.Action} \" không?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if(confirm)
            {
                var response = await httpClient.DeleteAsync
                    ($"api/permission/delete/{HandlePermissionDto.Id}");
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await LoadData();
                    await Message.Success(content?.message);
                }
                else
                {
                    await Message.Success(content?.message);
                }
                await InvokeAsync(StateHasChanged);
            }    

        }
    }
}
