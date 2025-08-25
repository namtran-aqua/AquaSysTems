using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.ManageMedicalRooms.MedicineSupplyRequest
{
    public partial class MedicineSupplyRequestDetailModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private MedicineSupplyRequestDto MedicineSupplyRequestDto { get; set; }
        private List<MedicineSupplyRequestDetailDto> medicineSupplyRequestDetailDtos { get; set; }
        private bool IsVisibleModal { get; set; }
        #endregion
        #region Innit
        public async Task ShowModal(MedicineSupplyRequestDto medicineSupplyRequestDto)
        {

            MedicineSupplyRequestDto = medicineSupplyRequestDto;
            await LoadDataDetail();
            IsVisibleModal = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadDataDetail()
        {
            medicineSupplyRequestDetailDtos = await Http.GetFromJsonAsync<List<MedicineSupplyRequestDetailDto>>($"api/MedicineSupplyRequest/get-detail-by-medicinsuplyrequestid/{MedicineSupplyRequestDto.Id}");
        }


        #endregion
        #region Action
      
        private async Task Close()
        {
            IsVisibleModal = false;
        }

        #endregion
    }
}
