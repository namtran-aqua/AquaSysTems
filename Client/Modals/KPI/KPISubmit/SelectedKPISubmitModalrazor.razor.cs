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
            if (!CurrenUser.IsChangeTask)
            {
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
            }
            else
            {
                // 🔹 IsChangeTask = true: weight quý hiện tại KHÔNG áp dụng đúng cho các tháng trước khi đổi vị trí
                // -> KHÔNG tính riêng KPI/KeyTask/OMG nữa, chỉ trung bình cộng TotaleScore đã chốt của từng tháng.
                // (Vẫn gắn Quarter vào các dòng chi tiết tháng để hiển thị, nhưng không dùng để tính điểm)
                var monthsInQuarter = GetMonthsInQuarter(month);
                var monthlyDetails = handleKPISubmit.HandleActual
                    .Where(x => x.Quarter == null && x.Month.HasValue && monthsInQuarter.Contains(x.Month.Value))
                    .ToList();

                foreach (var item in monthlyDetails)
                {
                    item.Quarter = quarter;
                }

                var monthlyTotals = handleKPISubmit.KPITotalScore
                    .Where(x => x.Year == year && x.Month.HasValue && monthsInQuarter.Contains(x.Month.Value))
                    .ToList();

                kpiScore = 0;
                keytaskscore = 0;
                omgscore = 0;
                totalScore = monthlyTotals.Any()
                    ? ConvertNumberCommon.ConvertNumber(monthlyTotals.Average(x => x.TotaleScore))
                    : 0;
                totalActualScore = totalScore;
            }
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

            if (!CurrenUser.IsChangeTask)
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
                var quarterlyDetails = handleKPISubmit.HandleActual
                    .Where(x => x.HalfYear == null && x.Quarter.HasValue)
                    .ToList();

                foreach (var item in quarterlyDetails)
                {
                    item.HalfYear = haftyear;
                }

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
