using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.RequestClinics;
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
            MyRequestClinicData = await Http.GetFromJsonAsync<List<MyRequestClinicDto>>("api/MyRequestClinic/get-reuqest-by-user");
            MyRequestClinicData = MyRequestClinicData.Where(x => x.CreatedBy == CurrenUser.Id).ToList();
        }
        private async Task GetPage()
        {
            var baseUri = new Uri(Navigation.BaseUri);
            var uri = new Uri(Navigation.Uri);

            var basePath = baseUri.AbsolutePath.TrimEnd('/');
            var fullPath = uri.AbsolutePath;

            string currentPath;
            if (!string.IsNullOrEmpty(basePath))
                currentPath = fullPath.Replace(basePath, "");
            else
                currentPath = fullPath;
            currentPath = currentPath.TrimStart('/');
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{currentPath}");
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
        private async Task EditedAsync(MyRequestClinicDto myRequestClinicDto)
        {
            var paramShowModal = new ParamShowModal();
            paramShowModal.currenUser = CurrenUser;
            paramShowModal.users = users;
            paramShowModal.IsEdit = true;
            paramShowModal.handleMyRequestClinicDto = new HandleMyRequestClinicDto
            {
                Id = myRequestClinicDto.Id,
                WorkDayUserRequestId = myRequestClinicDto.WorkDayUserRequestId,
                UserRequestId = myRequestClinicDto.UserRequestId,
                UserRequestName = myRequestClinicDto.UserRequestName,
                RequestTitle = myRequestClinicDto.RequestTitle,
                PurposeType = myRequestClinicDto.PurposeType,
                ManagerId = myRequestClinicDto.ManagerId,
                ManagerName = myRequestClinicDto.ManagerName,
                EmailRequestter = myRequestClinicDto.EmailRequestter,
                WorkDayManager = myRequestClinicDto.WorkDayManager,
                Status = myRequestClinicDto.Status,
                ApprovalDate = myRequestClinicDto.ApprovalDate,
                ApprovalById = myRequestClinicDto.ApprovalById,
                ApprovalByName = myRequestClinicDto.ApprovalByName,
                RejectDate = myRequestClinicDto.RejectDate,
                RejectById = myRequestClinicDto.RejectById,
                RejectByName = myRequestClinicDto.RejectByName,
                SuccesDate = myRequestClinicDto.SuccesDate,
                PharmacyManagerId = myRequestClinicDto.PharmacyManagerId,
                PharmacyManagerName = myRequestClinicDto.PharmacyManagerName,
                CreatedDate = myRequestClinicDto.CreatedDate,
                Note = myRequestClinicDto.Note,
                HistoryReuqest = myRequestClinicDto.HistoryReuqest,
                CreatedBy = myRequestClinicDto.CreatedBy,
                CreatedName = myRequestClinicDto.CreatedName,
                CreatedWorkDay = myRequestClinicDto.CreatedWorkDay,

            };
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
