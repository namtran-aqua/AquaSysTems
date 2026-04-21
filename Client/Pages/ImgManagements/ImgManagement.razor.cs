using AquaSolution.Client.Common;
using AquaSolution.Shared.Imgs;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ImgManagements
{
    public partial class ImgManagement
    {

        #region Declaration
        private UserDto? CurrenUser { get; set; }
        private List<GroupImg> images = new();
        private List<UserContributerDto> Contributors = new();
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
            if (Contributors == null || !Contributors.Any())
                return;

            var result = new List<GroupImg>();

            foreach (var contributor in Contributors)
            {
                var workId = contributor.WorkDayId.ToString();

                var data = await Http.GetFromJsonAsync<List<CloudinaryImageDto>>(
                    $"api/Img/get-img/{workId}");

                if (data != null && data.Any())
                {
                    result.Add(new GroupImg
                    {
                        WorkId = workId,
                        CloudinaryImageDtos = data
                    });
                }
            }

            images = result;
        }
        #endregion
        private void Download(CloudinaryImageDto row)
        {
            var url =
                "/api/img/download" +
                "?url=" + Uri.EscapeDataString(row.SecureUrl);

            Navigation.NavigateTo(url, forceLoad: true);
        }

    }
}
