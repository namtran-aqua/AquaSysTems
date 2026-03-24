using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.IndexWeight;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.IndexWeight
{
    public partial class IndexWeightModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private IndexWeightDto IndexWeight = new IndexWeightDto();
        private bool isEdit ;
        private List<PeriodType> ListPeriodType = new List<PeriodType>();
        private List<KPIIndexType> ListKPIIndexType = new List<KPIIndexType>();
        private List<PositionType> ListPositionType = new List<PositionType>();
        #endregion
        #region Init
        public async Task ShowModal(bool IsEdit = false, 
            IndexWeightDto? indexWeightDto = null , PositionType positionType = PositionType.HOD)
        {
            IsEdit = isEdit;

            if (isEdit)
            {
                IndexWeight = indexWeightDto ?? new IndexWeightDto(); 
            }
            else
            {
                IndexWeight = new IndexWeightDto();
                IndexWeight.PositionType = positionType;
            }

            IsModalVisible = true;

            await  GetEnum();
            StateHasChanged();
        }

        private async Task GetEnum()
        {
            ListPositionType = Enum.GetValues(typeof(PositionType))
                    .Cast<PositionType>()
                    .ToList();
            ListPeriodType = Enum.GetValues(typeof(PeriodType))
                    .Cast<PeriodType>()
                    .ToList();
            ListKPIIndexType = Enum.GetValues(typeof(KPIIndexType))
                    .Cast<KPIIndexType>()
                    .ToList();
        }
        #endregion
        #region Actions
        private async Task SaveAsync()
        {
            if (!isEdit)
            {
                await CreatedAsync();
            }
        }
        private async Task<bool> CreatedAsync()
        {

            var response = await Http.PostAsJsonAsync("api/IndexWeight/create", IndexWeight);

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
            var response = await Http.PutAsJsonAsync("api/KPITask/update", IndexWeight);

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
