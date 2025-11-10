using AntDesign;
using AntDesign.TableModels;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ExcelHelper;
using AquaSolution.Client.Components.KPI.KPISubmit;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AquaSolution.Client.Pages.KPI.ResultKPI
{
    public partial class ResultKPI
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<ViewResultKpiDto> DataSource { get; set; }
        private List<ViewResultKpiDto> DataFilter { get; set; }
        private ApprovalTaskModal ApprovalTaskModal = new ApprovalTaskModal();
        private UserDto? CurrenUser { get; set; }
        #endregion
        #region Init
        protected override async Task OnInitializedAsync()
        {
            await LoadCurrenUser();
            await LoadData();
        }
        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<ViewResultKpiDto>>("api/KPISubmit/get-result-kpi");
            var filtered = new List<ViewResultKpiDto>();
            if (result is not null)
            {
                if (CurrenUser.Roles.Any(x => x.Name == "Admin") || CurrenUser.Roles.Any(x => x.Name == "HR"))
                {
                    filtered = result;
                }
                else
                {
                    if (CurrenUser.DepartmentId != null && CurrenUser.FactoryId != null)
                    {
                        filtered = result
                            .Where(x => x.FactoryId == CurrenUser.FactoryId
                                     && x.DepartmentId == CurrenUser.DepartmentId)
                            .ToList();
                    }
                    else if (CurrenUser.FactoryId != null)
                    {
                        filtered = result
                            .Where(x => x.FactoryId == CurrenUser.FactoryId)
                            .ToList();
                    }
                }
                DataSource = filtered.OrderBy(x => x.Year).ThenBy(x => GetSortPriority(x)).ToList();
            }
            else
            {
                DataSource = new();
            }
            DataFilter = DataSource;
            //add Filter
            // Filter tháng
            _monthFilter = DataFilter
                .Where(x => x.Month.HasValue && x.Month.Value > 0)
                .Select(x => new TableFilter<int?>
                {
                    Text = $"Month {x.Month}",
                    Value = x.Month,
                    Selected = false
                })
                .GroupBy(f => f.Value)
                .Select(g => g.First())
                .ToArray();

            // Filter quý
            _quaterFilter = DataFilter
                .Where(x => x.Quarter.HasValue && x.Quarter.Value > 0)
                .Select(x => new TableFilter<int?>
                {
                    Text = $"Quarter {x.Quarter}",
                    Value = x.Quarter,
                    Selected = false
                })
                .GroupBy(f => f.Value)
                .Select(g => g.First())
                .ToArray();

            // Filter nửa năm
            _halfYearFilter = DataFilter
                .Where(x => x.HalfYear.HasValue && x.HalfYear.Value > 0)
                .Select(x => new TableFilter<int?>
                {
                    Text = $"HalfYear {x.HalfYear}",
                    Value = x.HalfYear,
                    Selected = false
                })
                .GroupBy(f => f.Value)
                .Select(g => g.First())
                .ToArray();

            // Filter năm
            _yearFilter = DataFilter
                .Where(x => x.Year.HasValue && x.Year.Value > 0)
                .Select(x => new TableFilter<int?>
                {
                    Text = $"Year {x.Year}",
                    Value = x.Year,
                    Selected = false
                })
                .GroupBy(f => f.Value)
                .Select(g => g.First())
                .ToArray();

            //---------
            StateHasChanged();
        }
        TableFilter<int?>[] _monthFilter = Array.Empty<TableFilter<int?>>();
        TableFilter<int?>[] _quaterFilter = Array.Empty<TableFilter<int?>>();
        TableFilter<int?>[] _halfYearFilter = Array.Empty<TableFilter<int?>>();
        TableFilter<int?>[] _yearFilter = Array.Empty<TableFilter<int?>>();
        private Table<ViewResultKpiDto>? _tableRef;

        private int GetSortPriority(ViewResultKpiDto x)
        {
            var month = x.Month ?? 0;

            if (month >= 1 && month <= 3) return month;
            if (x.Quarter == 1) return 10;
            if (month >= 4 && month <= 6) return month + 10;
            if (x.Quarter == 2) return 20;
            if (x.HalfYear == 1) return 30;
            if (month >= 7 && month <= 9) return month + 30;
            if (x.Quarter == 3) return 40;
            if (month >= 10 && month <= 12) return month + 40;
            if (x.Quarter == 4) return 60;
            if (x.HalfYear == 2) return 70;

            return 999;
        }
        #endregion
        #region Action
        private async Task ViewAsync(ViewResultKpiDto viewResultKpiDto)
        {
            var approvalInfo = new ApprovalInfo();
            approvalInfo.SubmitId = viewResultKpiDto.SubmitId;
            await ApprovalTaskModal.ShowModalAsync(approvalInfo, true);
        }
        private async Task ExportExcel()
        {
            try
            {
                var selectedMonths = _monthFilter.Where(f => f.Selected).Select(f => f.Value).ToList();
                var selectedQuarters = _quaterFilter.Where(f => f.Selected).Select(f => f.Value).ToList();
                var selectedHalfYears = _halfYearFilter.Where(f => f.Selected).Select(f => f.Value).ToList();
                var selectedYears = _yearFilter.Where(f => f.Selected).Select(f => f.Value).ToList();
                var filteredData = DataFilter.Where(row =>
                    (selectedMonths.Count == 0 || (row.Month.HasValue && selectedMonths.Contains(row.Month.Value))) &&
                    (selectedQuarters.Count == 0 || (row.Quarter.HasValue && selectedQuarters.Contains(row.Quarter.Value))) &&
                    (selectedHalfYears.Count == 0 || (row.HalfYear.HasValue && selectedHalfYears.Contains(row.HalfYear.Value))) &&
                    (selectedYears.Count == 0 || (row.Year.HasValue && selectedYears.Contains(row.Year.Value)))
                ).ToList();

                var data = filteredData;
                string[] excludeProperties = { "SubmitId", "DepartmentId", "FactoryId", "kPITotalScoreType" };
                var excelBytes = ExcelFileGenerator.GenerateExcelFile(data, null, excludeProperties);
                await JS.InvokeVoidAsync("saveAsFile", $"KPIResult_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                    Convert.ToBase64String(excelBytes));

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
        #endregion
        #region Search
        private async Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            //DataFilter = DataSource;
            await LoadData();
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }

        private string? WorkDayId { get; set; }
        private string? FullName { get; set; }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void WorkDayIdInputChanged(ChangeEventArgs e)
        {
            WorkDayId = e.Value?.ToString();
        }
        private void FullNameInputChanged(ChangeEventArgs e)
        {
            FullName = e.Value?.ToString();
        }
        private Task Search()
        {
            try
            {
                var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                var fullName = StringHelper.NormalizeText(FullName?.Trim());

                if (DataSource != null)
                {
                    var filtered = DataSource
                        .Where(x =>
                            (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                            (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.UserName) && StringHelper.NormalizeText(x.UserName).Contains(fullName)))
                        )
                        .ToList();

                    if (string.IsNullOrWhiteSpace(workDayId) &&
                        string.IsNullOrWhiteSpace(fullName))
                    {
                        filtered = DataSource;
                    }

                    DataFilter = filtered;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
