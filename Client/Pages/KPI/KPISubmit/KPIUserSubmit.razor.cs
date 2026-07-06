using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.KPI.KPISubmit;
using AquaSolution.Client.Modals.KPI.Target;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.KPI.KPISubmit
{
    public partial class KPIUserSubmit
    {
        #region Declaration
        private UserDto? CurrenUser { get; set; }
        [Inject] private HttpClient? Http { get; set; }
        private SelectedKPISubmitModalrazor _selectedKpiSubmitModalrazor = new();
        private TargetModal targetModal = new();
        private List<ViewKPITotalScoreDto> DataSource { get; set; } = new();
        Table<ViewKPITotalScoreDto>? TableRef;
        private ApprovalTaskModal ApprovalTaskModalRef = new();
        private HubConnection? _hubConnection;
        private Guid PageId { get; set; }
        private bool IsLock { get; set; }
        #endregion
        #region Init    
        protected override async Task OnInitializedAsync()
        {
            await LoadCurrenUser();
            await GetPage();
            await CheckLock();
            await LoadData();
            await InitSignalRAsync();
            await ReloadIsLock();
        }
        private async Task InitSignalRAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                   .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                   .Build();
            _hubConnection.On("ReloadKPIForUserApproval", async () =>
            {
                await LoadData();
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
        }
        private async Task ReloadIsLock()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                .Build();
            _hubConnection.On<Guid>("IsLockSystem", async pageId =>
            {
                if (pageId == PageId)
                {
                    await CheckLock();
                    await InvokeAsync(StateHasChanged);
                }
            });
            await _hubConnection.StartAsync();
        }

        private async Task GetPage()
        {
            var url = "kpi-submit";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task CheckLock()
        {
            if (CurrenUser != null && CurrenUser.Roles.Any(x => x.Name == "Admin"))
            {
                IsLock = false;
                return;
            }
            IsLock = await Http.GetFromJsonAsync<bool>($"api/systemLock/check-lock/{PageId}");
        }
        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<ViewKPITotalScoreDto>>
                ($"api/KPISubmit/get-kpi-total-score-by-userid/{CurrenUser.Id}");

            if (result is not null)
            {
                DataSource = result;
            }
            else
            {
                DataSource = new();
            }
            StateHasChanged();
        }
        #endregion
        #region Action
        private async Task ViewTarget()
        {
            if(CurrenUser == null) { return; }
            await targetModal.ShowModal(CurrenUser);
        }
        private async Task SelectedKpi()
        {
            if(CurrenUser.PositionId == null)
            {
                Message.Error("Chưa có position. Vui lòng liên hệ IT hỗ trợ");
            }    
          await  _selectedKpiSubmitModalrazor.ShowModal(CurrenUser!);
        }
        private async Task ViewAsync(ViewKPITotalScoreDto kPiTotalScoreDto )
        {
            var approvalInfo = new ApprovalInfo();
            approvalInfo.SubmitId = kPiTotalScoreDto.SubmitId;
            await ApprovalTaskModalRef.ShowModalAsync(approvalInfo, true);
        }
        #endregion
    }
}
