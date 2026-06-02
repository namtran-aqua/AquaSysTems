using AntDesign;
using AquaSolution.Shared.ScrapManagement.Materials;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Modals.ScrapManagement.Materials
{
    public partial class DetailMaterialModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IMessageService Message { get; set; }


        private bool IsModalVisible = false;
        private List<WeightDto> ListWeight = new();
        private Guid CurrentMaterialId { get; set; }
        private MaterialDto Material { get; set; }
        #endregion

        #region Init
        public async Task ShowModal(MaterialDto material)
        {
            Material = material;
            CurrentMaterialId = material.Id;
            await LoadScheduledWeightsAsync();
            IsModalVisible = true;
            StateHasChanged();
        }
        #endregion

        #region Actions
        private async Task LoadScheduledWeightsAsync()
        {
            try
            {
                var data = await Http.GetFromJsonAsync<List<WeightDto>>(
                    $"api/Material/get-scheduled-weights/{CurrentMaterialId}") ?? new();
                ListWeight = data.ToList();
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải dữ liệu weight: {ex.Message}");
            }
        }

        private void Close()
        {
            IsModalVisible = false;
        }
        #endregion
    }
}
