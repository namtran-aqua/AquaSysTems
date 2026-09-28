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
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetCurrentUser();
            await GetIMG();

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

        private Dictionary<string, IEnumerable<GoogleDriveImageDto>> SelectedMap
            = new();
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
                    var url = $"/api/Img/thumbnail/{img.FileId}";
                    await JSRuntime.InvokeVoidAsync("downloadFile", url, img.FileName);
                    // Add a tiny delay to not overwhelm the browser's download queue
                    await Task.Delay(100);
                }
            }
            else
            {
                await Message.Warning("Vui lòng chọn ít nhất 1 hình ảnh để tải xuống!");
            }
        }

        private async Task DeleteInternal(GoogleDriveImageDto row)
        {
            if (Http == null || row == null) return;

            try
            {
                var fileId = Uri.EscapeDataString(row.FileId);

                var res = await Http.DeleteAsync($"api/Img/delete?fileId={fileId}");

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
