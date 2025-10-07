using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Administration.UserManagements;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.QuaterCalculated;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.QuaterCalculated
{
   public partial class QuaterCalculatedModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private QuaterCalculatedDto QuaterCalculatedDto = new QuaterCalculatedDto();
        private Form<QuaterCalculatedDto> formRef;
        private bool IsEdit { get; set; }
        private List<KPIQuarterCalculateType> ListKPIQuarterCalculateType = new List<KPIQuarterCalculateType>();

        #endregion
        #region Init
        public async Task ShowModal( bool isEdit = false, QuaterCalculatedDto? quaterCalculatedDto = null)
        {
            IsEdit = isEdit;

            if (isEdit)
            {
                QuaterCalculatedDto = quaterCalculatedDto ?? new QuaterCalculatedDto(); 
            }
            else
            {
                QuaterCalculatedDto = new QuaterCalculatedDto();
               
            }

            IsModalVisible = true;

            GetEnum();
            StateHasChanged();
        }

      
        private void GetEnum()
        {
            ListKPIQuarterCalculateType = Enum.GetValues(typeof(KPIQuarterCalculateType))
                    .Cast<KPIQuarterCalculateType>()
                    .ToList();
   
        }
        #endregion
        #region Actions
        private async Task SaveAsync()
        {
            bool isSave = false;
            if (IsEdit)
            {
                isSave = await UpdateAsync();
            }
            else
            {
                isSave = await CreatedAsync();
            }
            if (isSave)
            {
                await OnSave.InvokeAsync();
                IsModalVisible = false;
            }
        }
        private async Task<bool> CreatedAsync()
        {

            var response = await Http.PostAsJsonAsync("api/quaterCalculated/create", QuaterCalculatedDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                return true;
            }
            else
            {
                await Message.Error("Created failed.");
                return false;
            }
        }
        private async Task<bool> UpdateAsync()
        {
            var response = await Http.PutAsJsonAsync("api/quaterCalculated/update", QuaterCalculatedDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Updated successfully.");
                return true;
            }
            else
            {
                await Message.Error("Updated failed.");
                return false;
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
