using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.Treatments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.ClinicCheckIns
{
    public partial class ClinicCheckIn
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<MyRequestClinicDto> RequestClinicUser { get; set; } = new List<MyRequestClinicDto>();
        private List<MyRequestClinicDto> FilterRequestClinicUser { get; set; } = new List<MyRequestClinicDto>();

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
            _hubConnection.On("LoadRequestClinic", async () =>
            {
                await LoadData();
                await Search();
                StateHasChanged();
            });
            await _hubConnection.StartAsync();
            await LoadData();
            await LoadEnum();
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
            FilterRequestClinicUser = RequestClinicUser;
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
        #region Filter
        TableFilter<StatusClinicType>[] _statusFilter = Array.Empty<TableFilter<StatusClinicType>>();
       
        TableFilter<PurposeType>[] _purposeTypeFilter = Array.Empty<TableFilter<PurposeType>>();
        private string? RequesterName { get; set; }
        private async Task LoadEnum()
        {
            _statusFilter = Enum.GetValues(typeof(StatusClinicType))
                .Cast<StatusClinicType>()
                .Where(e => e == StatusClinicType.Approval || e == StatusClinicType.Done) // chỉ lấy 2 giá trị
                .Select(e => new TableFilter<StatusClinicType>
                {
                    Text = EnumHelper.GetDisplayName(e),
                    Value = e,
                    Selected = false
                })
                .ToArray();


            _purposeTypeFilter = Enum.GetValues(typeof(PurposeType))
               .Cast<PurposeType>()
               .Select(e => new TableFilter<PurposeType>
               {
                   Text = EnumHelper.GetDisplayName(e),
                   Value = e,
                   Selected = false
               })
               .ToArray();


        }
        private async Task Search()
        {
            var name = StringHelper.NormalizeText(RequesterName?.Trim());

            var filtered = RequestClinicUser
                .Where(x =>
                    (string.IsNullOrWhiteSpace(name) ||
                        (!string.IsNullOrWhiteSpace(x.UserRequestName) &&
                         StringHelper.NormalizeText(x.UserRequestName).Contains(name)))
                )
                .ToList();

            if (string.IsNullOrWhiteSpace(name))
            {
                filtered = RequestClinicUser;
            }
            FilterRequestClinicUser = filtered;
        }
        private async Task Reset()
        {
            RequesterName = null;
            FilterRequestClinicUser = RequestClinicUser;
            tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);

        }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void RequesterNameInputChanged(ChangeEventArgs e)
        {
            RequesterName = e.Value?.ToString();
        }


        private Table<MyRequestClinicDto> tableRef;
        #endregion
    }
}
