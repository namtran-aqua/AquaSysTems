using AquaSolution.Client.Common;
using AquaSolution.Shared.Imgs;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace AquaSolution.Client.Pages.ImgManagements
{
    public partial class ImgManagement
    {
        #region Declaration
        private UserDto? CurrenUser { get; set; }
        private List<GroupImg> images = new();
        private List<UserContributerDto> Contributors = new();
        private bool Loading { get; set; }
        [Inject] private HttpClient? Http { get; set; }
        #endregion

        #region Image Preview State
        private bool isPreviewOpen = false;
        private GoogleDriveImageDto? previewImage = null;
        private List<GoogleDriveImageDto> currentFolderImages = new();
        private int previewIndex = 0;
        private double zoomLevel = 1.0;
        private int rotationAngle = 0;
        private bool isEnhanced = false;

        private void OpenPreview(GoogleDriveImageDto img, List<GoogleDriveImageDto> folderImages)
        {
            previewImage = img;
            currentFolderImages = folderImages;
            previewIndex = folderImages.IndexOf(img);
            if (previewIndex < 0) previewIndex = 0;
            zoomLevel = 1.0;
            rotationAngle = 0;
            isEnhanced = false;
            isPreviewOpen = true;
        }

        private void ClosePreview()
        {
            isPreviewOpen = false;
            previewImage = null;
        }

        private void ZoomIn()
        {
            if (zoomLevel < 3.0)
                zoomLevel = Math.Round(zoomLevel + 0.25, 2);
        }

        private void ZoomOut()
        {
            if (zoomLevel > 0.5)
                zoomLevel = Math.Round(zoomLevel - 0.25, 2);
        }

        private void ResetZoom()
        {
            zoomLevel = 1.0;
            rotationAngle = 0;
        }

        private void Rotate()
        {
            rotationAngle = (rotationAngle + 90) % 360;
        }

        private void ToggleEnhance()
        {
            isEnhanced = !isEnhanced;
        }

        private void NextImage()
        {
            if (currentFolderImages == null || !currentFolderImages.Any()) return;
            previewIndex = (previewIndex + 1) % currentFolderImages.Count;
            previewImage = currentFolderImages[previewIndex];
            zoomLevel = 1.0;
            rotationAngle = 0;
        }

        private void PrevImage()
        {
            if (currentFolderImages == null || !currentFolderImages.Any()) return;
            previewIndex = (previewIndex - 1 + currentFolderImages.Count) % currentFolderImages.Count;
            previewImage = currentFolderImages[previewIndex];
            zoomLevel = 1.0;
            rotationAngle = 0;
        }
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetCurrentUser();
            await GetIMG();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("eval", @"
                    window.downloadImgFile = async (url, fileName) => {
                        try {
                            const res = await fetch(url);
                            if (!res.ok) {
                                alert('Không thể tải hình ảnh (Mã lỗi: ' + res.status + '). Vui lòng thử lại!');
                                return;
                            }
                            const blob = await res.blob();
                            const blobUrl = URL.createObjectURL(blob);
                            const link = document.createElement('a');
                            link.href = blobUrl;
                            link.download = fileName;
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);
                            setTimeout(() => URL.revokeObjectURL(blobUrl), 10000);
                        } catch (err) {
                            alert('Lỗi khi tải hình ảnh: ' + err.message);
                        }
                    };
                    window.downloadFile = window.downloadImgFile;
                ");
            }
        }

        public string GetApiUrl(string path)
        {
            var baseUri = Navigation.BaseUri.TrimEnd('/');
            if (Navigation.Uri.IndexOf("/ITSM", StringComparison.OrdinalIgnoreCase) >= 0 &&
                baseUri.IndexOf("/ITSM", StringComparison.OrdinalIgnoreCase) < 0)
            {
                baseUri = $"{baseUri}/ITSM";
            }
            return $"{baseUri}/{path.TrimStart('/')}";
        }

        private async Task GetCurrentUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
            if (CurrenUser.FactoryId == Guid.Empty && CurrenUser.DepartmentId == Guid.Empty)
            {
                return;
            }

            try
            {
                var data = await Http.GetFromJsonAsync<List<UserContributerDto>>(
                    $"api/User/get-contributer");

                if (data == null)
                {
                    Contributors = new List<UserContributerDto>();
                    return;
                }

                var query = data.AsQueryable();

                if (CurrenUser.Roles.Any(x => x.Name == "Admin"))
                {
                    query = data.AsQueryable();
                }
                else
                {
                    if (CurrenUser.FactoryId != Guid.Empty)
                    {
                        query = query.Where(x => x.FactoryId == CurrenUser.FactoryId);
                    }

                    if (CurrenUser.DepartmentId != Guid.Empty)
                    {
                        query = query.Where(x => x.DepartmentId == CurrenUser.DepartmentId);
                    }
                }

                Contributors = query.ToList();
            }
            catch (HttpRequestException ex)
            {
                Contributors = new List<UserContributerDto>();
                Console.WriteLine(ex.Message);
            }
        }

        private async Task GetIMG()
        {
            Loading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                var isAdmin = CurrenUser.Roles.Any(r => r.Name == "Admin");
                var allImages = await Http.GetFromJsonAsync<List<GoogleDriveImageDto>>(
                    $"api/Img/get-all-img?workDayId={CurrenUser.WorkDayId}&isAdmin={isAdmin}");

                if (allImages == null || !allImages.Any())
                {
                    images = new List<GroupImg>();
                    return;
                }

                images = allImages
                    .GroupBy(img => img.FolderName)
                    .Select(g => new GroupImg
                    {
                        FolderName = g.Key,
                        GoogleDriveImageDtos = g
                            .OrderByDescending(x => x.CreatedTime)
                            .ToList()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                images = new List<GroupImg>();
                Console.WriteLine($"Error GetIMG: {ex.Message}");
                await Message.Error($"Lỗi kết nối Google Drive: {ex.Message}");
            }
            finally
            {
                Loading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        void OnSelectAll(bool selectAll, string folderName)
        {
            if (!selectAll)
            {
                SelectedMap[folderName] = new List<GoogleDriveImageDto>();
            }
        }

        private Dictionary<string, IEnumerable<GoogleDriveImageDto>> SelectedMap = new();

        private async Task DeleteAllImg(string folderName)
        {
            var confirm = await JSRuntime.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;

            if (!SelectedMap.ContainsKey(folderName))
                return;

            var listDelete = SelectedMap[folderName].ToList();

            if (!listDelete.Any())
                return;

            try
            {
                await Task.WhenAll(listDelete.Select(DeleteInternal));
                SelectedMap[folderName] = new List<GoogleDriveImageDto>();
                await GetIMG();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task DownloadSelectedImg(string folderName)
        {
            if (SelectedMap.TryGetValue(folderName, out var selected) && selected.Any())
            {
                foreach (var img in selected)
                {
                    var url = GetApiUrl($"api/Img/download/{img.FileId}");
                    await JSRuntime.InvokeVoidAsync("downloadImgFile", url, img.FileName);
                    // Add a tiny delay to not overwhelm the browser's download queue
                    await Task.Delay(200);
                }
            }
            else
            {
                await Message.Warning("Vui lòng chọn ít nhất 1 hình ảnh để tải xuống!");
            }
        }

        private async Task DownloadSingleImg(GoogleDriveImageDto img)
        {
            if (img == null) return;
            var url = GetApiUrl($"api/Img/download/{img.FileId}");
            await JSRuntime.InvokeVoidAsync("downloadImgFile", url, img.FileName);
        }

        private async Task DeleteInternal(GoogleDriveImageDto row)
        {
            if (Http == null || row == null) return;

            try
            {
                var fileId = Uri.EscapeDataString(row.FileId);
                var deleteUrl = GetApiUrl($"api/Img/delete?fileId={fileId}");
                var res = await Http.DeleteAsync(deleteUrl);

                if (!res.IsSuccessStatusCode)
                {
                    var msg = await res.Content.ReadAsStringAsync();
                    Console.WriteLine(msg);
                    return;
                }

                await InvokeAsync(() =>
                {
                    var group = images.FirstOrDefault(x => x.FolderName == row.FolderName);
                    if (group != null)
                    {
                        group.GoogleDriveImageDtos.Remove(row);

                        if (!group.GoogleDriveImageDtos.Any())
                        {
                            images.Remove(group);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task Delete(GoogleDriveImageDto row)
        {
            var confirm = await JSRuntime.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;

            await DeleteInternal(row);

            if (SelectedMap.ContainsKey(row.FolderName))
            {
                SelectedMap[row.FolderName] = SelectedMap[row.FolderName]
                    .Where(x => x.FileId != row.FileId)
                    .ToList();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
