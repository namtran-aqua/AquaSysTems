
using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.WarehouseExport
{
    public partial class WarehouseExport
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<LoadWarehouseExportDto> _listWarehouseExport = new List<LoadWarehouseExportDto>();
        private WarehouseExportModal _warehouseExportModal { get; set; } = new();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
           _listWarehouseExport = await Http.GetFromJsonAsync<List<LoadWarehouseExportDto>>("api/WarehouseExport/get-all");
            await InvokeAsync(StateHasChanged);

        }
            #endregion
        #region Action
        private async Task ImportWarehouse()
        {
          await _warehouseExportModal.ShowModalAsync( new LoadWarehouseExportDto(),false);
        }
        private async Task View (LoadWarehouseExportDto loadWarehouseExport)
        {
           await _warehouseExportModal.ShowModalAsync(loadWarehouseExport, true);
        }
        #endregion
    }
}
