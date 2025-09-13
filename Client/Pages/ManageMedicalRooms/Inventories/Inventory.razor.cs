using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.Inventories
{
    public partial class Inventory
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<InventoryDto> _listInventory = new List<InventoryDto>();
        private WarehouseImportModal _warehouseImportModal { get; set; } = new();
        private WarehouseExportModal _warehouseExportModal { get; set; } = new();

        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            _listInventory = await Http.GetFromJsonAsync<List<InventoryDto>>("api/Inventory/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action
        private async Task ImportWarehouse()
        {
            await _warehouseImportModal.ShowModalAsync(new LoadWarehouseImportDto(), false);
        }
        private async Task ExportWarehouse()
        {
            await _warehouseExportModal.ShowModalAsync(new LoadWarehouseExportDto(), false);
        }
        #endregion
    }
}
