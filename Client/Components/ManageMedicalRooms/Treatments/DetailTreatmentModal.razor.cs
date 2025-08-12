using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace AquaSolution.Client.Components.ManageMedicalRooms.Treatments
{
    public partial class DetailTreatmentModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private MedicalHistoryDto _medicalHistoryDto = new MedicalHistoryDto();
        private bool IsVisible {  get; set; }
        #endregion
        #region Innit
        public async Task ShowModal(Guid requestId)
        {
            _medicalHistoryDto = new MedicalHistoryDto();
            await GetDataByRequest(requestId);
            IsVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task GetDataByRequest(Guid requestId)
        {
            _medicalHistoryDto = await Http.GetFromJsonAsync<MedicalHistoryDto>($"api/MyRequestClinic/get-history/{requestId}");
        }
        #endregion
        #region  Action
        private void Close()
        {
            IsVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
