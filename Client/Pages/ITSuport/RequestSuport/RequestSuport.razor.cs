
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ITSuport.RequestITSuport;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

        private List<UserContributerDto> ListTechnician = new List<UserContributerDto>();
        private HasPermission hasPermission = new();
        private RequestITSuportDetailModal requestITSuportDetailModal =new();
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
            await InvokeAsync(StateHasChanged);
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
            foreach(var item in Attachment)
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
        #region Search
        private Func<Task> SelectedChange;
        private string? RequesterName { get; set; }
        private Guid _technicalSuport { get; set; } = Guid.Empty;
        private Guid TechnicalSuport
        {
            get => _technicalSuport;
            set
            {
                if (value != _technicalSuport)
                {
                    _technicalSuport = value;
                    SelectedChange.Invoke();
                }
            }
        }
        private int _status { get; set; } = -99;
        private int Status
        {
            get => _status;
            set
            {
                if (value != _status)
                {
                    _status = value;
                    SelectedChange.Invoke();
                }
            }
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

        private List<UserContributerDto> ListTechnicianFilter = new();

        private List<KeyValuePair<RequestSuportStatusType, string>> StatusOptions = new();
        private List<KeyValuePair<RequestSuportStatusType, string>> StatusOptionsFilter = new();

        private async Task LoadTechnician()
        {
            ListTechnician = new List<UserContributerDto>();
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            ListTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
            ListTechnician.Add(new UserContributerDto
            {
                Id = Guid.Empty,
                Name = "--ALL--"
            });
            ListTechnicianFilter = ListTechnician;
        }

        private async Task LoadStatusOptions()
        {
            StatusOptions = Enum.GetValues(typeof(RequestSuportStatusType))
                    .Cast<RequestSuportStatusType>()
                    .Select(e => new KeyValuePair<RequestSuportStatusType, string>(e, GetDisplayName(e)))
                    .ToList();
            StatusOptionsFilter = StatusOptions;
            StatusOptionsFilter.Add(new KeyValuePair<RequestSuportStatusType, string>((RequestSuportStatusType)(-99), "--ALL--"));

        }
        private string GetDisplayName(RequestSuportStatusType status)
        {
            var displayAttribute = status.GetType()
                .GetField(status.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
            return displayAttribute?.Name ?? status.ToString();
        }
        private async Task Search()
        {
            try
            {
                var name = RequesterName?.Trim().ToLower();

                var filtered = _requestSuport
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(name) ||
                            (!string.IsNullOrWhiteSpace(x.RequestByName) && x.RequestByName.ToLower().Contains(name))) &&
                        (TechnicalSuport == Guid.Empty || x.TechnicianId == TechnicalSuport) &&
                     (Status == -99 || x.Status == (RequestSuportStatusType)Status)
                    )
                    .ToList();
                if (string.IsNullOrWhiteSpace(name) &&
                    TechnicalSuport == Guid.Empty && Status == -99)
                {
                    filtered = _requestSuport;
                }

                _requestSuportFillter = filtered;
                // StateHasChanged();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }
        }
        private Task Reset()
        {
            RequesterName = null;
            TechnicalSuport = Guid.Empty;
            Status = -99;
            _requestSuportFillter = _requestSuport;
            StateHasChanged();
            return Task.CompletedTask;
        }
        #endregion
    }
}
