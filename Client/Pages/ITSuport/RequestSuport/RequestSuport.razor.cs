
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
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ITSuport.RequestSuport
{
    public partial class RequestSuport
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }

        private List<RequestSuportDto> _requestSuport = new();
        private List<RequestSuportDto> _requestSuportFillter = new();
        private HubConnection? _hubConnection;
        private List<UserContributerDto> ListTechnician = new List<UserContributerDto>();
        private HasPermission hasPermission = new();
        private RequestITSuportDetailModal requestITSuportDetailModal = new();
        private UserDto CurrenUser { get; set; }
        private RequestITSuport requestITSuport = new();
        private List<AttachmentDto> Attachment = new();
        private bool Created { get; set; }
        private bool Edit { get; set; }
        private bool Delete { get; set; }
        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
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
            await LoadData();
            await LoadStatusOptions();
            await LoadTechnician();
            await GetPage();
            await CheckPermission();
            SelectedChange += Search;
        }
        private async Task GetPage()
        {

            var url = "request-it-suport";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");

        }
        private async Task LoadData()
        {
            _requestSuport = new();
            var data = await Http.GetFromJsonAsync<List<RequestSuportDto>>("api/RequestITSuport/get-all");
            _requestSuport = data.ToList();
            _requestSuportFillter = _requestSuport.ToList();
            StateHasChanged();
        }
        private async Task CheckPermission()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            Created = await hasPermission.CheckPermissions(PageId, PermissionActionType.Add.ToString(), CurrenUser);

            Edit = await hasPermission.CheckPermissions(PageId, PermissionActionType.Edit.ToString(), CurrenUser);

            Delete = await hasPermission.CheckPermissions(PageId, PermissionActionType.Delete.ToString(), CurrenUser);

        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            await requestITSuport.ShowModal(false);
        }
        private async Task UpdateAsync(RequestSuportDto requestSuportDto)
        {
            await requestITSuport.ShowModal(true, requestSuportDto);
        }
        private async Task DeleteAsync(Guid id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            await DeleteFileServer(id);
            var response = await Http.DeleteAsync($"api/RequestITSuport/delete/{id}");

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
            Attachment = new();
            var data = await Http.GetFromJsonAsync<List<AttachmentDto>>($"api/RequestITSuport/get-attechment/{requestSuportId}");
            Attachment = data.ToList();
            foreach (var item in Attachment)
            {
                var url = $"{item.FilePath}";
                var response = await Http.DeleteAsync($"api/Common/delete-file-suport?avatarUrl={url}");
            }
        }
        private async Task ViewAsync(RequestSuportDto requestSuportDto)
        {
            await requestITSuportDetailModal.ShowModal(requestSuportDto);
        }
        #endregion
        #region Filter
        private Func<Task> SelectedChange;
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
            ListTechnician = new List<UserContributerDto>();
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            ListTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
            _technicianNameFilter = ListTechnician
               .Select(x => new TableFilter<string>
               {
                   Text = x.Name,
                   Value = x.Name,
                   Selected = false
               })
               .ToArray();
        }

        private async Task LoadStatusOptions()
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

        }
        private async Task Search()
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
        }
        private Table<RequestSuportDto> tableRef;
        private async Task Reset()
        {
            RequesterName = null;
            _requestSuportFillter = _requestSuport;
            tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);

        }
        #endregion
    }
}
