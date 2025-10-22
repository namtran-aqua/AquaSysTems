using AquaSolution.Client.Common;
using AquaSolution.Client.Components.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;

namespace AquaSolution.Client.Pages.KPI.KPISubmit
{
    public partial class KPIUserSubmit
    {
        #region Declaration
        private UserDto? CurrenUser { get; set; }
        [Inject] private HttpClient? Http { get; set; }
        private SelectedKPISubmitModalrazor SelectedKPISubmitModalrazor = new();
        #endregion
        #region Init    
        protected override async Task OnInitializedAsync()
        {
           // CurrenUser = await UserService.GetCurrentUserAsync();
            await LoadCurrenUser();
        }
        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }
        #endregion
        #region Action
        private async Task SelectedKPI()
        {
          await  SelectedKPISubmitModalrazor.ShowModal(CurrenUser!);
        }
        #endregion
    }
}
