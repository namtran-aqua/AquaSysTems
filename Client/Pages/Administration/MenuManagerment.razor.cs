using AquaSolution.Client.Components.Administration;
using AquaSolution.Shared.Menus;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class MenuManagerment
    {
        #region Declaration
        List<MenuDto>? Menus = new();
        private CreatedMenuModal? createdMenuModal;
        private CreatedPagesModal? createdPagesModal;

        #endregion
        protected override async Task OnInitializedAsync()
        {
            await LoadMenu();
        }
        private async Task LoadMenu()
        {
            var menuTree = await httpClient.GetFromJsonAsync<List<MenuDto>>($"api/Menu/GetAllMenu");
            Menus = menuTree;
        }
        private Task CreatedMenu()
        {
            createdMenuModal?.ShowModal();
            return Task.CompletedTask;
        }
 
        private Task CreatedPage(MenuDto menuDto)
        {
            createdPagesModal?.ShowModal(menuDto.Id);
            return Task.CompletedTask;
        }

    }
}
