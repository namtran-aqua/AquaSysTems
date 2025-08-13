using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.RequestEmployee
{
    public partial class RequestEmployee
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<MyRequestClinicDto> MyRequestClinicData { get; set; } = new List<MyRequestClinicDto>();
        private HasPermission hasPermission = new();
        private Guid PageId { get; set; }
        private bool Approval { get; set; }
        private bool Rejected { get; set; }
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
            if (MyRequestClinicData != null)
            {
                MyRequestClinicData = MyRequestClinicData.Where(x => x.ManagerId == CurrenUser.Id && x.Status == StatusClinicType.New).ToList();
            }
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
            Approval = await hasPermission.CheckPermissions(PageId, PermissionActionType.Approve.ToString(), CurrenUser);
            Rejected = await hasPermission.CheckPermissions(PageId, PermissionActionType.Reject.ToString(), CurrenUser);

        }
        #endregion
        #region Action
        private async Task ApprovalAsync(Guid reuqestId)
        {
            var dto = new UpdateRequestClinicStatusDto
            {
                RequestClinicId = reuqestId,
                UserId = CurrenUser.Id,
            };
            var response = await Http.PostAsJsonAsync("api/MyRequestClinic/approval", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (response.IsSuccessStatusCode)
            {
                await Message.Success(result?.message ?? "Approved successfully.");
            }
            else
            {
                await Message.Error(result?.message ?? "Approved unsuccessful.");
            }
            await LoadData();
        }
        private async Task RejectedAsync(Guid reuqestId)
        {
            var dto = new UpdateRequestClinicStatusDto
            {
                RequestClinicId = reuqestId,
                UserId = CurrenUser.Id,
            };
            var response = await Http.PostAsJsonAsync("api/MyRequestClinic/reject", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (response.IsSuccessStatusCode)
            {
                await Message.Success(result?.message ?? "Rejected successfully.");
            }
            else
            {
                await Message.Error(result?.message ?? "Rejected unsuccessful.");
            }
            await LoadData();
        }
        private async Task View(Guid reuqestId)
        {
            await requestClinicDetailModal.ShowModalAsync(reuqestId, false, Approval, Rejected);
        }
        #endregion
    }
}
