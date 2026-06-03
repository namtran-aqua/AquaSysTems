using AntDesign;
using AntDesign.TableModels;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ITSuport.RequestITSuport;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ITSuport.RequestSuport
{
    public partial class RequestSuport
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }

        private List<RequestSuportDto> _requestSuportFillter = new();
        private HubConnection? _hubConnection;
        private List<UserContributerDto> _listTechnician = new();
        private readonly HasPermission _hasPermission = new();
        private RequestITSuportDetailModal _requestItSuportDetailModal = new();
        private UserDto? CurrenUser { get; set; }
        private RequestITSuport _requestItSuport = new();
        private List<AttachmentDto> _attachment = new();
        private bool Created { get; set; }
        private bool Loading { get; set; }
        private bool Edit { get; set; }
        private bool Delete { get; set; }
        private Guid PageId { get; set; }

        // Pagination
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _total = 0;

        // Filter text
        private string? RequesterName { get; set; }
        private string? Requesteremail { get; set; }
        private string? TicketCode { get; set; }

        // Filter multi-select
        private IEnumerable<string> _selectedStatuses = new List<string>();
        private IEnumerable<string> _selectedTechnicians = new List<string>();

        // Stats (tổng hợp riêng, không phụ thuộc vào page)
        private int _statTotal = 0;
        private int _statOpen = 0;
        private int _statInProgress = 0;
        private int _statResolved = 0;
        private int _statCancel = 0;
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }

            await SignalRReload();
            await GetPage();
            await Task.WhenAll(
                LoadData(),
                LoadTechnician(),
                LoadStats(),
                CheckPermission()
   
            );
            LoadStatusOptions();
        }

        private async Task SignalRReload()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                .Build();

            _hubConnection.On("LoadRequestSuport", async () =>
            {
                await LoadData();
                await LoadStats();
                await InvokeAsync(StateHasChanged);
            });

            await _hubConnection.StartAsync();
        }

        private async Task GetPage()
        {
            if (Http != null)
                PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/request-it-suport");
        }

        private async Task CheckPermission()
        {
            if (CurrenUser != null)
            {
                Created = await PermissionService.HasPermissionAsync(PageId, PermissionActionType.Add);
                Edit = await PermissionService.HasPermissionAsync(PageId, PermissionActionType.Edit);
                Delete = await PermissionService.HasPermissionAsync(PageId, PermissionActionType.Delete);
            }
        }
        #endregion

        #region LoadData
        private async Task LoadData()
        {
            Loading = true;
            StateHasChanged();

            if (Http == null || CurrenUser == null) return;

            var isITOrAdmin = CurrenUser.Roles.Any(x => x.Name == "IT" || x.Name == "Admin");

            var url = $"api/RequestITSuport/get-paged?" +
                      $"page={_currentPage}&pageSize={_pageSize}" +
                      $"&currentUserId={CurrenUser.Id}" +
                      $"&isITOrAdmin={isITOrAdmin}" +
                      $"&requesterName={Uri.EscapeDataString(RequesterName ?? "")}" +
                      $"&requesterEmail={Uri.EscapeDataString(Requesteremail ?? "")}" +
                      $"&ticketCode={Uri.EscapeDataString(TicketCode ?? "")}";

            foreach (var s in _selectedStatuses)
                url += $"&statuses={Uri.EscapeDataString(s)}";

            foreach (var t in _selectedTechnicians)
                url += $"&technicianNames={Uri.EscapeDataString(t)}";

            var data = await Http.GetFromJsonAsync<PagedResult<RequestSuportDto>>(url);
            if (data != null)
            {
                _requestSuportFillter = data.Items;
                _total = data.Total;
            }
            
            Loading = false;
            StateHasChanged();
        }

        // Load stats riêng — không bị ảnh hưởng bởi filter/pagination
        private async Task LoadStats()
        {
            if (Http == null || CurrenUser == null) return;

            var isITOrAdmin = CurrenUser.Roles.Any(x => x.Name == "IT" || x.Name == "Admin");
            var statsUrl = $"api/RequestITSuport/get-stats?currentUserId={CurrenUser.Id}&isITOrAdmin={isITOrAdmin}";
            var stats = await Http.GetFromJsonAsync<RequestSuportStatsDto>(statsUrl);
            if (stats != null)
            {
                _statTotal = stats.Total;
                _statOpen = stats.Open;
                _statInProgress = stats.InProgress;
                _statResolved = stats.Resolved;
                _statCancel = stats.Cancel;
            }
        }
        private List<string> _statusOptions = new();

  
        private void LoadStatusOptions()
        {
            _statusOptions = Enum.GetNames(typeof(RequestSuportStatusType)).ToList();
        }
        private async Task LoadTechnician()
        {
            if (Http == null) return;
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            if (data != null)
                _listTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
        }
        #endregion

        #region Actions
        private async Task CreatedAsync()
        {
            await _requestItSuport.ShowModal(false);
        }

        private async Task UpdateAsync(RequestSuportDto dto)
        {
            await _requestItSuport.ShowModal(true, dto);
        }

        private async Task ViewAsync(RequestSuportDto dto)
        {
            await _requestItSuportDetailModal.ShowModal(dto);
        }

        private async Task DeleteAsync(Guid id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm) return;

            await DeleteFileServer(id);
            var response = await Http!.DeleteAsync($"api/RequestITSuport/delete/{id}");

            if (response.IsSuccessStatusCode)
                await Message.Success("Delete successfully!");
            else
                await Message.Error("Delete failed!");

            await LoadData();
            await LoadStats();
        }

        private async Task DeleteFileServer(Guid requestSuportId)
        {
            if (Http == null) return;
            var data = await Http.GetFromJsonAsync<List<AttachmentDto>>($"api/RequestITSuport/get-attechment/{requestSuportId}");
            if (data == null) return;
            foreach (var item in data)
                await Http.DeleteAsync($"api/Common/delete-file-suport?avatarUrl={item.FilePath}");
        }
        #endregion

        #region Filter & Search
        private void RequesterNameInputChanged(ChangeEventArgs e) => RequesterName = e.Value?.ToString();
        private void RequesteremailInputChanged(ChangeEventArgs e) => Requesteremail = e.Value?.ToString();
        private void TicketCodeInputChanged(ChangeEventArgs e) => TicketCode = e.Value?.ToString();

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await Search();
        }

        private async Task Search()
        {
            _currentPage = 1;
            await LoadData();
        }

        private async Task Reset()
        {
            _currentPage = 1;
            RequesterName = "";
            Requesteremail = "";
            TicketCode = "";
            _selectedStatuses = new List<string>();
            _selectedTechnicians = new List<string>();
            await LoadData();
        }

        private async Task OnTableChange(QueryModel<RequestSuportDto> query)
        {
            _currentPage = query.PageIndex;
            _pageSize = query.PageSize;
            await LoadData();
        }
        #endregion
    }
}
