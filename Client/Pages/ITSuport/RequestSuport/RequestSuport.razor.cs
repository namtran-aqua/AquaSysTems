
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ITSuport.RequestITSuport;
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

        private List<RequestSuportDto> _requestSuport = new();
        private List<RequestSuportDto> _requestSuportFillter = new();
        private HubConnection? _hubConnection;
        private List<UserContributerDto> _listTechnician = new List<UserContributerDto>();
        private readonly HasPermission _hasPermission = new();
        private RequestITSuportDetailModal _requestItSuportDetailModal = new();
        private UserDto? CurrenUser { get; set; }
        private RequestITSuport _requestItSuport = new();
        private List<AttachmentDto> _attachment = new();
        private bool Created { get; set; }
        private bool Edit { get; set; }
        private bool Delete { get; set; }
        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
           // _hubConnection = new HubConnectionBuilder()
           //.WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
           //.Build();
           // _hubConnection.On("LoadRequestSuport", async () =>
           // {
           //     await LoadData();
           //     await Search();
           //     StateHasChanged();
           // });
           // await _hubConnection.StartAsync();
            await SignalRReload();
            await LoadData();
            await LoadStatusOptions();
            await LoadTechnician();
            await GetPage();
            await CheckPermission();
            _selectedChange += Search;
        }
        private async Task SignalRReload()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
            .Build();
            _hubConnection.On("LoadRequestSuport", async () =>
            {
                await LoadData();
                await Search();
                StateHasChanged();
            });
            await _hubConnection.StartAsync();
        }
        private async Task GetPage()
        {
            var url = "request-it-suport";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task LoadData()
        {
            _requestSuport = new();
            if (Http != null)
            {
                var data = await Http.GetFromJsonAsync<List<RequestSuportDto>>("api/RequestITSuport/get-all");
                if (data != null) _requestSuport = data.ToList();
            }

            _requestSuportFillter = _requestSuport.ToList();
            StateHasChanged();
        }
        private async Task CheckPermission()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }

            if (CurrenUser != null)
            {
                Created = await _hasPermission.CheckPermissions(PageId, nameof(PermissionActionType.Add),
                    CurrenUser);

                Edit = await _hasPermission.CheckPermissions(PageId, nameof(PermissionActionType.Edit), CurrenUser);

                Delete = await _hasPermission.CheckPermissions(PageId, nameof(PermissionActionType.Delete),
                    CurrenUser);
            }
        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            await _requestItSuport.ShowModal(false);
        }
        private async Task UpdateAsync(RequestSuportDto requestSuportDto)
        {
            await _requestItSuport.ShowModal(true, requestSuportDto);
        }
        private async Task DeleteAsync(Guid id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            await DeleteFileServer(id);
            var response = await Http?.DeleteAsync($"api/RequestITSuport/delete/{id}")!;

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Delete successfully !");
            }
            else
            {
                await Message.Error("Delete failed !");
            }
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        private async Task DeleteFileServer(Guid requestSuportId)
        {
            _attachment = new();
            if (Http != null)
            {
                var data = await Http.GetFromJsonAsync<List<AttachmentDto>>($"api/RequestITSuport/get-attechment/{requestSuportId}");
                if (data != null) _attachment = data.ToList();
            }

            foreach (var item in _attachment)
            {
                var url = $"{item.FilePath}";
                await Http?.DeleteAsync($"api/Common/delete-file-suport?avatarUrl={url}")!;
            }
        }
        private async Task ViewAsync(RequestSuportDto requestSuportDto)
        {
            await _requestItSuportDetailModal.ShowModal(requestSuportDto);
        }
        #endregion
        #region Filter
        private Func<Task>? _selectedChange;
        private string? RequesterName { get; set; }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        TableFilter<string>[] _technicianNameFilter = Array.Empty<TableFilter<string>>();
        private void RequesterNameInputChanged(ChangeEventArgs e)
        {
            RequesterName = e.Value?.ToString();
        }
        TableFilter<RequestSuportStatusType>[] _statusFilter = Array.Empty<TableFilter<RequestSuportStatusType>>();
        private async Task LoadTechnician()
        {
            _listTechnician = new List<UserContributerDto>();
            if (Http != null)
            {
                var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
                if (data != null) _listTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
            }

            _technicianNameFilter = _listTechnician
               .Select(x => new TableFilter<string>
               {
                   Text = x.Name,
                   Value = x.Name,
                   Selected = false
               })
               .ToArray();
        }

        private Task LoadStatusOptions()
        {
            _statusFilter = Enum.GetValues(typeof(RequestSuportStatusType))
               .Cast<RequestSuportStatusType>()
               .Select(e => new TableFilter<RequestSuportStatusType>
               {
                   Text =  EnumHelper.GetDisplayName(e), 
                   Value = e,                
                   Selected = false
               })
               .ToArray();
            return Task.CompletedTask;
        }
        private Task Search()
        {
            var name = StringHelper.NormalizeText(RequesterName?.Trim());

            var filtered = _requestSuport
                .Where(x =>
                    (string.IsNullOrWhiteSpace(name) ||
                        (!string.IsNullOrWhiteSpace(x.RequestByName) &&
                         StringHelper.NormalizeText(x.RequestByName).Contains(name)))
                )
                .ToList();

            if (string.IsNullOrWhiteSpace(name))
            {
                filtered = _requestSuport;
            }
            _requestSuportFillter = filtered;
            return Task.CompletedTask;
        }
        private Table<RequestSuportDto>? _tableRef;
        private async Task Reset()
        {
            RequesterName = null;
            _requestSuportFillter = _requestSuport;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);

        }
        #endregion
    }
}
