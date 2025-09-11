
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ITSuport.RequestITSuport;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
            await GetPage();
            await CheckPermission();
        }
        private async Task GetPage()
        {

            var url = "request-it-suport";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");

        }
        private async Task LoadData()
        {
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
    }
}
