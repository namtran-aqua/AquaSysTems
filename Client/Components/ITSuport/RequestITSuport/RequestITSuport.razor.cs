
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace AquaSolution.Client.Components.ITSuport.RequestITSuport
{
    public partial class RequestITSuport
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsModalVisible = false;
        private RequestSuportDto RequestSuport = new();
        private HandleRequestSuportDto HandleRequestSuport = new();

        private List<UserContributerDto> ListTechnician = new List<UserContributerDto>();
        private List<UserContributerDto> ListUser = new List<UserContributerDto>();
        private List<RequestSuportCategoryDto> RequestSuportCategories = new();
        private UserDto CurrenUser { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<RequestSuportDto> formRef = new();
        private bool IsEdit { get; set; }
        private List<AttachmentDto> Attachment = new();
        #endregion
        #region Innit
        public async Task ShowModal(bool isEdit, RequestSuportDto requestSuportDto = null)
        {
            RequestSuport = new();
            Attachment = new();
            await LoadUser();
            await LoadCategory();
            IsEdit = isEdit;
            if (requestSuportDto != null && IsEdit)
            {
                RequestSuport = requestSuportDto;
                await LoadAttachment(RequestSuport.Id);
            }
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadUser()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            ListTechnician = new List<UserContributerDto>();
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            ListUser = data.ToList();

            if (ListUser != null)
            {
                RequestSuport.RequestById = ListUser.FirstOrDefault(x => x.Id == CurrenUser.Id)?.Id ?? Guid.Empty;
            }

            ListTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
        }
        private async Task LoadAttachment(Guid requestSuportId)
        {
            Attachment = new();
            var data = await Http.GetFromJsonAsync<List<AttachmentDto>>($"api/RequestITSuport/get-attechment/{requestSuportId}");
            Attachment = data.ToList();
        }
        private async Task LoadCategory()
        {
            var response = await Http.GetFromJsonAsync<List<RequestSuportCategoryDto>>("api/RequestSuportCategory/get-all");
            if (response != null)
            {
                RequestSuportCategories = response;
            }
            else
            {
                RequestSuportCategories = new List<RequestSuportCategoryDto>();
            }
        }

        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var data = await MappingData();


            if (IsEdit)
            {
                await UpdateAsync(data);
            }
            else
            {
                var valid = formRef.Validate();
                if (!valid)
                {
                    return;
                }
                await CreatedAsync(data);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        private Task<HandleRequestSuportDto> MappingData()
        {

            RequestSuport.TechnicianId = RequestSuportCategories.FirstOrDefault(
                x => x.Id == RequestSuport.RequestSuportCategoryId)?.TechnicianId;
            var data = new HandleRequestSuportDto
            {
                Id = RequestSuport.Id,
                RequestTitle = RequestSuport.RequestTitle,
                RequestSuportCategoryId = RequestSuport.RequestSuportCategoryId,
                RequestBy = RequestSuport.RequestById,
                CreatedDate = RequestSuport.CreatedDate,
                RequestDescription = RequestSuport.RequestDescription,
                RequestSolution = RequestSuport.RequestSolution,
                TechnicianId = RequestSuport.TechnicianId,
                DueDate = RequestSuport.DueDate,
                Attachments = Attachment
            };

            if (!IsEdit)
            {
                data.Status = RequestSuportStatusType.Open;
                data.CreatedDate = DateTime.Now;
            }

            return Task.FromResult(data);
        }

        private void Close()
        {
            IsModalVisible = false;
        }
        private async Task ChangeStatus(RequestSuportStatusType status)
        {
            var data = await MappingData();
            switch (status)
            {
                case RequestSuportStatusType.InProgress:
                    data.Status = RequestSuportStatusType.InProgress;
                    data.InProgessDate = DateTime.Now;
                    break;
                case RequestSuportStatusType.Cancel:
                    data.Status = RequestSuportStatusType.Cancel;
                    data.CancelDate = DateTime.Now;
                    break;
                case RequestSuportStatusType.OnHold:
                    data.Status = RequestSuportStatusType.OnHold;
                    data.OnHoldDate = DateTime.Now;
                    break;
                case RequestSuportStatusType.Resolved:
                    data.Status = RequestSuportStatusType.Resolved;
                    data.ResolveDate = DateTime.Now;
                    break;
            }
            if (data.Status == RequestSuportStatusType.Resolved)
            {
                var valid = formRef.Validate();
                if (!valid)
                {
                    return;
                }
            }
            await UpdateAsync(data);
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        #endregion
        #region HandleData
        private async Task UpdateAsync(HandleRequestSuportDto handleRequestSuportDto)
        {
            var response = await Http.PutAsJsonAsync("api/RequestITSuport/update", handleRequestSuportDto);

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Update successfully !");

            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Update failed: {error}");

            }
        }
        private async Task CreatedAsync(HandleRequestSuportDto handleRequestSuportDto)
        {
            var response = await Http.PostAsJsonAsync("api/RequestITSuport/created", handleRequestSuportDto);

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Created successfully !");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Create failed: {error}");
            }
        }
        #endregion
        #region Handle Img
        private async Task Deleted(AttachmentDto file)
        {
            Attachment.Remove(file);
            var url = $"{file.FilePath}";
            var response = await Http.DeleteAsync($"api/Common/delete-file-suport?avatarUrl={url}");
            await InvokeAsync(StateHasChanged);

        }
        private async Task OnSingleCompleted(UploadInfo fileinfo)
        {
            if (fileinfo.File.State == UploadState.Success)
            {
                var url = fileinfo.File.Response?.ToString();
                if (!string.IsNullOrEmpty(url))
                {
                    var uri = new Uri(url);
                    var relativePath = uri.AbsolutePath.TrimStart('/');
                    Attachment.Add(new AttachmentDto
                    {
                        FilePath = relativePath,
                        FileName = fileinfo.File.FileName,
                        FileExtend = fileinfo.File.Ext,
                        FileSize = fileinfo.File.Size,
                        CreatedTime = DateTime.Now
                    });

                }

            }
        }
        private string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{Math.Round(bytes / (1024.0 * 1024.0))} MB";
            return $"{Math.Round(bytes / 1024.0)} KB";
        }

        private bool BeforeUpload1(UploadFileItem file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!(ext == ".jpg" || ext == ".jpeg" || ext == ".png" ||
                  ext == ".docx" || ext == ".xlsx" || ext == ".pdf" ||
                  ext == ".txt" || ext == ".slx"))
            {
                Message.Error("You can only upload JPG, PNG, DOCX, XLSX, PDF, TXT or SLX files!");
                return false;
            }
            var isLt2M = file.Size / 1024 / 1024 < 5;
            if (!isLt2M)
            {
                Message.Error("File must be smaller than 3MB!");
                return false;
            }

            return true;
        }
        #endregion
    }
}
