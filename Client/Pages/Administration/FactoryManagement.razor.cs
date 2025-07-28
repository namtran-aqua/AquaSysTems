using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Factory;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Factory;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class FactoryManagement
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<FactoryDto> ListFactory = new();
        private FactoryModal factoryModal = new FactoryModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            ListFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task CreatedFactory()
        {
            await factoryModal.Showmodal(new FactoryDto(), false);
        }
        private async Task EditFactory(FactoryDto factoryDto)
        {
            await factoryModal.Showmodal(factoryDto, true);
        }
        private async Task DeleteAsync(FactoryDto factoryDto)
        {
            var message = $"Bạn có muốn xóa factory \" {factoryDto.Name} \" không?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/factory/delete/{factoryDto.Id}");
                await LoadDataAsync();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Xóa thành công");
                }
                else
                {
                    await Message.Error(content?.message ?? "Có lỗi xảy ra");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion

    }
}
