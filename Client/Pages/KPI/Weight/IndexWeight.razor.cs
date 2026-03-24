using AntDesign;
using AquaSolution.Client.Modals.KPI.IndexWeight;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.IndexWeight;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.Weight
{
    public partial class IndexWeight
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<PositionType> ListPositionType = new List<PositionType>();
        private List<IndexWeightDto> DataSource = new();
        private IndexWeightModal indexWeightModal = new();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await GetEnum();
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>("api/IndexWeight/index-weight");

            if (result is not null)
            {
                DataSource = result;
            }
            else
            {
                DataSource = new();
            }

            StateHasChanged();
        }
        #endregion
        #region Load Data
        private async Task GetEnum()
        {
            ListPositionType = Enum.GetValues(typeof(PositionType))
                    .Cast<PositionType>()
                    .ToList(); 
        }
        #endregion
        #region Actions
        private async Task CreatedAsync(PositionType type)
        {
            await indexWeightModal.ShowModal(
                 IsEdit: true,
                 indexWeightDto: null,
                 positionType: type
             );
        }
        #endregion

    }
}
