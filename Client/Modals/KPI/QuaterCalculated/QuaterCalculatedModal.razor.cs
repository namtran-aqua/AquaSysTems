using AntDesign;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.QuaterCalculated;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.QuaterCalculated
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
        private List<QuarterCalculateType> ListKPIQuarterCalculateType = new List<QuarterCalculateType>();

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
            ListKPIQuarterCalculateType = Enum.GetValues(typeof(QuarterCalculateType))
                    .Cast<QuarterCalculateType>()
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

            var response = await Http.PostAsJsonAsync("api/QuarterCalculated/create", QuaterCalculatedDto);

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
            var response = await Http.PutAsJsonAsync("api/QuarterCalculated/update", QuaterCalculatedDto);

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
