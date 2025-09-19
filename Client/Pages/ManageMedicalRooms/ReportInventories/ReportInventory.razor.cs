using AntDesign;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.ReportInventories
{
    public partial class ReportInventory
    {
        [Inject] private HttpClient Http { get; set; }
        private LoadReportInventoryDto loadReportInventory = new LoadReportInventoryDto();

        protected override async Task OnInitializedAsync()
        {
            LoadMonthAndYearSelected();
            await LoadData();
            SelectedChange += LoadData;

        }
        private async Task LoadData()
        {
            //var data = await Http.GetFromJsonAsync<LoadReportInventoryDto>(
            //    $"api/Inventory/get-report");
            var data = await Http.GetFromJsonAsync<LoadReportInventoryDto>(
                    $"api/Inventory/get-report/{MonthValue}/{YearValue}");
            loadReportInventory = data;

            await InvokeAsync(StateHasChanged);
        }
        #region Filter
        private Func<Task> SelectedChange;
        private int _monthvalue { get; set; } = 0;
        private int MonthValue
        {
            get => _monthvalue;
            set
            {
                if (value != _monthvalue)
                {
                    _monthvalue = value;
                    SelectedChange?.Invoke();
                }
            }
        }
        private int _yearvalue { get; set; } = 0;
        private int YearValue
        {
            get => _yearvalue;
            set
            {
                if (value != _yearvalue)
                {
                    _yearvalue = value;
                    SelectedChange?.Invoke();
                }
            }
        }

        private List<int> Years = new();
        private void LoadMonthAndYearSelected()
        {
            var currentYear = DateTime.Now.Year;
            Years = new List<int>
                {
                    currentYear - 2,
                    currentYear - 1,
                    currentYear,
                    currentYear + 1
                };
            YearValue = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            MonthValue = currentMonth == 1 ? 12 : currentMonth - 1;
        }

        #endregion
    }
}
