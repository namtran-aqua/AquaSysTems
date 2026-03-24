using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.MyRequestClinics
{
    public partial class MyRequestClinic
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<MyRequestClinicDto> MyRequestClinicData { get; set; } =new List<MyRequestClinicDto>();
        private HasPermission hasPermission = new();
        private Guid PageId { get; set; }
        private bool Created { get; set; } = true;
        private bool Edited { get; set; }
        private bool Deleted { get; set; }
        private UserDto CurrenUser { get; set; }
        private MyRequestClinicModal myRequestClinicModal { get; set; }
        private RequestClinicDetailModal requestClinicDetailModal { get; set; }
        private HubConnection? _hubConnection;
        [Inject] NavigationManager NavigationManager { get; set; }
        private List<UserDto> users = new();
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
                StateHasChanged();
            });
            await _hubConnection.StartAsync();
            await LoadData();
            await CheckPermission();
        }
        private async Task LoadData()
        {
            users = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
            MyRequestClinicData = await Http.GetFromJsonAsync<List<MyRequestClinicDto>>("api/MyRequestClinic/get-reuqest-by-user");
            MyRequestClinicData = MyRequestClinicData.Where(x => x.CreatedBy == CurrenUser.Id).ToList();
        }
        private async Task GetPage()
        {
            var url = "my-request-clinic-management";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task CheckPermission()
        {
            await GetPage();
        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            var paramShowModal = new ParamShowModal();
            paramShowModal.currenUser = CurrenUser;
            paramShowModal.users = users;
            paramShowModal.IsEdit = false;
            paramShowModal.handleMyRequestClinicDto =new HandleMyRequestClinicDto();
            await myRequestClinicModal.ShowModalAsync(paramShowModal);
        }
        private async Task DeletedAsync(MyRequestClinicDto myRequestClinicDto)
        {

            var message = $"Do you want to delete the request \"{myRequestClinicDto.RequestTitle}\"?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/MyRequestClinic/Delete/{myRequestClinicDto.Id}");
                await LoadData();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Deleted successfully");
                }
                else
                {
                    await Message.Error(content?.message ?? "Delete operation failed");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        private async Task View(MyRequestClinicDto myRequestClinicDto)
        {

            await requestClinicDetailModal.ShowModalAsync(myRequestClinicDto.Id,true,false,false);
        }

        #endregion
    }
}
