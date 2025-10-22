
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.DealineManagement;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.KPISubmit
{
    public partial class SelectedKPISubmitModalrazor
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsModalVisible { get; set; } = false;
        private List<HandleKPISubmitDto> HandleKPISubmit = new();
        private List<HandleKPISubmitDto> Header = new();
        private UserDto CurrenUser = new();
        private List<DealineManagementDto> DealineManagement = new();
        private List<IndexWeightDto> IndexWeight = new();

        public DealineManagementDto KPIPeriodSubmit { get; set; }
        private string activeTabKey { get; set; } = string.Empty;
        private string TitleButton = string.Empty;
        private bool IsInputActual { get; set; } = false;
        private string HeaderLine { get; set; } = string.Empty;
        private string PlaceholderSelected { get; set; } = "select month";
        #endregion
        #region Init  
        public async Task ShowModal(UserDto currenUser)
        {
            HandleKPISubmit = new List<HandleKPISubmitDto>();
            CurrenUser = currenUser;
            await GetDeadline();
            await GetIndexWeight();
            IsInputActual = false;
            activeTabKey = "1";
            TitleButton = "Next";
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task<List<HandleKPISubmitDto>> LoadSelectedKPiSubmit(int year, int? month)
        {

            var result = await Http.GetFromJsonAsync<List<HandleKPISubmitDto>>($"api/kpisubmit/get-kpi-submit/{CurrenUser.Id}/{year}/{month}");
            if (result != null)
            {
                return result;
            }
            return new List<HandleKPISubmitDto>();
        }
        private async Task GetDeadline()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<DealineManagementDto>>("api/DealineManagement/get-deadline");
                if (result.Any())
                {
                    DealineManagement = result;
                    PlaceholderSelected = "select month";
                }
                else
                {
                    PlaceholderSelected = "There are no KPIs to choose from";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] API Call Failed: {ex.Message}");

            }
        }
        private async Task GetIndexWeight()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>($"api/kpisubmit/get-indexweight/{CurrenUser.PositionType}/{PeriodType.Quarter}");
                if (result.Any())
                {
                    IndexWeight = result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] API Call Failed: {ex.Message}");

            }
        }
        #endregion
        #region Actions
        private void CloseModal()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        private async Task SaveAsync()
        {
            var response = await Http.PostAsJsonAsync($"api/kpisubmit/create", HandleKPISubmit);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
            activeTabKey = "1";
            TitleButton = "Next";
        }
        private async Task SelectedChange()
        {
            if (KPIPeriodSubmit != null)
            {
                HandleKPISubmit = await LoadSelectedKPiSubmit(KPIPeriodSubmit.Year, KPIPeriodSubmit.Month);
                if (HandleKPISubmit != null && HandleKPISubmit.Any())
                {
                    IsInputActual = true;
                }
            }

            activeTabKey = "1";
            TitleButton = "Next";
        }
        private async Task ActionTab()
        {

            if (activeTabKey == "1")
            {

                activeTabKey = "2";
                TitleButton = "Back";
                foreach (var item in HandleKPISubmit)
                {
                    await CalculatedScore(item);

                    //if (item.HalfYear != null)
                    //{
                    //    item.HeaderTitle = $"EMC - {CurrenUser.FullName} - H{item.HalfYear}";
                    //}
                    item.HeaderTitle = $"EMC - {CurrenUser.FullName} - {KPIPeriodSubmit.MonthString}";

                }
                SetTotalScoreForAllRows(HandleKPISubmit);


                var totalScore = HandleKPISubmit?.FirstOrDefault()?.TotaleScore ?? 0;

                HeaderLine = $"{(KPIPeriodSubmit.MonthString)}  {CurrenUser.FullName}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"KPI Score:{ConvertNumberCommon.ConvertNumber(HandleKPISubmit?.Sum(x => x.KPIScore) ?? 0)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"KeyTaskScore:{ConvertNumberCommon.ConvertNumber(HandleKPISubmit?.Sum(x => x.KeyTaskScore) ?? 0)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"OMGScore:{ConvertNumberCommon.ConvertNumber(HandleKPISubmit?.Sum(x => x.OMGScore) ?? 0)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"Total Score:{ConvertNumberCommon.ConvertNumber(totalScore)}";
                if (HandleKPISubmit != null)
                {
                    if (HandleKPISubmit?.FirstOrDefault()?.Month == 3 || HandleKPISubmit?.FirstOrDefault()?.Month == 6 ||
                        HandleKPISubmit?.FirstOrDefault()?.Month == 9 || HandleKPISubmit?.FirstOrDefault()?.Month == 12)
                    {
                        await CalculateQuarterScores(HandleKPISubmit, HandleKPISubmit?.FirstOrDefault()?.Month);
                    }
                    if (HandleKPISubmit?.FirstOrDefault()?.Month == 6 || HandleKPISubmit?.FirstOrDefault()?.Month == 12)
                    {
                        await CalculateHalfYearScoresFromQuarters(HandleKPISubmit, HandleKPISubmit?.FirstOrDefault()?.Month);
                    }
                }
            }
            else if (activeTabKey == "2")
            {
                activeTabKey = "1";
                TitleButton = "Next";
            }
        }
        private void SetTotalScoreForAllRows(List<HandleKPISubmitDto> list)
        {
            decimal totalKPI = list.Sum(x => x.KPIScore ?? 0);
            decimal totalKeyTask = list.Sum(x => x.KeyTaskScore ?? 0);
            decimal totalScore = totalKPI + totalKeyTask;
            var listSetValue = list.Where(x => x.Month != null);
            foreach (var item in listSetValue)
            {
                item.TotaleScore = totalScore;
            }
        }
        #endregion
        #region HandleScore
        private List<int> GetMonthsInQuarter(int month)
        {
            if (month == 3) return new List<int> { 1, 2, 3 };
            if (month == 6) return new List<int> { 4, 5, 6 };
            if (month == 9) return new List<int> { 7, 8, 9 };
            if (month == 12) return new List<int> { 10, 11, 12 };
            return new List<int>();
        }
        #endregion
        #region HandleMonthlyScore
        private async Task CalculatedScore(HandleKPISubmitDto handleKPISubmit)
        {
            if (handleKPISubmit == null || !handleKPISubmit.ActualValue.HasValue || !handleKPISubmit.TargetValue.HasValue)
                return;
            if (handleKPISubmit.Month != null)
            {
                decimal actual = handleKPISubmit.ActualValue.Value;
                decimal target = handleKPISubmit.TargetValue.Value;
                decimal achievement = 0;
                decimal totalKPI = 0;
                decimal totalKeyTask = 0;
                decimal totalOMG = 0;
                switch (handleKPISubmit.KPIFormulaType)
                {
                    case KPIFormulaType.KF1:
                        achievement = target == 0 ? 0 : actual / target;
                        break;
                    case KPIFormulaType.KF2:
                        achievement = target == 0 ? 0 : 2 - (actual / target);
                        break;
                    case KPIFormulaType.KF3:
                        if (target > 0)
                        {
                            achievement = actual / target;
                        }
                        else
                        {
                            achievement = 2 - (actual / target);
                        }
                        break;
                    default:
                        achievement = 0;
                        break;
                }

                if (handleKPISubmit.Bottom.HasValue && achievement < handleKPISubmit.Bottom.Value)
                {
                    achievement = 0;
                }
                else if (handleKPISubmit.Max.HasValue && achievement > handleKPISubmit.Max.Value)
                {
                    achievement = handleKPISubmit.Max.Value;
                }

                handleKPISubmit.Achiement = Math.Round(achievement, 4, MidpointRounding.AwayFromZero);

                if (handleKPISubmit.Weight.HasValue)
                {
                    handleKPISubmit.Score = Math.Round(achievement * handleKPISubmit.Weight.Value, 4, MidpointRounding.AwayFromZero);
                }
                else
                {
                    handleKPISubmit.Score = null;
                }

                //-------------------------
                decimal scoreForType = handleKPISubmit.Score.Value * handleKPISubmit.IndexWeight;
                switch (handleKPISubmit.KPIIndexType)
                {
                    case KPIIndexType.KPI:
                        totalKPI += scoreForType;
                        break;
                    case KPIIndexType.KeyTask:
                        totalKeyTask += scoreForType;
                        break;
                    case KPIIndexType.OMG:
                        totalOMG += scoreForType;
                        break;
                }

                switch (handleKPISubmit.KPIIndexType)
                {
                    case KPIIndexType.KPI:
                        handleKPISubmit.KPIScore = totalKPI;
                        break;
                    case KPIIndexType.KeyTask:
                        handleKPISubmit.KeyTaskScore = totalKeyTask;
                        break;
                    case KPIIndexType.OMG:
                        handleKPISubmit.OMGScore = totalOMG;
                        break;
                }
            }

            await Task.CompletedTask;
        }
        #endregion
        #region HandleQuarterlyScore
        private async Task CalculateQuarterScores(List<HandleKPISubmitDto> handleKPISubmit, int? month)
        {
            if (!month.HasValue || handleKPISubmit == null || !handleKPISubmit.Any())
                return;

            var listMonths = GetMonthsInQuarter(month.Value);
            var kpiTotalScoreDto = new List<KPITotalScoreDto>();

            foreach (var m in listMonths)
            {
                var result = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                    $"api/kpisubmit/get-result-kpi/{CurrenUser.Id}/{handleKPISubmit.First().Year}/{m}");

                if (result != null && result.Any())
                {
                    kpiTotalScoreDto.AddRange(result);
                }
            }

            var currentMonth = handleKPISubmit.First().Month;

            if (!kpiTotalScoreDto.Any(x => x.Month == currentMonth))
            {
                var currentData = handleKPISubmit.FirstOrDefault();
                if (currentData != null)
                {
                    kpiTotalScoreDto.Add(new KPITotalScoreDto
                    {
                        KPIScore = currentData.KPIScore ?? 0,
                        KeyTaskScore = currentData.KeyTaskScore ?? 0,
                        OMGScore = currentData.OMGScore ?? 0,
                        TotaleScore = currentData.TotaleScore ?? 0,
                        Month = currentMonth,
                        CreatedBy = CurrenUser.Id,
                        Year = currentData.Year
                    });
                }
            }

            if (kpiTotalScoreDto.Count == 3)
            {
                decimal avgKPI = kpiTotalScoreDto.Sum(x => x.KPIScore) / 3;
                decimal avgKeyTask = kpiTotalScoreDto.Sum(x => x.KeyTaskScore) / 3;
                decimal avgOMG = kpiTotalScoreDto.Sum(x => x.OMGScore) / 3;
                var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList();

                decimal kpiWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
                decimal keyTaskWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;
                decimal omgWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;

                // === 3. Tính điểm tổng ===
                var kpiscore = avgKPI * kpiWeight;
                var keytaskscore = avgKeyTask * keyTaskWeight;
                var omgscore = avgOMG * omgWeight;
                decimal totalScore = kpiscore + keytaskscore + omgscore;
                int quarter = (month.Value + 2) / 3;

                //bool alreadyExists = handleKPISubmit.Any(x => x.HeaderTitle != null && x.HeaderTitle.Contains($"Q{quarter}"));
                //if (!alreadyExists)
                //{
                var quarterScore = new HandleKPISubmitDto
                {
                    KPIScore = Math.Round(kpiscore, 2),
                    KeyTaskScore = Math.Round(keytaskscore, 2),
                    OMGScore = Math.Round(omgscore, 2),
                    TotaleScore = Math.Round(totalScore, 2),
                    HeaderTitle = $"EMC - {CurrenUser.FullName} - Q{quarter}",
                    Year = handleKPISubmit.First().Year,
                    Month = null,
                    Quarter = quarter,
                    CreatedBy = CurrenUser.Id
                };

                handleKPISubmit.Add(quarterScore);
                //}
            }

            await Task.CompletedTask;
        }
        #endregion
        #region HandleHalfYearlyScore
        private async Task CalculateHalfYearScoresFromQuarters(List<HandleKPISubmitDto> handleKPISubmit, int? month)
        {
            if (handleKPISubmit == null || !handleKPISubmit.Any())
                return;
            int halfYear = (month.Value <= 6) ? 1 : 2;
            var quarterNumbers = month == 6 ? new List<int> { 1, 2 } : new List<int> { 3, 4 };
            var quarterScores = new List<HandleKPISubmitDto>();

            foreach (var quarter in quarterNumbers)
            {
                // Ưu tiên lấy từ handleKPISubmit
                var existingQuarter = handleKPISubmit.FirstOrDefault(x => x.Quarter == quarter && x.Month == null);
                if (existingQuarter != null)
                {
                    quarterScores.Add(existingQuarter);
                }
                else
                {
                    // Nếu chưa có thì gọi API để lấy
                    var result = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                        $"api/kpisubmit/get-result-quarter/{CurrenUser.Id}/{handleKPISubmit.First().Year}/{quarter}");

                    var data = result?.FirstOrDefault();
                    if (data != null)
                    {
                        quarterScores.Add(new HandleKPISubmitDto
                        {
                            KPIScore = data.KPIScore,
                            KeyTaskScore = data.KeyTaskScore,
                            OMGScore = data.OMGScore,
                            TotaleScore = data.TotaleScore,
                            Quarter = quarter,
                            Year = data.Year,
                            CreatedBy = CurrenUser.Id,
                        });
                    }
                }
            }

            // Đảm bảo đủ 2 quý
            if (quarterScores.Count != 2)
                return;

            // Trung bình cộng
            decimal avgKPI = quarterScores.Average(x => x.KPIScore ?? 0);
            decimal avgKeyTask = quarterScores.Average(x => x.KeyTaskScore ?? 0);
            decimal avgOMG = quarterScores.Average(x => x.OMGScore ?? 0);
            decimal avgTotal = quarterScores.Average(x => x.TotaleScore ?? 0);

            // Kiểm tra xem đã có Bán niên trong danh sách chưa
            //bool exists = handleKPISubmit.Any(x => x.HalfYear == halfYear && x.Quarter == null && x.Month == null);
            //if (!exists)
            //{
            var halfYearScore = new HandleKPISubmitDto
            {
                KPIScore = Math.Round(avgKPI, 2),
                KeyTaskScore = Math.Round(avgKeyTask, 2),
                OMGScore = Math.Round(avgOMG, 2),
                TotaleScore = Math.Round(avgTotal, 2),
                HeaderTitle = $"EMC - {CurrenUser.FullName} - H {halfYear}",
                Year = handleKPISubmit.First().Year,
                HalfYear = halfYear,
                CreatedBy = CurrenUser.Id
            };

            handleKPISubmit.Add(halfYearScore);
            //}
        }
        #endregion
      
    }
}
