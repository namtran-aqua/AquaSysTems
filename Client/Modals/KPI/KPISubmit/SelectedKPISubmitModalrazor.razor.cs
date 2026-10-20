using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.CeilingLevel;
using AquaSolution.Shared.KPI.DealineManagement;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using NPOI.SS.Formula.Functions;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.KPISubmit
{
    public partial class SelectedKPISubmitModalrazor
    {
        #region DECLARATION

        [Inject] private HttpClient Http { get; set; }

        private bool IsModalVisible { get; set; }
        private string activeTabKey { get; set; } = "1";
        private string TitleButton { get; set; } = "Next ⟶";
        private string HeaderLine { get; set; } = string.Empty;
        private string PlaceholderSelected { get; set; } = "select month";

        private bool IsInputActual { get; set; }
        private bool HandleButtonClicked { get; set; }

        private UserDto CurrenUser = new();
        private DealineManagementDto KPIPeriodSubmit { get; set; }

        private List<DealineManagementDto> DealineManagement = new();
        private List<IndexWeightDto> IndexWeight = new();

        private HandleKPISubmitDto HandleKPISubmitDto = new();
        private List<HandleActualDto> HandleActualDto = new();

        private KPITotalScoreDto CurrenTotalScore = new();
        private int QuarterValue { get; set; }
        private int MonthValue { get; set; }
        private CeilingLevelDto CeilingLevel = new();
        private YearPrecheckResultDto YearPrecheckResult = new();
        private bool IsYearIneligible { get; set; } = false;
        private string YearIneligibleMessage { get; set; } = string.Empty;
        #endregion

        #region INIT

        public async Task ShowModal(UserDto currenUser)
        {
            CurrenUser = currenUser;
            HandleKPISubmitDto = new HandleKPISubmitDto();
            HandleActualDto = new List<HandleActualDto>();
            DealineManagement = new List<DealineManagementDto>();
            IndexWeight = new List<IndexWeightDto>();
            HandleButtonClicked = false;
            IsModalVisible = true;
            IsInputActual = false;
            activeTabKey = "1";
            TitleButton = "Next ⟶";
            YearPrecheckResult = new();
            IsYearIneligible = false;
            YearIneligibleMessage = string.Empty;

            await GetDeadline();
            await GetIndexWeight();
            await GetCeilingLevel();
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetDeadline()
        {
            var result = await Http.GetFromJsonAsync<List<DealineManagementDto>>(
                $"api/DealineManagement/get-deadline/{CurrenUser.Id}");

            DealineManagement = result ?? new();
            PlaceholderSelected = DealineManagement.Any()
                ? "select month"
                : "There are no KPIs to choose from";
        }

        private async Task GetIndexWeight()
        {
            var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>(
                $"api/kpisubmit/get-indexweight/{CurrenUser.PositionType}/{PeriodType.Quarter}");

            IndexWeight = result ?? new();
        }
        private async Task GetCeilingLevel()
        {
            CeilingLevel = await Http.GetFromJsonAsync<CeilingLevelDto>(
                $"api/ceilinglevel/ceilingLevel-by-userId/{CurrenUser.Id}")
                ?? new CeilingLevelDto();
        }
        #endregion

        #region ACTIONS

        private void CloseModal()
        {
            IsModalVisible = false;
        }

        private async Task SelectedChange()
        {
            if (KPIPeriodSubmit == null) return;

            var data = await LoadSelectedKPiSubmit(
                KPIPeriodSubmit.Year,
                KPIPeriodSubmit.Month);
            HandleActualDto = data.OrderBy(x => x.Index).ToList();
            IsInputActual = HandleActualDto.Any();
            activeTabKey = "1";
            TitleButton = "Next ⟶";
        }

        private async Task<List<HandleActualDto>> LoadSelectedKPiSubmit(int year, int? month)
        {
            return await Http.GetFromJsonAsync<List<HandleActualDto>>(
                $"api/kpisubmit/get-kpi-submit/{CurrenUser.Id}/{year}/{month}")
                ?? new();
        }
        bool isCalculating;
        private async Task ActionTab()
        {
            if (activeTabKey == "1")
            {
                isCalculating = true;
                activeTabKey = "2";
                TitleButton = "⟵ Back";

                HandleKPISubmitDto.KPITotalScore.Clear();

                HandleKPISubmitDto.HandleActual = HandleActualDto
                    .Select(x => x)
                    .ToList();

                foreach (var item in HandleKPISubmitDto.HandleActual)
                {
                    await CalculatedScoreMonth(item);
                    item.HeaderTitle = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName}";

                }

                await AddCurrenTask(HandleKPISubmitDto);

                var month = HandleKPISubmitDto.HandleActual.FirstOrDefault()?.Month;
                bool quarterCreated = false;

                if (month == 3 || month == 6 || month == 9 || month == 12)
                {

                    quarterCreated = await LoadAndCalculateQuarterAsync(HandleKPISubmitDto, month);
                }

                if ((month == 6 || month == 12) && quarterCreated)
                {
                    await LoadAndCalculateHalfYearAsync(HandleKPISubmitDto, month);
                }

                if (month == 12)
                {
                    await LoadAndCalculateYearAsync(HandleKPISubmitDto, month.Value);
                }
                isCalculating = false;
            }
            else
            {
                activeTabKey = "1";
                TitleButton = "Next ⟶";
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task SaveAsync()
        {
            HandleButtonClicked = true;

            var confirm = await MessageBox.Confirm(Modal, "Are you sure you want to submit?");
            if (!confirm)
            {
                HandleButtonClicked = false;
                return;
            }

            var response = await Http.PostAsJsonAsync($"api/kpisubmit/create/{KPIPeriodSubmit.Month}", HandleKPISubmitDto);

            if (response.IsSuccessStatusCode)
                await Message.Success("Created successfully");
            else
                await Message.Error(await response.Content.ReadAsStringAsync());
            IsModalVisible = false;

        }
        #endregion

        #region MONTH CALCULATION
        private async Task CalculatedScoreMonth(HandleActualDto item)
        {
            if (!item.ActualValue.HasValue || !item.TargetValue.HasValue) return;

            decimal actual = item.ActualValue.Value;
            decimal target = item.TargetValue.Value;

            decimal achievement = item.KPIFormulaType switch
            {
                KPIFormulaType.KF1 => target == 0 ? 0 : actual / target,
                KPIFormulaType.KF2 => target == 0 ? 0 : 2 - (actual / target),
                KPIFormulaType.KF3 => target > 0 ? actual / target : 2 - (actual / target),
                KPIFormulaType.KF4 => actual > target ? 0 : 1,
                _ => 0
            };

            if (target == 0)
            {
                achievement = item.Max.HasValue ? item.Max.Value * 100 : 0;
            }
            if (item.Bottom.HasValue && achievement < item.Bottom) achievement = 0;
            if (item.Max.HasValue && achievement > item.Max) achievement = item.Max.Value;

            item.Achiement = Math.Round(achievement, 4);
            item.Score = item.Weight.HasValue
                ? Math.Round(achievement * item.Weight.Value, 4)
                : null;

            decimal score = (item.Score ?? 0) * item.IndexWeight;

            switch (item.KPIIndexType)
            {
                case KPIIndexType.KPI: item.KPIScore = score; break;
                case KPIIndexType.KeyTask: item.KeyTaskScore = score; break;
                case KPIIndexType.OMG: item.OMGScore = score; break;
            }

            await Task.CompletedTask;
        }

        #endregion

        #region QUARTER
        private List<int> GetMonthsInQuarter(int month)
        {
            return month switch
            {
                3 => new() { 1, 2, 3 },
                6 => new() { 4, 5, 6 },
                9 => new() { 7, 8, 9 },
                12 => new() { 10, 11, 12 },
                _ => new()
            };
        }

        private async Task<bool> LoadAndCalculateQuarterAsync(
            HandleKPISubmitDto handleKPISubmit,
            int? month)
        {
            if (!month.HasValue) return false;

            var months = GetMonthsInQuarter(month.Value);
            int year = handleKPISubmit.HandleActual.First().Year;


            foreach (var m in months)
            {
                var totals = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                              $"api/kpiSubmit/get-result-total-score-by-month/{CurrenUser.Id}/{year}/{m}");
                if (totals != null) handleKPISubmit.KPITotalScore.AddRange(totals);
                var details = await Http.GetFromJsonAsync<List<HandleActualDto>>(
                    $"api/kpiSubmit/get-result-detail-by-momth/{CurrenUser.Id}/{year}/{m}");
                if (details != null) handleKPISubmit.HandleActual.AddRange(details);

            }
            var monthCountInQuarter = handleKPISubmit.KPITotalScore
                .Count(x => x.Year == year && x.Month.HasValue && months.Contains(x.Month.Value));

            if (monthCountInQuarter < 2) return false;
            await CalculatedQuarter(handleKPISubmit, month.Value);
            return true;
        }
        private async Task CalculatedQuarter(HandleKPISubmitDto handleKPISubmit, int month)
        {
            int quarter = (month + 2) / 3;
            QuarterValue = quarter;

            int year = handleKPISubmit.HandleActual.First().Year;

            var TotalQuarter = new KPITotalScoreDto();
            var newHandleActualList = new List<HandleActualDto>();

            decimal kpiScore = 0;
            decimal omgscore = 0;
            decimal keytaskscore = 0;
            decimal totalScore = 0;
            decimal totalActualScore = 0;
                // 🔹 STEP 1: BUILD DATA
                foreach (var group in handleKPISubmit.HandleActual.GroupBy(x => x.TaskId))
            {
                var first = group.First();
                decimal? actual = null;
                decimal? target = null;
                decimal achivement = 0;
                switch (first.QuarterCalculateType)
                {
                    case QuarterCalculateType.CAL1:
                        actual = group.Sum(x => x.ActualValue ?? 0);
                        target = group.Sum(x => x.TargetValue ?? 0);
                     
                        break;

                    case QuarterCalculateType.CAL2:
                        actual = group.Where(x => x.ActualValue.HasValue).Average(x => x.ActualValue);
                        target = group.Where(x => x.TargetValue.HasValue).Average(x => x.TargetValue);
                     
                        break;

                    case QuarterCalculateType.CAL3:
                        var last = group.OrderByDescending(x => x.Month).First();
                        actual = last.ActualValue;
                        target = last.TargetValue;
                      
                        break;
                }

                achivement = await HelperCalculated(first.KPIFormulaType, actual ?? 0, target ?? 0, first.Max.Value);
                if (first.Bottom.HasValue && achivement < first.Bottom)
                    achivement = 0;

                if (first.Max.HasValue && achivement > first.Max)
                    achivement = first.Max.Value;

                var newItem = new HandleActualDto
                {
                    TaskId = first.TaskId,
                    Year = year,
                    Quarter = quarter,
                    Month = null,

                    TaskName = first.TaskName,
                    KPIIndexType = first.KPIIndexType,
                    KPICategory = first.KPICategory,

                    QuarterCalculateType = first.QuarterCalculateType,
                    KPIFormulaType = first.KPIFormulaType,
                    Max = first.Max,
                    Bottom = first.Bottom,
                    Weight = first.Weight,
                    IndexWeight = first.IndexWeight,
                    calculated = first.calculated,
                    Description = first.Description,
                    CalculateMethod = first.CalculateMethod,
                    Formula = first.Formula,
                    Unit = first.Unit,
                    DataSource = first.DataSource,
                    PIC = first.PIC,

                    ActualValue = actual,
                    TargetValue = target,
                    Achiement = achivement,

                    Score = achivement != 0 && first.Weight.HasValue
                        ? Math.Round(achivement * first.Weight.Value, 4, MidpointRounding.AwayFromZero)
                        : null
                };

                newHandleActualList.Add(newItem);
            }

            // 🔹 STEP 2: ADD DATA SAU KHI BUILD XONG
            handleKPISubmit.HandleActual.AddRange(newHandleActualList);
            
            var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList()
                                ?? new List<IndexWeightDto>();
            decimal omgWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
            decimal kpiWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
            decimal keyTaskWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;
            // 🔹 STEP 3: CALCULATE SCORE (SAU KHI CÓ DATA ĐẦY ĐỦ)
            var listOMGDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.OMG && x.Quarter == quarter)
                .ToList();

            var listKPIDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.KPI && x.Quarter == quarter)
                .ToList();

            var listKeyTaskDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.KeyTask && x.Quarter == quarter)
                .ToList();

            decimal sumOMG = listOMGDetails.Sum(x => x.Score ?? 0);
            decimal sumKPI = listKPIDetails.Sum(x => x.Score ?? 0);
            decimal sumKeyTask = listKeyTaskDetails.Sum(x => x.Score ?? 0);

            omgscore = sumOMG * omgWeight;
            kpiScore = sumKPI * kpiWeight;
            keytaskscore = sumKeyTask * keyTaskWeight;

            totalScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore);
            totalActualScore = totalScore;
            if (totalScore > CeilingLevel.CeilingLevelValue && CeilingLevel.CeilingLevelValue > 0)
            {
                totalScore = CeilingLevel.CeilingLevelValue;
            }

            TotalQuarter = new KPITotalScoreDto
            {
                Title = $"EMC - {year} - Q{quarter} - {CurrenUser.FullName}",
                Year = year,
                Quarter = quarter,
                KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                TotaleScore = totalScore,
                TotalActualScore = totalActualScore,
                CreatedBy = CurrenUser.Id
            };
            handleKPISubmit.KPITotalScore.Add(TotalQuarter);
            // 🔹 STEP 4: REMOVE OLD DATA
            var removeHandleActual = handleKPISubmit.HandleActual
                .Where(x => x.Month != month && x.Quarter == null)
                .ToList();

            var removeTotalScore = handleKPISubmit.KPITotalScore
                .Where(x => x.Month != month && x.Quarter == null)
                .ToList();

            removeHandleActual.ForEach(x => handleKPISubmit.HandleActual.Remove(x));
            removeTotalScore.ForEach(x => handleKPISubmit.KPITotalScore.Remove(x));

            await Task.CompletedTask;
        }
        #endregion

        #region HALF YEAR
        private async Task LoadAndCalculateHalfYearAsync(
            HandleKPISubmitDto handleKPISubmit,
            int? month)
        {
            if (!month.HasValue) return;
            int quarter = month <= 6 ? 1 : 3;
            int year = handleKPISubmit.HandleActual.First().Year;

            var totalQuarters = await SafeGetFromJsonAsync<KPITotalScoreDto>(
                $"api/kpiSubmit/get-result-total-score-by-quarter/{CurrenUser.Id}/{year}/{quarter}");
            //if (totalQuarters != null) handleKPISubmit.KPITotalScore.Add(totalQuarters);
            if (totalQuarters == null) return;


            var details = await SafeGetFromJsonAsync<List<HandleActualDto>>(
             $"api/kpiSubmit/get-result-detail-quarter/{CurrenUser.Id}/{year}/{quarter}");
            if (details != null) handleKPISubmit.HandleActual.AddRange(details);
            if (details == null || !details.Any()) return;

            //if (handleKPISubmit.KPITotalScore.Count < 1) return;
            await CalculateHalfYear(handleKPISubmit, month, totalQuarters);
        }

        private async Task CalculateHalfYear(
            HandleKPISubmitDto handleKPISubmit,
            int? month,
            KPITotalScoreDto quarterTotal)
        {
            int haftyear = month <= 6 ? 1 : 2;
            int quarterOfHalfYear = month <= 6 ? 2 : 4;
            int year = handleKPISubmit.HandleActual.First().Year;

            var TotalQuarter = new KPITotalScoreDto();
            var newHandleActualList = new List<HandleActualDto>();

            decimal kpiScore = 0;
            decimal omgscore = 0;
            decimal keytaskscore = 0;
            decimal totalScore = 0;

            bool useNewWayForHalfYear = false;
            if (CurrenUser.IsChangeTask && CurrenUser.ChangeTaskMonth.HasValue)
            {
                if (CurrenUser.ChangeTaskMonth.Value == 4 && haftyear == 1)
                    useNewWayForHalfYear = true;
                else if (CurrenUser.ChangeTaskMonth.Value == 7 && haftyear == 2)
                    useNewWayForHalfYear = true;
            }

            if (!useNewWayForHalfYear)
            {
                // 🔹 STEP 1: BUILD DATA
                foreach (var group in handleKPISubmit.HandleActual.Where(x => x.Quarter != null).GroupBy(x => x.TaskId))
            {
                var first = group.First();
                decimal? actual = null;
                decimal? target = null;

                switch (first.QuarterCalculateType)
                {
                    case QuarterCalculateType.CAL1:
                        actual = group.Sum(x => x.ActualValue ?? 0);
                        target = group.Sum(x => x.TargetValue ?? 0);
                        break;

                    case QuarterCalculateType.CAL2:
                        actual = group.Where(x => x.ActualValue.HasValue).Average(x => x.ActualValue);
                        target = group.Where(x => x.TargetValue.HasValue).Average(x => x.TargetValue);
                        break;

                    case QuarterCalculateType.CAL3:
                        var last = group.OrderByDescending(x => x.Quarter).First();
                        actual = last.ActualValue;
                        target = last.TargetValue;
                        break;
                }

                decimal achivement = await HelperCalculated(first.KPIFormulaType, actual ?? 0, target ?? 0, first.Max ?? 0);

                if (first.Bottom.HasValue && achivement < first.Bottom)
                    achivement = 0;

                if (first.Max.HasValue && achivement > first.Max)
                    achivement = first.Max.Value;

                var newItem = new HandleActualDto
                {
                    TaskId = first.TaskId,
                    Year = year,
                    Quarter = null,
                    Month = null,

                    TaskName = first.TaskName,
                    KPIIndexType = first.KPIIndexType,
                    KPICategory = first.KPICategory,
                    HalfYear = haftyear,
                    QuarterCalculateType = first.QuarterCalculateType,
                    KPIFormulaType = first.KPIFormulaType,
                    Max = first.Max,
                    Bottom = first.Bottom,
                    Weight = first.Weight,
                    IndexWeight = first.IndexWeight,
                    calculated = first.calculated,
                    Description = first.Description,
                    CalculateMethod = first.CalculateMethod,
                    Formula = first.Formula,
                    Unit = first.Unit,
                    DataSource = first.DataSource,
                    PIC = first.PIC,

                    ActualValue = actual,
                    TargetValue = target,
                    Achiement = achivement,

                    Score = achivement != 0 && first.Weight.HasValue
                        ? Math.Round(achivement * first.Weight.Value, 4, MidpointRounding.AwayFromZero)
                        : null
                };

                newHandleActualList.Add(newItem);
            }

            // 🔹 STEP 2: ADD DATA SAU KHI BUILD XONG
            handleKPISubmit.HandleActual.AddRange(newHandleActualList);
           
            var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList()
                                ?? new List<IndexWeightDto>();
            decimal omgWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
            decimal kpiWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
            decimal keyTaskWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;
            // 🔹 STEP 3: CALCULATE SCORE (SAU KHI CÓ DATA ĐẦY ĐỦ)
            var listOMGDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.OMG && x.HalfYear == haftyear)
                .ToList();

            var listKPIDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.KPI && x.HalfYear == haftyear)
                .ToList();

            var listKeyTaskDetails = handleKPISubmit.HandleActual
                .Where(x => x.KPIIndexType == KPIIndexType.KeyTask && x.HalfYear == haftyear)
                .ToList();

            decimal sumOMG = listOMGDetails.Sum(x => x.Score ?? 0);
            decimal sumKPI = listKPIDetails.Sum(x => x.Score ?? 0);
            decimal sumKeyTask = listKeyTaskDetails.Sum(x => x.Score ?? 0);

            omgscore = sumOMG * omgWeight;
            kpiScore = sumKPI * kpiWeight;
            keytaskscore = sumKeyTask * keyTaskWeight;
            

            totalScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore);
            }
            else
            {
                // 🔹 IsChangeTask = true: weight của 2 quý có thể khác nhau (đổi vị trí giữa Q1/Q2)
                // -> KHÔNG tính riêng KPI/KeyTask/OMG nữa, chỉ trung bình cộng TotaleScore của 2 quý đã chốt.


                var quarter2Total = handleKPISubmit.KPITotalScore
                    .FirstOrDefault(x => x.Quarter == quarterOfHalfYear && x.Year == year);

                kpiScore = 0;
                keytaskscore = 0;
                omgscore = 0;

                totalScore = (quarterTotal != null && quarter2Total != null)
                    ? ConvertNumberCommon.ConvertNumber((quarterTotal.TotaleScore + quarter2Total.TotaleScore) / 2)
                    : 0;
            }

            if (totalScore > CeilingLevel.CeilingLevelValue && CeilingLevel.CeilingLevelValue > 0)
            {
                totalScore = CeilingLevel.CeilingLevelValue;
            }

            TotalQuarter = new KPITotalScoreDto
            {
                Title = $"EMC - {year} - H{haftyear} - {CurrenUser.FullName}",
                Year = year,
                HalfYear = haftyear,
                KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                TotaleScore = totalScore,
                CreatedBy = CurrenUser.Id
            };
            handleKPISubmit.KPITotalScore.Add(TotalQuarter);
            // 🔹 STEP 4: REMOVE OLD DATA
            //var removeHandleActual = handleKPISubmit.HandleActual
            //    .Where(x => x.Quarter != quarterOfHalfYear && x.HalfYear == null)
            //    .ToList();

            //var removeTotalScore = handleKPISubmit.KPITotalScore
            //    .Where(x => x.Quarter != quarterOfHalfYear && x.HalfYear == null)
            //    .ToList();
            var removeHandleActual = handleKPISubmit.HandleActual
                .Where(x => x.HalfYear == null && x.Quarter != quarterOfHalfYear && x.Month != month)
                .ToList();

            var removeTotalScore = handleKPISubmit.KPITotalScore
                .Where(x => x.HalfYear == null && x.Quarter != quarterOfHalfYear && x.Month != month)
                .ToList();

            removeHandleActual.ForEach(x => handleKPISubmit.HandleActual.Remove(x));
            removeTotalScore.ForEach(x => handleKPISubmit.KPITotalScore.Remove(x));

            await Task.CompletedTask;
        }

        #endregion

        #region YEAR CALCULATION
        private async Task LoadAndCalculateYearAsync(HandleKPISubmitDto handleKPISubmit, int month)
        {
            var firstItem = handleKPISubmit.HandleActual.FirstOrDefault();
            if (firstItem == null) return;
            int year = firstItem.Year;

            // 1. Precheck điều kiện từ Server
            var precheck = await SafeGetFromJsonAsync<YearPrecheckResultDto>(
                $"api/kpisubmit/check-year-eligibility/{CurrenUser.Id}/{year}");

            if (precheck == null || !precheck.IsEligible)
            {
                IsYearIneligible = true;
                YearPrecheckResult = precheck ?? new();
                var missingList = precheck?.InvalidMonths ?? new List<int>();
                YearIneligibleMessage = $"Không thể nộp KPI Tháng 12 do thiếu điểm các tháng: {string.Join(", ", missingList.Select(m => "Tháng " + m))}. Vui lòng hoàn tất phê duyệt hoặc cấu hình miễn KPI trước khi nộp.";
                await Message.Error(YearIneligibleMessage);
                return;
            }

            IsYearIneligible = false;
            YearIneligibleMessage = string.Empty;
            YearPrecheckResult = precheck;

            if (precheck.IsFullYearScenario && !CurrenUser.IsChangeTask)
            {
                // KỊCH BẢN 1: Đủ 12 tháng làm việc VÀ KHÔNG thay đổi nhiệm vụ (IsChangeTask = false)
                var allYearActuals = new List<HandleActualDto>();
                for (int m = 1; m <= 11; m++)
                {
                    var details = await SafeGetFromJsonAsync<List<HandleActualDto>>(
                        $"api/kpiSubmit/get-result-detail-by-momth/{CurrenUser.Id}/{year}/{m}");
                    if (details != null && details.Any())
                    {
                        allYearActuals.AddRange(details);
                    }
                }
                var month12Details = handleKPISubmit.HandleActual.Where(x => x.Month == 12).ToList();
                allYearActuals.AddRange(month12Details);

                await CalculateYearFromActuals(handleKPISubmit, allYearActuals, year);
            }
            else
            {
                // KỊCH BẢN 2: Có tháng miễn KPI thực tế HOẶC IsChangeTask = true -> Trung bình cộng các tháng có điểm thực tế
                var approvedPoints = new List<decimal>();
                var monthsToFetch = precheck.ApprovedMonths.Any()
                    ? precheck.ApprovedMonths
                    : Enumerable.Range(1, 11).ToList();

                foreach (var m in monthsToFetch)
                {
                    var totals = await SafeGetFromJsonAsync<List<KPITotalScoreDto>>(
                        $"api/kpiSubmit/get-result-total-score-by-month/{CurrenUser.Id}/{year}/{m}");
                    var item = totals?.FirstOrDefault();
                    if (item != null)
                    {
                        var pt = item.TotalActualScore > 0 ? item.TotalActualScore : item.TotaleScore;
                        approvedPoints.Add(pt);
                    }
                }

                var m12 = handleKPISubmit.KPITotalScore.FirstOrDefault(x => x.Month == 12);
                if (m12 != null)
                {
                    var pt12 = m12.TotalActualScore > 0 ? m12.TotalActualScore : m12.TotaleScore;
                    approvedPoints.Add(pt12);
                }

                decimal avgScore = approvedPoints.Any()
                    ? Math.Round(approvedPoints.Average(), 4, MidpointRounding.AwayFromZero)
                    : 0;

                decimal finalScore = (CeilingLevel.CeilingLevelValue > 0 && avgScore > CeilingLevel.CeilingLevelValue)
                    ? CeilingLevel.CeilingLevelValue
                    : avgScore;

                var yearTotal = new KPITotalScoreDto
                {
                    Title = $"EMC - {year} - {CurrenUser.FullName}",
                    Year = year,
                    Month = null,
                    Quarter = null,
                    HalfYear = null,
                    KPIScore = 0,
                    KeyTaskScore = 0,
                    OMGScore = 0,
                    TotaleScore = avgScore,
                    TotalActualScore = finalScore,
                    CreatedBy = CurrenUser.Id
                };

                handleKPISubmit.KPITotalScore.Add(yearTotal);
            }
        }

        private async Task CalculateYearFromActuals(
            HandleKPISubmitDto handleKPISubmit,
            List<HandleActualDto> allYearActuals,
            int year)
        {
            var newHandleActualList = new List<HandleActualDto>();

            foreach (var group in allYearActuals.GroupBy(x => x.TaskId))
            {
                var first = group.First();
                decimal? actual = null;
                decimal? target = null;

                switch (first.QuarterCalculateType)
                {
                    case QuarterCalculateType.CAL1:
                        actual = group.Sum(x => x.ActualValue ?? 0);
                        target = group.Sum(x => x.TargetValue ?? 0);
                        break;

                    case QuarterCalculateType.CAL2:
                        actual = group.Where(x => x.ActualValue.HasValue).Average(x => x.ActualValue);
                        target = group.Where(x => x.TargetValue.HasValue).Average(x => x.TargetValue);
                        break;

                    case QuarterCalculateType.CAL3:
                        var last = group.OrderByDescending(x => x.Month ?? 0).First();
                        actual = last.ActualValue;
                        target = last.TargetValue;
                        break;
                }

                decimal achivement = await HelperCalculated(first.KPIFormulaType, actual ?? 0, target ?? 0, first.Max ?? 0);

                if (first.Bottom.HasValue && achivement < first.Bottom)
                    achivement = 0;

                if (first.Max.HasValue && achivement > first.Max)
                    achivement = first.Max.Value;

                var newItem = new HandleActualDto
                {
                    TaskId = first.TaskId,
                    Year = year,
                    Quarter = null,
                    Month = null,
                    HalfYear = null,

                    TaskName = first.TaskName,
                    KPIIndexType = first.KPIIndexType,
                    KPICategory = first.KPICategory,

                    QuarterCalculateType = first.QuarterCalculateType,
                    KPIFormulaType = first.KPIFormulaType,
                    Max = first.Max,
                    Bottom = first.Bottom,
                    Weight = first.Weight,
                    IndexWeight = first.IndexWeight,
                    calculated = first.calculated,
                    Description = first.Description,
                    CalculateMethod = first.CalculateMethod,
                    Formula = first.Formula,
                    Unit = first.Unit,
                    DataSource = first.DataSource,
                    PIC = first.PIC,

                    ActualValue = actual,
                    TargetValue = target,
                    Achiement = achivement,

                    Score = achivement != 0 && first.Weight.HasValue
                        ? Math.Round(achivement * first.Weight.Value, 4, MidpointRounding.AwayFromZero)
                        : null
                };

                newHandleActualList.Add(newItem);
            }

            handleKPISubmit.HandleActual.AddRange(newHandleActualList);

            var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList()
                                ?? new List<IndexWeightDto>();
            decimal omgWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
            decimal kpiWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
            decimal keyTaskWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

            var yearActuals = handleKPISubmit.HandleActual
                .Where(x => x.Year == year && x.Month == null && x.Quarter == null && x.HalfYear == null)
                .ToList();

            decimal sumOMG = yearActuals.Where(x => x.KPIIndexType == KPIIndexType.OMG).Sum(x => x.Score ?? 0);
            decimal sumKPI = yearActuals.Where(x => x.KPIIndexType == KPIIndexType.KPI).Sum(x => x.Score ?? 0);
            decimal sumKeyTask = yearActuals.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).Sum(x => x.Score ?? 0);

            decimal omgscore = sumOMG * omgWeight;
            decimal kpiScore = sumKPI * kpiWeight;
            decimal keytaskscore = sumKeyTask * keyTaskWeight;

            decimal totalScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore);
            decimal totalActualScore = totalScore;
            if (CeilingLevel.CeilingLevelValue > 0 && totalScore > CeilingLevel.CeilingLevelValue)
            {
                totalActualScore = CeilingLevel.CeilingLevelValue;
            }

            var yearTotal = new KPITotalScoreDto
            {
                Title = $"EMC - {year} - {CurrenUser.FullName}",
                Year = year,
                Month = null,
                Quarter = null,
                HalfYear = null,
                KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                TotaleScore = totalScore,
                TotalActualScore = totalActualScore,
                CreatedBy = CurrenUser.Id
            };
            handleKPISubmit.KPITotalScore.Add(yearTotal);
        }
        #endregion

        #region HELPER

        private async Task AddCurrenTask(HandleKPISubmitDto handleKPISubmit)
        {
            CurrenTotalScore = new KPITotalScoreDto
            {
                Title = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName}",
                KPIScore = handleKPISubmit.HandleActual.Sum(x => x.KPIScore ?? 0),
                KeyTaskScore = handleKPISubmit.HandleActual.Sum(x => x.KeyTaskScore ?? 0),
                OMGScore = handleKPISubmit.HandleActual.Sum(x => x.OMGScore ?? 0),
                TotaleScore = handleKPISubmit.HandleActual.Sum(x =>
                    (x.KPIScore ?? 0) +
                    (x.KeyTaskScore ?? 0) +
                    (x.OMGScore ?? 0)),
                TotalActualScore = handleKPISubmit.HandleActual.Sum(x =>
                   (x.KPIScore ?? 0) +
                   (x.KeyTaskScore ?? 0) +
                   (x.OMGScore ?? 0)),
                Month = KPIPeriodSubmit.Month,
                Year = KPIPeriodSubmit.Year,
                CreatedBy = CurrenUser.Id
            };
            if (CurrenTotalScore.TotaleScore > CeilingLevel.CeilingLevelValue && CeilingLevel.CeilingLevelValue > 0)
            {
                CurrenTotalScore.TotaleScore = CeilingLevel.CeilingLevelValue;
            }
            handleKPISubmit.KPITotalScore.Add(CurrenTotalScore);
        }
        private async Task<decimal> HelperCalculated(KPIFormulaType kPIFormulaType, decimal actual, decimal target, decimal max)
        {
            try
            {
                decimal achievement = 0m;
                if (kPIFormulaType == KPIFormulaType.KF1)
                {
                    if (target == 0)
                    {
                        achievement = actual >= 0 ? max * 100 : 0;
                    }
                    else
                    {
                        achievement = actual / target;
                    }
                }
                if (kPIFormulaType == KPIFormulaType.KF2)
                {
                    if(target == 0)
                    {
                        achievement = actual > 0 ? 0 : max * 100;
                    }
                    else                    {
                         achievement = 2 - (actual / target);
                    }
                }
                if (kPIFormulaType == KPIFormulaType.KF3)
                {
                    if(target == 0)
                    {
                        achievement = actual > 0 ? max * 100 : 0;
                    }
                    else
                    {
                        achievement = target > 0 ? actual / target : 2 - (actual / target);
                    }
                   
                }

                return achievement;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private async Task<T?> SafeGetFromJsonAsync<T>(string url) where T : class
        {
            try
            {
                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null") return null;

                return System.Text.Json.JsonSerializer.Deserialize<T>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SafeGetFromJsonAsync ERROR] {url} -> {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
