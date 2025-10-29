using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.WarehouseImport
{
    public partial class WarehouseImport
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<LoadWarehouseImportDto> _listWarehouseImport = new List<LoadWarehouseImportDto>();
        private WarehouseImportModal _warehouseImportModal { get; set; } = new();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
           _listWarehouseImport = await Http.GetFromJsonAsync<List<LoadWarehouseImportDto>>("api/WarehouseImport/get-all");
            await InvokeAsync(StateHasChanged);

        }
        #endregion
        #region Action
        private async Task ImportWarehouse()
        {
          await _warehouseImportModal.ShowModalAsync( new LoadWarehouseImportDto(),false);
        }
        private async Task View (LoadWarehouseImportDto loadWarehouseImport)
        {
            await _warehouseImportModal.ShowModalAsync(loadWarehouseImport, true);
        }
        #endregion
    }
}
