using AntDesign;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Pages;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration
{
    public partial class CreatedPagesModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private HandlePageDto HandlePageDto = new HandlePageDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<HandlePageDto> formRef = new();
        private bool _visible = false;
        private Guid MenuId { get; set; }
        #endregion
        #region Innit 
        public void ShowModal(Guid menuId)
        {
            MenuId = menuId;
            IsModalVisible = true;
            HandlePageDto = new HandlePageDto();
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
            HandlePageDto.MenuId = MenuId;
            var response = await Http.PostAsJsonAsync($"api/page/create", HandlePageDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Page created successfully.");
                await OnSave.InvokeAsync();
            }
            else
            {
                await Message.Error("Failed to create Page.");
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
