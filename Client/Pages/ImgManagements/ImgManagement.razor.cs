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
                // var allImages = await Http.GetFromJsonAsync<List<CloudinaryImageDto>>(
                //     "api/Img/get-all-img");

                // if (allImages == null || !allImages.Any())
                // {
                //     images = new List<GroupImg>();
                //     return;
                // }
                // if (CurrenUser.Roles.Any(r => r.Name == "Admin"))
                // {
                //     images = allImages
                //         .Where(x => !string.IsNullOrWhiteSpace(x.WorkId))
                //         .GroupBy(x => x.WorkId)
                //         .Select(g => new GroupImg
                //         {
                //             WorkId = g.Key,
                //             CloudinaryImageDtos = g
                //                 .OrderByDescending(x => x.UpLoadDate)
                //                 .ToList()
                //         })
                //         .ToList();

                //     return;
                // }

                // if (Contributors == null || !Contributors.Any())
                // {
                //     images = new List<GroupImg>();
                //     return;
                // }

                // var allowedWorkIds = Contributors
                //     .Select(c => c.WorkDayId.ToString())
                //     .ToHashSet();

                // images = allImages
                //     .Where(img =>
                //         !string.IsNullOrWhiteSpace(img.WorkId) &&
                //         allowedWorkIds.Contains(img.WorkId))
                //     .GroupBy(img => img.WorkId)
                //     .Select(g => new GroupImg
                //     {
                //         WorkId = g.Key,
                //         CloudinaryImageDtos = g
                //             .OrderByDescending(x => x.UpLoadDate)
                //             .ToList()
                //     })
                //     .ToList();
            }
            finally
            {
                Loading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        void OnSelectAll(bool selectAll, string workId)
        {
            if (!selectAll)
            {
                SelectedMap[workId] = new List<CloudinaryImageDto>();
            }
        }

        private Dictionary<string, IEnumerable<CloudinaryImageDto>> SelectedMap
            = new();
        private async Task DeleteAllImg(string workId)
        {
            var confirm = await JSRuntime.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;

            if (!SelectedMap.ContainsKey(workId))
                return;

            var listDelete = SelectedMap[workId].ToList();

            if (!listDelete.Any())
                return;

            try
            {
                await Task.WhenAll(listDelete.Select(DeleteInternal));
                SelectedMap[workId] = new List<CloudinaryImageDto>();
                await GetIMG();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task DeleteInternal(CloudinaryImageDto row)
        {
            if (Http == null || row == null) return;

            try
            {
                var publicId = Uri.EscapeDataString(row.PublicId);

                var res = await Http.DeleteAsync($"api/Img/delete?publicId={publicId}");

                if (!res.IsSuccessStatusCode)
                {
                    var msg = await res.Content.ReadAsStringAsync();
                    Console.WriteLine(msg);
                    return;
                }

                await InvokeAsync(() =>
                {
                    var group = images.FirstOrDefault(x => x.WorkId == row.WorkId);
                    if (group != null)
                    {
                        group.CloudinaryImageDtos.Remove(row);

                        if (!group.CloudinaryImageDtos.Any())
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
       private async Task Delete(CloudinaryImageDto row)
        {
            var confirm = await JSRuntime.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;

            await DeleteInternal(row);

            if (SelectedMap.ContainsKey(row.WorkId))
            {
                SelectedMap[row.WorkId] = SelectedMap[row.WorkId]
                    .Where(x => x.PublicId != row.PublicId)
                    .ToList();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
