using AntDesign;
using AquaSolution.Client.Components.ManageMedicalRooms.ReportInventory;
using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Client.Components.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.Inventories
{
    public partial class Inventory
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<InventoryDto> _listInventory = new List<InventoryDto>();
        private List<InventoryDto> _listInventoryFilter = new List<InventoryDto>();
        private ReportInventoryModal reportInventoryModal;
        private Table<InventoryDto> tableRef;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            _listInventory = await Http.GetFromJsonAsync<List<InventoryDto>>("api/Inventory/get-all");
            _listInventoryFilter =_listInventory.ToList();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action
        private async Task ExportReportMonth()
        {
            await reportInventoryModal.ShowModal();
        }
        #endregion
        #region Search
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private string? ProductName { get; set; }
        private void ProductNameChange(ChangeEventArgs e)
        {
            ProductName = e.Value?.ToString();
        }
        private async Task Search()
        {
            var productName = StringHelper.NormalizeText(ProductName?.Trim());
            var filtered = _listInventory
                .Where(x =>
                    (string.IsNullOrWhiteSpace(productName) ||
                    (!string.IsNullOrEmpty(x.ProductName) && 
                    StringHelper.NormalizeText(x.ProductName).Contains(productName)))
                )
                .ToList();

            if (string.IsNullOrWhiteSpace(productName))
            {
                filtered = _listInventory;
            }
            _listInventoryFilter = filtered;
        }

        private async Task Reset()
        {
            ProductName = null;
            _listInventoryFilter = _listInventory;
            tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
