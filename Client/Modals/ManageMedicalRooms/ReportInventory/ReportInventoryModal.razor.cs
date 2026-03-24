using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Modals.ManageMedicalRooms.ReportInventory
{
    public partial class ReportInventoryModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private UserDto CurrenUser { get; set; }
        private LoadReportInventoryDto loadReportInventory = new LoadReportInventoryDto();
        #endregion
        #region Innit 
        public async Task ShowModal()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await LoadReportInventory();
            IsModalVisible = true;
            StateHasChanged();
        }
        private async Task LoadReportInventory()
        {
            var data = await Http.GetFromJsonAsync<LoadReportInventoryDto>(
           $"api/Inventory/get-report");
            loadReportInventory = data;
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var data = await ConvertDataCreated();

            var response = await Http.PostAsJsonAsync("api/inventory/create-report", data);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Report submitted successfully.");
                IsModalVisible = false;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await Message.Error($"Failed to submit the report. {errorContent}");
            }
        }
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        private async Task<CreatedReportInventoryDto> ConvertDataCreated()
        {

            var createdReportInventory = new CreatedReportInventoryDto();
            createdReportInventory.CreatedBy = CurrenUser.Id;
            createdReportInventory.CreatedDate = DateTime.Now;
            createdReportInventory.Month = loadReportInventory.Month;
            createdReportInventory.Year = loadReportInventory.Year;
            createdReportInventory.LoadReportInventoryDetail = loadReportInventory.LoadReportInventoryDetail;

            return createdReportInventory;
        }
        #endregion
    }
}
