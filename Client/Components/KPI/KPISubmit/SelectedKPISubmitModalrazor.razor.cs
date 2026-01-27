
using AntDesign;
using AquaSolution.Client.Common;
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
        //private List<HandleKPISubmitDto> HandleKPISubmit = new();
        private List<HandleKPISubmitDto> Header = new();
        private UserDto CurrenUser = new();
        private List<DealineManagementDto> DealineManagement = new();
        private List<IndexWeightDto> IndexWeight = new();

        private HandleKPISubmitDto HandleKPISubmitDto = new();
        public DealineManagementDto KPIPeriodSubmit { get; set; }
        private string activeTabKey { get; set; } = string.Empty;
        private string TitleButton = string.Empty;
        private bool IsInputActual { get; set; } = false;
        private string HeaderLine { get; set; } = string.Empty;
        private string PlaceholderSelected { get; set; } = "select month";
        private bool HandleButtonClicked { get; set; }
        #endregion
        #region Init  
        public async Task ShowModal(UserDto currenUser)
        {
            HandleKPISubmitDto = new HandleKPISubmitDto();
            DealineManagement = new List<DealineManagementDto>();
            HandleButtonClicked = false;
            CurrenUser = currenUser;
            await GetDeadline();
            await GetIndexWeight();
            IsInputActual = false;
            activeTabKey = "1";
            TitleButton = "Next";
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task<List<HandleActualDto>> LoadSelectedKPiSubmit(int year, int? month)
        {

            var result = await Http.GetFromJsonAsync<List<HandleActualDto>>($"api/kpisubmit/get-kpi-submit/{CurrenUser.Id}/{year}/{month}");
            if (result != null)
            {
                return result;
            }
            return new List<HandleActualDto>();
        }
        private async Task GetDeadline()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<DealineManagementDto>>($"api/DealineManagement/get-deadline/{CurrenUser.Id}");
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

            HandleButtonClicked = true;
            var message = $"Are you sure you want to submit?";
            var confirm = await MessageBox.Confirm(Modal, message);
            if (!confirm)
            {
                HandleButtonClicked = false;
                await InvokeAsync(StateHasChanged);
                return;
            }
            var response = await Http.PostAsJsonAsync($"api/kpisubmit/create", HandleKPISubmitDto);
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
            TitleButton = "Next ⟶";
            IsModalVisible = false;
        }
        private async Task SelectedChange()
        {
            if (KPIPeriodSubmit != null)
            {
                HandleKPISubmitDto.HandleActual = await LoadSelectedKPiSubmit(KPIPeriodSubmit.Year, KPIPeriodSubmit.Month);
                if (HandleKPISubmitDto.HandleActual != null && HandleKPISubmitDto.HandleActual.Any())
                {
                    IsInputActual = true;
                }
            }

            activeTabKey = "1";
            TitleButton = "Next ⟶";
        }
        private async Task ActionTab()
        {
            HandleKPISubmitDto.KPITotalScore = new();
            if (activeTabKey == "1")
            {

                activeTabKey = "2";
                TitleButton = "⟵ Back";
                foreach (var item in HandleKPISubmitDto.HandleActual)
                {
                    await CalculatedScore(item);
                    item.HeaderTitle = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName}";

                }
                await AddCurrenTask(HandleKPISubmitDto);
                var kpiScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.KPIScore) ?? 0;
                var keyTaskScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.KeyTaskScore) ?? 0;
                var omgScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.OMGScore) ?? 0;

                var totalScore = keyTaskScore + kpiScore + omgScore;

                HeaderLine = $"{(KPIPeriodSubmit.MonthString)}  {CurrenUser.FullName}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"KPI Score:{ConvertNumberCommon.ConvertNumber(kpiScore)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"KeyTaskScore:{ConvertNumberCommon.ConvertNumber(keyTaskScore)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"OMGScore:{ConvertNumberCommon.ConvertNumber(omgScore)}"
                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
                         + $"Total Score:{ConvertNumberCommon.ConvertNumber(totalScore)}";
                if (HandleKPISubmitDto.HandleActual != null)
                {
                    if (HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 3 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 6 ||
                       HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 9 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 12)
                    {
                        await CalculateQuarterScores(HandleKPISubmitDto, HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month);
                    }
                    if (HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 6 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 12)
                    {
                        await CalculateHalfYearScoresFromQuarters(HandleKPISubmitDto, HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month);
                    }
                }
            }
            else if (activeTabKey == "2")
            {
                activeTabKey = "1";
                TitleButton = "Next ⟶";
            }
        }
        private Task AddCurrenTask(HandleKPISubmitDto handleKPISubmit)
        {
            CurrenTotalScore = new KPITotalScoreDto();
            decimal totalKPI = handleKPISubmit.HandleActual.Sum(x => x.KPIScore ?? 0);
            decimal totalKeyTask = handleKPISubmit.HandleActual.Sum(x => x.KeyTaskScore ?? 0);
            decimal omgScore = handleKPISubmit.HandleActual.Sum(x => x.OMGScore ?? 0);
            decimal totalScore = totalKPI + totalKeyTask;
            var listSetValue = handleKPISubmit.HandleActual.Where(x => x.Month != null);
            CurrenTotalScore.Title = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName} ";
            CurrenTotalScore.KPIScore = totalKPI;
            CurrenTotalScore.KeyTaskScore = totalKeyTask;
            CurrenTotalScore.TotaleScore = totalScore;
            CurrenTotalScore.OMGScore = 0;
            CurrenTotalScore.Month = handleKPISubmit.HandleActual.FirstOrDefault()?.Month;
            CurrenTotalScore.Year = handleKPISubmit.HandleActual.FirstOrDefault()?.Year ?? 0;
            CurrenTotalScore.CreatedBy = CurrenUser.Id;
            return Task.CompletedTask;
        }
        private KPITotalScoreDto CurrenTotalScore = new();
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
        private async Task CalculatedScore(HandleActualDto handleKPISubmit)
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
                    case KPIFormulaType.KF4:
                        if (actual > target)
                        {
                            achievement = 0;
                        }
                        else
                        {
                            achievement = 1;
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


        private async Task CalculateQuarterScores(HandleKPISubmitDto handleKPISubmit, int? month)
        {
            try
            {
                if (!month.HasValue || handleKPISubmit == null || !handleKPISubmit.HandleActual.Any())
                    return;

                var listMonths = GetMonthsInQuarter(month.Value);
                var actuals = new List<KPITotalScoreDto>();
                var listResultDetail = new List<HandleActualDto>();
                foreach (var m in listMonths)
                {
                    // lấy Total để validate đủ 2 tahngs trở lên không
                    var result = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                        $"api/kpiSubmit/get-result-kpi/{CurrenUser.Id}/{handleKPISubmit.HandleActual.First().Year}/{m}");

                    if (result != null && result.Any())
                    {
                        actuals.AddRange(result);
                    }
                    // lấy Detail để tính 3 tháng cho quý
                    var resultOMG = await Http.GetFromJsonAsync<List<HandleActualDto>>(
                       $"api/kpiSubmit/get-result-omg/{CurrenUser.Id}/{handleKPISubmit.HandleActual.First().Year}/{m}");
                    if (resultOMG != null && resultOMG.Any())
                    {
                        listResultDetail.AddRange(resultOMG);
                    }
                }
                listResultDetail.AddRange(handleKPISubmit.HandleActual.Where(x => x.Month == month));

                //validate
                var currentMonth = handleKPISubmit.HandleActual.First().Month;
                if (!actuals.Any(x => x.Month == currentMonth))
                {

                    actuals.Add(CurrenTotalScore);

                }
                if (actuals.Count < 2)
                    return;
             

                decimal kpiScore = 0;
                decimal omgscore = 0;
                decimal keytaskscore = 0;

                var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList() ?? new List<IndexWeightDto>();

                decimal omgWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
                decimal kpiWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
                decimal keyTaskWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

                if (listResultDetail != null && listResultDetail.Any())
                {
                    // OMG
                    var listOMGDetails = listResultDetail.Where(x => x.KPIIndexType == KPIIndexType.OMG).ToList();
                    decimal sumOMG = listOMGDetails.Sum(x => x.Score ?? 0);
                    var avgOMG = listOMGDetails.Count > 0 ? sumOMG / listOMGDetails.Count : 0;
                    omgscore = avgOMG * omgWeight;

                    // KPI
                    var listKPIDetails = listResultDetail.Where(x => x.KPIIndexType == KPIIndexType.KPI).ToList();
                    decimal sumKPI = listKPIDetails.Sum(x => x.Score ?? 0);
                    var avgKPI = listKPIDetails.Count > 0 ? sumKPI / listKPIDetails.Count : 0;
                    kpiScore = avgKPI * kpiWeight;

                    // KeyTask
                    var listKeyTaskDetails = listResultDetail.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).ToList();
                    decimal sumKeyTask = listKeyTaskDetails.Sum(x => x.Score ?? 0);
                    var avgKeyTask = listKeyTaskDetails.Count > 0 ? sumKeyTask / listKeyTaskDetails.Count : 0;
                    keytaskscore = avgKeyTask * keyTaskWeight;
                }

                decimal totalScore = kpiScore + keytaskscore + omgscore;

                int quarter = (month.Value + 2) / 3;

                // Kiểm tra đã có chưa
                //bool alreadyExists = handleKPISubmit.KPITotalScore.Any(x => x.Quarter == quarter && x.Month == null && x.HalfYear == null);
                //if (!alreadyExists)
                //{
                    var quarterScore = new KPITotalScoreDto
                    {
                        KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                        KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                        OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                        TotaleScore = ConvertNumberCommon.ConvertNumber(totalScore),
                        Title = $"EMC - {handleKPISubmit.HandleActual.First().Year} - Q{quarter} - {CurrenUser.FullName} ",
                        Year = handleKPISubmit.HandleActual.First().Year,
                        Month = null,
                        Quarter = quarter,
                        CreatedBy = CurrenUser.Id
                    };

                    handleKPISubmit.KPITotalScore.Add(quarterScore);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] CalculateQuarterScores Failed: {ex.Message}");
            }

            await Task.CompletedTask;
        }
        #endregion
        #region HandleHalfYearlyScore
        private async Task CalculateHalfYearScoresFromQuarters(HandleKPISubmitDto handleKPISubmit, int? month)
        {
            if (handleKPISubmit == null || !handleKPISubmit.KPITotalScore.Any())
                return;
            int halfYear = (month.Value <= 6) ? 1 : 2;
            var quarterNumbers = month == 6 ? 1 : 3;
            var quarterScores = new List<KPITotalScoreDto>();
            var result = await Http.GetFromJsonAsync<KPITotalScoreDto>(
                $"api/kpisubmit/get-result-quarter/{CurrenUser.Id}/{handleKPISubmit.HandleActual.First().Year}/{quarterNumbers}");
            if (result != null)
            {
                quarterScores.Add(result);
            }
            if (handleKPISubmit.KPITotalScore.Any())
            {
                quarterScores.Add(handleKPISubmit.KPITotalScore.First(x => x.Quarter == (quarterNumbers + 1)));
            }
            if (quarterScores.Count != 2)
                return;

            // Trung bình cộng
            decimal avgKPI = quarterScores.Average(x => x.KPIScore);
            decimal avgKeyTask = quarterScores.Average(x => x.KeyTaskScore);
            decimal avgOMG = quarterScores.Average(x => x.OMGScore);
            decimal avgTotal = quarterScores.Average(x => x.TotaleScore);
            //bool alreadyExists = handleKPISubmit.KPITotalScore.Any(x => x.HalfYear == halfYear && x.Month == null && x.Quarter == null);
            //if (!alreadyExists)
            //{
                var halfYearScore = new KPITotalScoreDto
                {
                    KPIScore = Math.Round(avgKPI, 2),
                    KeyTaskScore = Math.Round(avgKeyTask, 2),
                    OMGScore = Math.Round(avgOMG, 2),
                    TotaleScore = Math.Round(avgTotal, 2),
                    Title = $"EMC - {handleKPISubmit.HandleActual.First().Year} - H{halfYear} - {CurrenUser.FullName} ",
                    Year = handleKPISubmit.HandleActual.First().Year,
                    HalfYear = halfYear,
                    CreatedBy = CurrenUser.Id
                };

                handleKPISubmit.KPITotalScore.Add(halfYearScore);
            //}
        }
        #endregion

    }
}
