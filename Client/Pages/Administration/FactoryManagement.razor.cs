using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Factory;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Factory;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class FactoryManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<FactoryDto>? _listFactory = new();
        private FactoryModal _factoryModal = new FactoryModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            if (Http != null) _listFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task CreatedFactory()
        {
            await _factoryModal.Showmodal(new FactoryDto(), false);
        }
        private async Task EditFactory(FactoryDto factoryDto)
        {
            await _factoryModal.Showmodal(factoryDto, true);
        }
        private async Task DeleteAsync(FactoryDto factoryDto)
        {
            var message = $"Are you sure you want to delete the factory \" {factoryDto.Name} \"?";
            var confirm = await MessageBox.Confirm(Modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/factory/delete/{factoryDto.Id}")!;
                await LoadDataAsync();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Deleted successfully");
                }
                else
                {
                    await Message.Error(content?.message ?? "An unexpected error occurred");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion

    }
}
