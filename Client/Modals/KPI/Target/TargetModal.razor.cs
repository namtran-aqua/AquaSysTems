using AntDesign;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.KPITarget;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.Target
{
    public partial class TargetModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        private List<TargetDto> DataSource = new();
        private UserDto User =new();
        #endregion
        #region Init
        public async Task ShowModal(UserDto userDto)
        {
            User = userDto;
            await GetData();
            IsModalVisible = true;
            StateHasChanged();
        }

        #endregion
        #region Actions
        private async Task GetData()
        {
            var response = await Http.GetFromJsonAsync<List<TargetDto>>($"api/KPIMonthlyTarget/target-by-user/{User.Id}");
            if (response != null)
            {
                DataSource = response;
                StateHasChanged();
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
