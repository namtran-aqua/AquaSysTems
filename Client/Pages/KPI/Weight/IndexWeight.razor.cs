using AntDesign;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.QuaterCalculated;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.Weight
{
    public partial class IndexWeight
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<PositionType> ListPositionType = new List<PositionType>();
        private List<PeriodType> ListPeriodType = new List<PeriodType>();
        private List<KPIIndexType> ListKPIIndexType = new List<KPIIndexType>();
        private List<IndexWeightDto> DataSource = new();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await GetEnum();
        }
        private async Task LoadData()
        {
            //var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>("api/QuarterCalculated/get-list");

            //if (result is not null)
            //{
            //    DataSource = result;
            //}
            //else
            //{
            //    DataSource = new();
            //}

            StateHasChanged();
        }
        #endregion
        #region Load Data
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
        private async Task CreatedAsync(PositionType type)
        {


        }

        #endregion

    }
}
