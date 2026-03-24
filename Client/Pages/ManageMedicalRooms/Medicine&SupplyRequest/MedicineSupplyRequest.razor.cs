using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ManageMedicalRooms.MedicineSupplyRequest;
using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.Medicine_SupplyRequest
{
    public partial class MedicineSupplyRequest
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<MedicineSupplyRequestDto> medicineSupplyRequestDtos =new List<MedicineSupplyRequestDto>();
        private MedicineSupplyRequestModal medicineSupplyRequestModal;
        private MedicineSupplyRequestDetailModal medicineSupplyRequestDetailModal;
        private UserDto CurrenUser { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            medicineSupplyRequestDtos = await Http.GetFromJsonAsync<List<MedicineSupplyRequestDto>>("api/MedicineSupplyRequest/get-all-master");
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action
        private async Task ViewAsync(MedicineSupplyRequestDto medicineSupplyRequestDto)
        {
          await  medicineSupplyRequestDetailModal.ShowModal(medicineSupplyRequestDto);
        }
        private async Task CreatedAsync()
        {
            await medicineSupplyRequestModal.ShowModal(new MedicineSupplyRequestDto(),false, CurrenUser);   

        }
 
        #endregion
    }
}
