using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ManageMedicalRooms.InventoryPeriod;
using AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.InventoryPeriod
{
    public partial class InventoryPeriod
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private UserDto CurrenUser { get; set; }
        private InventoryPeriodModal InventoryPeriodModal = new();
        private CreatedInventoryPeriodDto CreatedInventoryPeriodDto { get; set; }
        private List<InventoryPeriodDto> inventoryPeriodDtos { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            inventoryPeriodDtos = await Http.GetFromJsonAsync<List<InventoryPeriodDto>>("api/InventoryPeriod/get-inventory-period");
            await InvokeAsync(StateHasChanged);

        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            var createdInventoryPeriod = new CreatedInventoryPeriodDto();
           
            await InventoryPeriodModal.ShowModalAsync(createdInventoryPeriod, CurrenUser.Id,false);
        }
        private async Task View (InventoryPeriodDto inventoryPeriodDto) 
        {
            CreatedInventoryPeriodDto = new();
            CreatedInventoryPeriodDto.InventoryPeriodDto = inventoryPeriodDto;
            await InventoryPeriodModal.ShowModalAsync(CreatedInventoryPeriodDto, CurrenUser.Id, true);
        }
        #endregion
    }
}
