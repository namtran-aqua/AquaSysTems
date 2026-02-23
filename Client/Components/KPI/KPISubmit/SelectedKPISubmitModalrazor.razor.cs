
//using AntDesign;
//using AquaSolution.Client.Common;
//using AquaSolution.Client.Common.ConvertNumber;
//using AquaSolution.Shared.Enum;
//using AquaSolution.Shared.Enum.KPIType;
//using AquaSolution.Shared.KPI.DealineManagement;
//using AquaSolution.Shared.KPI.IndexWeight;
//using AquaSolution.Shared.KPI.KPISubmit;
//using AquaSolution.Shared.UserManagements;
//using Microsoft.AspNetCore.Components;
//using System.Collections.Generic;
//using System.Net.Http.Json;

//namespace AquaSolution.Client.Components.KPI.KPISubmit
//{
//    public partial class SelectedKPISubmitModalrazor
//    {
//        #region Declaration
//        [Inject] private HttpClient Http { get; set; }
//        private bool IsModalVisible { get; set; } = false;
//        private List<HandleKPISubmitDto> Header = new();
//        private UserDto CurrenUser = new();
//        private List<DealineManagementDto> DealineManagement = new();
//        private List<IndexWeightDto> IndexWeight = new();

//        private HandleKPISubmitDto HandleKPISubmitDto = new();
//        private List<HandleActualDto> HandleActualDto = new();

//        public DealineManagementDto KPIPeriodSubmit { get; set; }
//        private string activeTabKey { get; set; } = string.Empty;
//        private string TitleButton = string.Empty;
//        private bool IsInputActual { get; set; } = false;
//        private string HeaderLine { get; set; } = string.Empty;
//        private string PlaceholderSelected { get; set; } = "select month";
//        private bool HandleButtonClicked { get; set; }
//        #endregion
//        #region Init  
//        public async Task ShowModal(UserDto currenUser)
//        {
//            HandleKPISubmitDto = new HandleKPISubmitDto();
//            DealineManagement = new List<DealineManagementDto>();
//            HandleButtonClicked = false;
//            CurrenUser = currenUser;
//            await GetDeadline();
//            await GetIndexWeight();
//            IsInputActual = false;
//            activeTabKey = "1";
//            TitleButton = "Next";
//            IsModalVisible = true;
//            await InvokeAsync(StateHasChanged);
//        }
//        private async Task<List<HandleActualDto>> LoadSelectedKPiSubmit(int year, int? month)
//        {

//            var result = await Http.GetFromJsonAsync<List<HandleActualDto>>($"api/kpisubmit/get-kpi-submit/{CurrenUser.Id}/{year}/{month}");
//            if (result != null)
//            {
//                return result;
//            }
//            return new List<HandleActualDto>();
//        }
//        private async Task GetDeadline()
//        {
//            try
//            {
//                var result = await Http.GetFromJsonAsync<List<DealineManagementDto>>($"api/DealineManagement/get-deadline/{CurrenUser.Id}");
//                if (result.Any())
//                {
//                    DealineManagement = result;
//                    PlaceholderSelected = "select month";
//                }
//                else
//                {
//                    PlaceholderSelected = "There are no KPIs to choose from";
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[CLIENT ERROR] API Call Failed: {ex.Message}");

//            }
//        }
//        private async Task GetIndexWeight()
//        {
//            try
//            {
//                var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>($"api/kpisubmit/get-indexweight/{CurrenUser.PositionType}/{PeriodType.Quarter}");
//                if (result.Any())
//                {
//                    IndexWeight = result;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[CLIENT ERROR] API Call Failed: {ex.Message}");

//            }
//        }
//        #endregion
//        #region Actions
//        private void CloseModal()
//        {
//            IsModalVisible = false;
//            StateHasChanged();
//        }
//        private async Task SaveAsync()
//        {

//            HandleButtonClicked = true;
//            var message = $"Are you sure you want to submit?";
//            var confirm = await MessageBox.Confirm(Modal, message);
//            if (!confirm)
//            {
//                HandleButtonClicked = false;
//                await InvokeAsync(StateHasChanged);
//                return;
//            }
//            var response = await Http.PostAsJsonAsync($"api/kpisubmit/create", HandleKPISubmitDto);
//            if (response.IsSuccessStatusCode)
//            {
//                await Message.Success("Created successfully.");
//            }
//            else
//            {
//                var error = await response.Content.ReadAsStringAsync();
//                await Message.Error($"Lỗi: {error}");
//            }
//            activeTabKey = "1";
//            TitleButton = "Next ⟶";
//            IsModalVisible = false;
//        }
//        private async Task SelectedChange()
//        {
//            if (KPIPeriodSubmit != null)
//            {
//                HandleActualDto = await LoadSelectedKPiSubmit(KPIPeriodSubmit.Year, KPIPeriodSubmit.Month);
//                if (HandleActualDto != null && HandleActualDto.Any())
//                {
//                    IsInputActual = true;
//                }
//            }

//            activeTabKey = "1";
//            TitleButton = "Next ⟶";
//        }
//        private async Task ActionTab()
//        {
//            HandleKPISubmitDto.KPITotalScore = new();
//            if (activeTabKey == "1")
//            {

//                activeTabKey = "2";
//                TitleButton = "⟵ Back";
//                foreach (var item in HandleActualDto)
//                {
//                    await CalculatedScoreMonth(item);
//                    item.HeaderTitle = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName}";

//                }
//                await AddCurrenTask(HandleKPISubmitDto);
//                var kpiScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.KPIScore) ?? 0;
//                var keyTaskScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.KeyTaskScore) ?? 0;
//                var omgScore = HandleKPISubmitDto.HandleActual?.Sum(x => x.OMGScore) ?? 0;

//                var totalScore = keyTaskScore + kpiScore + omgScore;

//                HeaderLine = $"{(KPIPeriodSubmit.MonthString)}  {CurrenUser.FullName}"
//                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
//                         + $"KPI Score:{ConvertNumberCommon.ConvertNumber(kpiScore)}"
//                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
//                         + $"KeyTaskScore:{ConvertNumberCommon.ConvertNumber(keyTaskScore)}"
//                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
//                         + $"OMGScore:{ConvertNumberCommon.ConvertNumber(omgScore)}"
//                         + "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"
//                         + $"Total Score:{ConvertNumberCommon.ConvertNumber(totalScore)}";
//                HandleKPISubmitDto.HandleActual = new();
//                HandleKPISubmitDto.HandleActual = HandleActualDto;
//                if (HandleKPISubmitDto.HandleActual != null)
//                {
//                    if (HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 3 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 6 ||
//                       HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 9 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 12)
//                    {
//                        await LoadAndCalculateQuarterAsync(HandleKPISubmitDto, HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month);
//                    }
//                    if (HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 6 || HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month == 12)
//                    {
//                        await CalculateHalfYearScoresFromQuarters(HandleKPISubmitDto, HandleKPISubmitDto.HandleActual?.FirstOrDefault()?.Month);
//                    }
//                }
//            }
//            else if (activeTabKey == "2")
//            {
//                activeTabKey = "1";
//                TitleButton = "Next ⟶";
//            }
//            await InvokeAsync(StateHasChanged);
//        }
//        private Task AddCurrenTask(HandleKPISubmitDto handleKPISubmit)
//        {
//            CurrenTotalScore = new KPITotalScoreDto();
//            decimal totalKPI = handleKPISubmit.HandleActual.Sum(x => x.KPIScore ?? 0);
//            decimal totalKeyTask = handleKPISubmit.HandleActual.Sum(x => x.KeyTaskScore ?? 0);
//            decimal omgScore = handleKPISubmit.HandleActual.Sum(x => x.OMGScore ?? 0);
//            decimal totalScore = totalKPI + totalKeyTask;
//            var listSetValue = handleKPISubmit.HandleActual.Where(x => x.Month != null);
//            CurrenTotalScore.Title = $"EMC - {KPIPeriodSubmit.MonthString} - {CurrenUser.FullName} ";
//            CurrenTotalScore.KPIScore = totalKPI;
//            CurrenTotalScore.KeyTaskScore = totalKeyTask;
//            CurrenTotalScore.TotaleScore = totalScore;
//            CurrenTotalScore.OMGScore = 0;
//            CurrenTotalScore.Month = handleKPISubmit.HandleActual.FirstOrDefault()?.Month;
//            CurrenTotalScore.Year = handleKPISubmit.HandleActual.FirstOrDefault()?.Year ?? 0;
//            CurrenTotalScore.CreatedBy = CurrenUser.Id;
//            return Task.CompletedTask;
//        }
//        private KPITotalScoreDto CurrenTotalScore = new();
//        #endregion
//        #region HandleScore
//        private List<int> GetMonthsInQuarter(int month)
//        {
//            if (month == 3) return new List<int> { 1, 2, 3 };
//            if (month == 6) return new List<int> { 4, 5, 6 };
//            if (month == 9) return new List<int> { 7, 8, 9 };
//            if (month == 12) return new List<int> { 10, 11, 12 };
//            return new List<int>();
//        }
//        #endregion
//        #region HandleMonthlyScore
//        private async Task CalculatedScoreMonth(HandleActualDto handleKPISubmit)
//        {
//            if (handleKPISubmit == null || !handleKPISubmit.ActualValue.HasValue || !handleKPISubmit.TargetValue.HasValue)
//                return;
//            if (handleKPISubmit.Month != null)
//            {
//                decimal actual = handleKPISubmit.ActualValue.Value;
//                decimal target = handleKPISubmit.TargetValue.Value;
//                decimal achievement = 0;
//                decimal totalKPI = 0;
//                decimal totalKeyTask = 0;
//                decimal totalOMG = 0;
//                switch (handleKPISubmit.KPIFormulaType)
//                {
//                    case KPIFormulaType.KF1:
//                        achievement = target == 0 ? 0 : actual / target;
//                        break;
//                    case KPIFormulaType.KF2:
//                        achievement = target == 0 ? 0 : 2 - (actual / target);
//                        break;
//                    case KPIFormulaType.KF3:
//                        if (target > 0)
//                        {
//                            achievement = actual / target;
//                        }
//                        else
//                        {
//                            achievement = 2 - (actual / target);
//                        }
//                        break;
//                    case KPIFormulaType.KF4:
//                        if (actual > target)
//                        {
//                            achievement = 0;
//                        }
//                        else
//                        {
//                            achievement = 1;
//                        }
//                        break;
//                    default:
//                        achievement = 0;
//                        break;
//                }

//                if (handleKPISubmit.Bottom.HasValue && achievement < handleKPISubmit.Bottom.Value)
//                {
//                    achievement = 0;
//                }
//                else if (handleKPISubmit.Max.HasValue && achievement > handleKPISubmit.Max.Value)
//                {
//                    achievement = handleKPISubmit.Max.Value;
//                }

//                handleKPISubmit.Achiement = Math.Round(achievement, 4, MidpointRounding.AwayFromZero);

//                if (handleKPISubmit.Weight.HasValue)
//                {
//                    handleKPISubmit.Score = Math.Round(achievement * handleKPISubmit.Weight.Value, 4, MidpointRounding.AwayFromZero);
//                }
//                else
//                {
//                    handleKPISubmit.Score = null;
//                }

//                //-------------------------
//                decimal scoreForType = handleKPISubmit.Score.Value * handleKPISubmit.IndexWeight;
//                switch (handleKPISubmit.KPIIndexType)
//                {
//                    case KPIIndexType.KPI:
//                        totalKPI += scoreForType;
//                        break;
//                    case KPIIndexType.KeyTask:
//                        totalKeyTask += scoreForType;
//                        break;
//                    case KPIIndexType.OMG:
//                        totalOMG += scoreForType;
//                        break;
//                }

//                switch (handleKPISubmit.KPIIndexType)
//                {
//                    case KPIIndexType.KPI:
//                        handleKPISubmit.KPIScore = totalKPI;
//                        break;
//                    case KPIIndexType.KeyTask:
//                        handleKPISubmit.KeyTaskScore = totalKeyTask;
//                        break;
//                    case KPIIndexType.OMG:
//                        handleKPISubmit.OMGScore = totalOMG;
//                        break;
//                }
//            }

//            await Task.CompletedTask;
//        }
//        #endregion
//        #region HandleQuarterlyScore
//        private async Task Calculated(HandleKPISubmitDto handleKPISubmit, int month)
//        {
//            var listDetail = new List<HandleActualDto>();
//            int year = handleKPISubmit.HandleActual.FirstOrDefault()?.Year ?? DateTime.Now.Year;
//            //-------------DetailScore-------------------------
//            int quarter = (month + 2) / 3;
//            foreach (var taskGroup in handleKPISubmit.HandleActual.GroupBy(x => x.TargetId))
//            {
//                decimal? actual = null;
//                decimal? target = null;
//                var first = taskGroup.First();
//                switch (first.QuarterCalculateType)
//                {
//                    case QuarterCalculateType.CAL1:
//                        actual = taskGroup.Sum(x => x.ActualValue ?? 0);
//                        target = taskGroup.Sum(x => x.TargetValue ?? 0);
//                        break;

//                    case QuarterCalculateType.CAL2:
//                        actual = taskGroup
//                            .Where(x => x.ActualValue.HasValue)
//                            .Average(x => x.ActualValue);

//                        target = taskGroup
//                            .Where(x => x.TargetValue.HasValue)
//                            .Average(x => x.TargetValue);
//                        break;

//                    case QuarterCalculateType.CAL3:
//                        var last = taskGroup
//                            .OrderByDescending(x => x.Month)
//                            .FirstOrDefault();

//                        actual = last?.ActualValue;
//                        target = last?.TargetValue;
//                        break;
//                }
//                decimal kpiScore = 0;
//                decimal omgscore = 0;
//                decimal keytaskscore = 0;

//                var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList() ?? new List<IndexWeightDto>();

//                decimal omgWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
//                decimal kpiWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
//                decimal keyTaskWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

//                if (handleKPISubmit.HandleActual != null && handleKPISubmit.HandleActual.Any())
//                {
//                    // OMG
//                    var listOMGDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.OMG).ToList();
//                    decimal sumOMG = listOMGDetails.Sum(x => x.Score ?? 0);
//                    var avgOMG = listOMGDetails.Count > 0 ? sumOMG / listOMGDetails.Count : 0;
//                    omgscore = avgOMG * omgWeight;

//                    // KPI
//                    var listKPIDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.KPI).ToList();
//                    decimal sumKPI = listKPIDetails.Sum(x => x.Score ?? 0);
//                    var avgKPI = listKPIDetails.Count > 0 ? sumKPI / listKPIDetails.Count : 0;
//                    kpiScore = avgKPI * kpiWeight;

//                    // KeyTask
//                    var listKeyTaskDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).ToList();
//                    decimal sumKeyTask = listKeyTaskDetails.Sum(x => x.Score ?? 0);
//                    var avgKeyTask = listKeyTaskDetails.Count > 0 ? sumKeyTask / listKeyTaskDetails.Count : 0;
//                    keytaskscore = avgKeyTask * keyTaskWeight;
//                    handleKPISubmit.HandleActual.AddRange(listKPIDetails);

//                }

//                decimal totalScore = kpiScore + keytaskscore + omgscore;
//                decimal? achivement = target > 0 ? actual / target : null;
//                var quarterScore = new KPITotalScoreDto
//                {
//                    KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
//                    KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
//                    OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
//                    TotaleScore = ConvertNumberCommon.ConvertNumber(totalScore),
//                    Title = $"EMC - {handleKPISubmit.HandleActual.First().Year} - Q{quarter} - {CurrenUser.FullName} ",
//                    Year = handleKPISubmit.HandleActual.First().Year,
//                    Month = null,
//                    Quarter = quarter,
//                    CreatedBy = CurrenUser.Id
//                };
//                handleKPISubmit.KPITotalScore.Add(quarterScore);

//                listDetail.Add(new HandleActualDto
//                {
//                    TaskId = first.TaskId,
//                    Year = year,
//                    Quarter = quarter,
//                    Month = null,

//                    TaskName = first.TaskName,
//                    KPIIndexType = first.KPIIndexType,
//                    KPICategory = first.KPICategory,

//                    QuarterCalculateType = first.QuarterCalculateType,
//                    KPIFormulaType = first.KPIFormulaType,

//                    Max = first.Max,
//                    Bottom = first.Bottom,
//                    Weight = first.Weight,
//                    IndexWeight = first.IndexWeight,

//                    ActualValue = actual,
//                    TargetValue = target,
//                    Achiement = achivement,

//                    Score = achivement.HasValue && first.Weight.HasValue
//                        ? Math.Round(
//                            achivement.Value * first.Weight.Value,
//                            4,
//                            MidpointRounding.AwayFromZero
//                          )
//                        : null
//                });


//            }
//            //-------------TotalScore--------------------------


//        }
//        private async Task LoadAndCalculateQuarterAsync(
//        HandleKPISubmitDto handleKPISubmit,
//        int? month
//    )
//        {
//            if (!month.HasValue || handleKPISubmit == null || !handleKPISubmit.HandleActual.Any())
//                return;
//            if (handleKPISubmit == null)
//                return;
//            var listMonths = GetMonthsInQuarter(month.Value);
//            int year = handleKPISubmit.HandleActual.FirstOrDefault()?.Year ?? DateTime.Now.Year;
//            int quarter = (month.Value + 2) / 3;
//            foreach (var m in listMonths)
//            {
//                var monthTotals = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
//                    $"api/kpiSubmit/get-result-kpi/{CurrenUser.Id}/{year}/{m}");

//                if (monthTotals != null && monthTotals.Any())
//                    handleKPISubmit.KPITotalScore.AddRange(monthTotals);

//                var monthDetails = await Http.GetFromJsonAsync<List<HandleActualDto>>(
//                    $"api/kpiSubmit/get-result-omg/{CurrenUser.Id}/{year}/{m}");

//                if (monthDetails != null && monthDetails.Any())
//                    handleKPISubmit.HandleActual.AddRange(monthDetails);
//            }
//            await Calculated(handleKPISubmit,month.Value);
//            var a = handleKPISubmit;

//        }

//        #endregion
//        #region HandleHalfYearlyScore
//        private async Task CalculateHalfYearScoresFromQuarters(HandleKPISubmitDto handleKPISubmit, int? month)
//        {
//            if (handleKPISubmit == null || !handleKPISubmit.KPITotalScore.Any())
//                return;
//            int halfYear = (month.Value <= 6) ? 1 : 2;
//            var quarterNumbers = month == 6 ? 1 : 3;
//            var quarterScores = new List<KPITotalScoreDto>();
//            var result = await Http.GetFromJsonAsync<KPITotalScoreDto>(
//                $"api/kpisubmit/get-result-quarter/{CurrenUser.Id}/{handleKPISubmit.HandleActual.First().Year}/{quarterNumbers}");
//            if (result != null)
//            {
//                quarterScores.Add(result);
//            }
//            if (handleKPISubmit.KPITotalScore.Any())
//            {
//                quarterScores.Add(handleKPISubmit.KPITotalScore.First(x => x.Quarter == (quarterNumbers + 1)));
//            }
//            if (quarterScores.Count != 2)
//                return;

//            // Trung bình cộng
//            decimal avgKPI = quarterScores.Average(x => x.KPIScore);
//            decimal avgKeyTask = quarterScores.Average(x => x.KeyTaskScore);
//            decimal avgOMG = quarterScores.Average(x => x.OMGScore);
//            decimal avgTotal = quarterScores.Average(x => x.TotaleScore);
//            //bool alreadyExists = handleKPISubmit.KPITotalScore.Any(x => x.HalfYear == halfYear && x.Month == null && x.Quarter == null);
//            //if (!alreadyExists)
//            //{
//                var halfYearScore = new KPITotalScoreDto
//                {
//                    KPIScore = Math.Round(avgKPI, 2),
//                    KeyTaskScore = Math.Round(avgKeyTask, 2),
//                    OMGScore = Math.Round(avgOMG, 2),
//                    TotaleScore = Math.Round(avgTotal, 2),
//                    Title = $"EMC - {handleKPISubmit.HandleActual.First().Year} - H{halfYear} - {CurrenUser.FullName} ",
//                    Year = handleKPISubmit.HandleActual.First().Year,
//                    HalfYear = halfYear,
//                    CreatedBy = CurrenUser.Id
//                };

//                handleKPISubmit.KPITotalScore.Add(halfYearScore);
//            //}
//        }
//        #endregion

//    }
//}
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.DealineManagement;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.KPISubmit
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

        #endregion

        #region INIT

        public async Task ShowModal(UserDto currenUser)
        {
            CurrenUser = currenUser;
            HandleKPISubmitDto = new HandleKPISubmitDto();
            HandleActualDto = new List<HandleActualDto>();
            DealineManagement = new List<DealineManagementDto>();
            IndexWeight = new List<IndexWeightDto>();

            IsModalVisible = true;
            IsInputActual = false;
            activeTabKey = "1";
            TitleButton = "Next ⟶";

            await GetDeadline();
            await GetIndexWeight();
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
            HandleActualDto = data.OrderByDescending(x => x.Index).ToList();
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

        private async Task ActionTab()
        {
            if (activeTabKey == "1")
            {
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

                if (month == 3 || month == 6 || month == 9 || month == 12)
                {
                    await LoadAndCalculateQuarterAsync(HandleKPISubmitDto, month);
                }

                if (month == 6 || month == 12)
                {
                    await CalculateHalfYearScoresFromQuarters(HandleKPISubmitDto, month);
                }
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


            var confirm = await MessageBox.Confirm(Modal, "Are you sure you want to submit?");
            if (!confirm) return;

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
                achievement = item.Max.HasValue ? item.Max.Value*100 :0;
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

        private async Task LoadAndCalculateQuarterAsync(
            HandleKPISubmitDto handleKPISubmit,
            int? month)
        {
            if (!month.HasValue) return;

            var months = GetMonthsInQuarter(month.Value);
            int year = handleKPISubmit.HandleActual.First().Year;

            foreach (var m in months)
            {
                var totals = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                    $"api/kpiSubmit/get-result-kpi/{CurrenUser.Id}/{year}/{m}");
                if (totals != null) handleKPISubmit.KPITotalScore.AddRange(totals);

                var details = await Http.GetFromJsonAsync<List<HandleActualDto>>(
                    $"api/kpiSubmit/get-result-detail/{CurrenUser.Id}/{year}/{m}");
                if (details != null) handleKPISubmit.HandleActual.AddRange(details);
            }

            await CalculatedQuarter(handleKPISubmit, month.Value);
        }

        private async Task CalculatedQuarter(HandleKPISubmitDto handleKPISubmit, int month)
        {
            int quarter = (month + 2) / 3;
            int year = handleKPISubmit.HandleActual.First().Year;
            var listTotalQuarter = new List<KPITotalScoreDto>();

            foreach (var group in handleKPISubmit.HandleActual.GroupBy(x => x.TaskId))
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
                        var last = group.OrderByDescending(x => x.Month).First();
                        actual = last.ActualValue;
                        target = last.TargetValue;
                        break;
                }
                decimal achivement = target > 0 ? actual.Value / target.Value : 0;
                handleKPISubmit.HandleActual.Add(new HandleActualDto
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
                        ? Math.Round(
                            achivement * first.Weight.Value,
                            4,
                            MidpointRounding.AwayFromZero
                          )
                        : null
                });
                decimal kpiScore = 0;
                decimal omgscore = 0;
                decimal keytaskscore = 0;

                var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList() ?? new List<IndexWeightDto>();

                decimal omgWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
                decimal kpiWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
                decimal keyTaskWeight = indexWeights?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

                if (handleKPISubmit.HandleActual != null && handleKPISubmit.HandleActual.Any())
                {
                    // OMG
                    var listOMGDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.OMG).ToList();
                    decimal sumOMG = listOMGDetails.Sum(x => x.Score ?? 0);
                    var avgOMG = listOMGDetails.Count > 0 ? sumOMG / listOMGDetails.Count : 0;
                    omgscore = avgOMG * omgWeight;

                    // KPI
                    var listKPIDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.KPI).ToList();
                    decimal sumKPI = listKPIDetails.Sum(x => x.Score ?? 0);
                    var avgKPI = listKPIDetails.Count > 0 ? sumKPI / listKPIDetails.Count : 0;
                    kpiScore = avgKPI * kpiWeight;

                    // KeyTask
                    var listKeyTaskDetails = handleKPISubmit.HandleActual.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).ToList();
                    decimal sumKeyTask = listKeyTaskDetails.Sum(x => x.Score ?? 0);
                    var avgKeyTask = listKeyTaskDetails.Count > 0 ? sumKeyTask / listKeyTaskDetails.Count : 0;
                    keytaskscore = avgKeyTask * keyTaskWeight;

                }

                listTotalQuarter.Add(new KPITotalScoreDto
                {
                    Title = $"EMC - {year} - Q{quarter} - {CurrenUser.FullName}",
                    Year = year,
                    Quarter = quarter,
                    KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                    KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                    OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                    TotaleScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore),
                    CreatedBy = CurrenUser.Id
                });
            }
            if (listTotalQuarter.Any())
            {
                handleKPISubmit.KPITotalScore.Add(listTotalQuarter.First());
            }
            await Task.CompletedTask;
        }

        #endregion

        #region HALF YEAR

        private async Task CalculateHalfYearScoresFromQuarters(
            HandleKPISubmitDto handleKPISubmit,
            int? month)
        {
            if (!month.HasValue) return;

            int halfYear = month <= 6 ? 1 : 2;
            var quarters = handleKPISubmit.KPITotalScore
                .Where(x => x.Quarter != null)
                .Take(2)
                .ToList();

            if (quarters.Count != 2) return;

            handleKPISubmit.KPITotalScore.Add(new KPITotalScoreDto
            {
                Title = $"EMC - {handleKPISubmit.HandleActual.First().Year} - H{halfYear} - {CurrenUser.FullName}",
                Year = handleKPISubmit.HandleActual.First().Year,
                HalfYear = halfYear,
                KPIScore = quarters.Average(x => x.KPIScore),
                KeyTaskScore = quarters.Average(x => x.KeyTaskScore),
                OMGScore = quarters.Average(x => x.OMGScore),
                TotaleScore = quarters.Average(x => x.TotaleScore),
                CreatedBy = CurrenUser.Id
            });

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
                Month = KPIPeriodSubmit.Month,
                Year = KPIPeriodSubmit.Year,
                CreatedBy = CurrenUser.Id
            };

            handleKPISubmit.KPITotalScore.Add(CurrenTotalScore);
            await Task.CompletedTask;
        }

        #endregion
    }
}
