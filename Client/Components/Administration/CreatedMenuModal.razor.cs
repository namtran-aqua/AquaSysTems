using AntDesign;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Menus;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration
{
    public partial class CreatedMenuModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private HandleMenuDto HandleMenuDto = new HandleMenuDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<HandleMenuDto> formRef = new();
        private bool _visible = false;
        #endregion
        #region Innit 
        public void ShowModal()
        {
            IsModalVisible = true;
            HandleMenuDto = new HandleMenuDto();
            StateHasChanged();
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return; 
            }
            HandleMenuDto.Action = PermissionActionType.View;
            HandleMenuDto.Type = PermissionType.Menu;
            var response = await Http.PostAsJsonAsync($"api/menu/create", HandleMenuDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Menu created successfully.");
                await OnSave.InvokeAsync();
            }
            else
            {
                await Message.Error("Failed to create Menu.");
            }
            IsModalVisible = false;
        }
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
