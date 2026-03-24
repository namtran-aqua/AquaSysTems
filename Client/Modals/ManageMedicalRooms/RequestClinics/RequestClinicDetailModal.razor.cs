using AntDesign;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.ManageMedicalRooms.RequestClinics
{
    public partial class RequestClinicDetailModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsVisible { get; set; }
        private bool IsView { get; set; }
        private bool IsApproval { get; set; }
        private bool IsRejected { get; set; }
        private MedicalHistoryDto MyRequestClinicDto { get; set; } = new MedicalHistoryDto();
        [Parameter] public EventCallback<Guid> IsApprovalEvent { get; set; }
        [Parameter] public EventCallback<Guid> IsRejectEvent { get; set; }

        private Guid RequestId { get; set; }
        #endregion
        #region Innit
        public async Task ShowModalAsync(Guid Id, bool isView,bool isApproval,bool isReject)
        {
            try
            {
                RequestId = Id;
                MyRequestClinicDto = new();
                MyRequestClinicDto = await Http.GetFromJsonAsync<MedicalHistoryDto>($"api/MyRequestClinic/get-history/{Id}"); ;
                IsView = isView;
                IsVisible = true;
                IsApproval = isApproval;
                IsRejected = isReject;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                throw ex;
            }
      

        }
        #endregion
        #region Action
        private async Task Approval()
        {
     
            await IsApprovalEvent.InvokeAsync(RequestId);
            IsVisible = false;
            StateHasChanged();
        }
        private async Task Rejected()
        {
     
            await IsRejectEvent.InvokeAsync(RequestId);
            IsVisible = false;
            StateHasChanged();
        }

        private void Close()
        {
            IsVisible = false;
            StateHasChanged();

        }
        #endregion

    }
}
