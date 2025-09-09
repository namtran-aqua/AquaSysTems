using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.Treatments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.ClinicCheckIns
{
    public partial class ClinicCheckIn
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<MedicalHistoryDto> RequestClinicData { get; set; } = new List<MedicalHistoryDto>();
        private List<MyRequestClinicDto> RequestClinicUser { get; set; } = new List<MyRequestClinicDto>();

        private HasPermission hasPermission = new();
        private Guid PageId { get; set; }
        private UserDto CurrenUser { get; set; }
        private Treatment treatment { get; set; }
        private DetailTreatmentModal DetailTreatmentModal { get; set; }
        private HubConnection? _hubConnection;
        private List<UserDto> users = new();
        [Inject] NavigationManager NavigationManager { get; set; }
        #endregion
        #region Innit   
        protected override async Task OnInitializedAsync()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            _hubConnection = new HubConnectionBuilder()
             .WithUrl(NavigationManager.ToAbsoluteUri(NavigationManager.BaseUri + "signalrhub"))
            .Build();
            _hubConnection.On("ChangeStatusRequestClinic", async () =>
            {
                await LoadData();
                StateHasChanged();
            });
            await _hubConnection.StartAsync();
            await LoadData();
            await CheckPermission();
        }
        private async Task LoadData()
        {
            users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
            RequestClinicUser = await Http.GetFromJsonAsync<List<MyRequestClinicDto>>("api/MyRequestClinic/get-reuqest-by-user");
            if (RequestClinicUser != null)
            {
                RequestClinicUser = RequestClinicUser.Where(x => x.Status == StatusClinicType.Approval || x.Status == StatusClinicType.Done).ToList();
            }
        }
        private async Task GetPage()
        {
            var url = "clinic-check-in-management";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task CheckPermission()
        {
            await GetPage();
        }
        #endregion
        #region Action

        private async Task Treatment(MyRequestClinicDto myRequestClinicDto)
        {
            var createdTreatment = new CreatedTreatmentDto();
            createdTreatment.PharmacyManagerId = CurrenUser.Id;
            createdTreatment.RequestId = myRequestClinicDto.Id;
            createdTreatment.PatientName = myRequestClinicDto.UserRequestName;
            createdTreatment.RequestId = myRequestClinicDto.Id;
            await treatment.OpenModal(createdTreatment, false);
        }
        private async Task View(Guid Id)
        {
            await DetailTreatmentModal.ShowModal(Id);
        }
        #endregion
    }
}
