using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Client.Modals.KPI.Target;
using AquaSolution.Client.Modals.KPI.UserTask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.CeilingLevel;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.UserTask
{
    public partial class UserTask
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<UserDto> users = new();
        private List<UserDto> userFilter = new();
        private UserTaskModal _userTaskModal = new();
        private TargetModal targetModal = new();
        private int Month { get; set; }
        private int Year { get; set; }
        private Guid PageId { get; set; }
        private List<CalculateQuarterPointDto> CalculateQuarterPoint = new();
        private UserDto? CurrenUser { get; set; }
        public bool Loading { get; set; }
        private bool CalculateQuarterPointsPermission { get; set; } = false;
        private bool TaskManagement { get; set; } = false;
        private bool IsLock { get; set; }
        private HubConnection? _hubConnection;
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadCurrenUser();
            await GetPage();
            await CheckPermission();
            Month = DateTime.Now.Month;
            Year = DateTime.Now.Year;
            await CheckLock();
            await LoadData();
            await LoadDataFilterAsync();
            await ReloadIsLock();
        }

        private async Task ReloadIsLock()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                .Build();
            _hubConnection.On<Guid>("IsLockSystem", async pageId =>
            {
                if (pageId == PageId)
                {
                    await CheckLock();
                    await InvokeAsync(StateHasChanged);
                }
            });
            await _hubConnection.StartAsync();
        }

        private async Task GetPage()
        {
            var url = "task-user-management";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }

        private async Task CheckLock()
        {
            if (CurrenUser != null && CurrenUser.Roles.Any(x => x.Name == "Admin"))
            {
                IsLock = false;
                return;
            }
            IsLock = await Http.GetFromJsonAsync<bool>($"api/systemLock/check-lock/{PageId}");
        }

        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }

        private async Task CheckPermission()
        {
            CalculateQuarterPointsPermission = await permissionService.HasPermissionAsync(PageId, PermissionActionType.CalculateQuarterPoints);
            TaskManagement = await permissionService.HasPermissionAsync(PageId, PermissionActionType.TaskManagement);
        }

        private async Task LoadData()
        {
            try
            {
                Loading = true;
                StateHasChanged();
                var data = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                var result = data.Where(x => x.IsActive == true && x.PositionId != null).ToList();

                foreach (var user in result)
                {
                    user.FullName ??= string.Empty;
                    user.WorkDayId ??= string.Empty;
                    user.DepartmentName ??= string.Empty;
                    user.FactoryName ??= string.Empty;
                    user.PositionName ??= string.Empty;
                }
                var filtered = new List<UserDto>();

                if (result is not null)
                {
                    if (CurrenUser.Roles.Any(x => x.Name == "Admin") || CurrenUser.Roles.Any(x => x.Name == "HR"))
                    {
                        filtered = result;
                    }
                    else if (CurrenUser.Roles.Any(x => x.Name == "KPIUSER_DepartmentViewer"))
                    {
                        filtered = result
                            .Where(x => x.DepartmentId == CurrenUser.DepartmentId)
                            .ToList();
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
                    users = filtered;
                }
                userFilter = users;
                await Search();
                Loading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        #endregion

        #region Actions
        private async Task ViewTarget(UserDto user)
        {
            await targetModal.ShowModal(user);
        }

        private async Task EditTask(UserDto user)
        {
            await _userTaskModal.ShowModal(user);
        }

        private HandleKPISubmitDto _handleKPISubmitForCalculate = new();

        private async Task CalculateQuarterPoints(UserDto user)
        {
            var confirm = await MessageBox.Confirm(modal, "Are you sure you want calculate Quarter score this user?");
            if (!confirm) return;

            await GetIndexWeight(user.PositionType);
            await GetCeilingLevel(user);

            if (Month == 4 || Month == 7 || Month == 10 || Month == 1)
            {
                // ✅ Reset mỗi lần tính
                _handleKPISubmitForCalculate = new HandleKPISubmitDto();

                await CalculateQuarterScores(Month, user);

                if (Month == 7 || Month == 1)
                {
                    await CalculateHalfYearScoresFromQuarters(Month, user);
                }

                var response = await Http.PostAsJsonAsync(
                    "api/kpisubmit/calculate-point-quarter",
                    _handleKPISubmitForCalculate); // ✅ Gửi HandleKPISubmitDto

                if (response.IsSuccessStatusCode)
                    await Message.Success("Calculate point quarter successfully.");
                else
                    await Message.Error($"Lỗi: Calculate point quarter failed.");
            }
            else
            {
                await MessageBox.Warning(modal, "Quarterly points cannot be calculated.");
            }
        }
        #endregion

        #region Search
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

        private async Task Search()
        {
            try
            {
                var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                var fullName = StringHelper.NormalizeText(FullName?.Trim());

                var filtered = users
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                        (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.FullName) && StringHelper.NormalizeText(x.FullName).Contains(fullName)))
                    )
                    .ToList();

                if (string.IsNullOrWhiteSpace(workDayId) && string.IsNullOrWhiteSpace(fullName))
                {
                    filtered = users;
                }

                userFilter = filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }
        }

        private async Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            userFilter = users;
            tableRef?.ReloadData();
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }

        private Table<UserDto> tableRef;
        private List<DepartmentDto> ListDepartment = new();
        private List<FactoryDto> ListFactory = new();
        private List<PositionDto> ListPosition = new();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _positionFilter = Array.Empty<TableFilter<string>>();

        private async Task LoadDataFilterAsync()
        {
            ListDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ?? new List<DepartmentDto>();
            _departmentFilter = ListDepartment
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            ListFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ?? new List<FactoryDto>();
            _factoryFilter = ListFactory
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            ListPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all") ?? new List<PositionDto>();
            _positionFilter = ListPosition
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();

            foreach (var user in users)
            {
                user.FactoryName ??= string.Empty;
                user.DepartmentName ??= string.Empty;
                user.PositionName ??= string.Empty;
            }
        }
        #endregion
        #region CalculateQuarterPoints

        private List<IndexWeightDto> IndexWeight = new();
        private CeilingLevelDto CeilingLevel = new();
        private List<HandleActualDto> _cachedQuarterHandleActual = new();

        private async Task GetIndexWeight(PositionType? positionType)
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>(
                    $"api/kpisubmit/get-indexweight/{positionType}/{PeriodType.Quarter}");
                if (result != null && result.Any())
                {
                    IndexWeight = result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] API Call Failed: {ex.Message}");
            }
        }

        private async Task GetCeilingLevel(UserDto user)
        {
            CeilingLevel = await Http.GetFromJsonAsync<CeilingLevelDto>(
                $"api/ceilinglevel/ceilingLevel-by-userId/{user.Id}")
                ?? new CeilingLevelDto();
        }

        private List<int> GetMonthsInQuarter(int month)
        {
            // Admin dùng month = tháng đầu quý tiếp theo (4,7,10,1)
            if (month == 4) return new List<int> { 1, 2, 3 };
            if (month == 7) return new List<int> { 4, 5, 6 };
            if (month == 10) return new List<int> { 7, 8, 9 };
            if (month == 1) return new List<int> { 10, 11, 12 };
            return new List<int>();
        }

        private async Task CalculateQuarterScores(int month, UserDto user)
        {
            try
            {
                _cachedQuarterHandleActual = new List<HandleActualDto>();

                var listMonths = GetMonthsInQuarter(month);
                var tempSubmit = new HandleKPISubmitDto();

                foreach (var m in listMonths)
                {
                    var totals = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                        $"api/kpiSubmit/get-result-total-score-by-month/{user.Id}/{Year}/{m}");
                    if (totals != null) tempSubmit.KPITotalScore.AddRange(totals);

                    var details = await Http.GetFromJsonAsync<List<HandleActualDto>>(
                        $"api/kpiSubmit/get-result-detail-by-momth/{user.Id}/{Year}/{m}");
                    if (details != null) tempSubmit.HandleActual.AddRange(details);
                }

                if (tempSubmit.KPITotalScore.Count < 2) return;

                int quarter = month switch
                {
                    4 => 1,
                    7 => 2,
                    10 => 3,
                    1 => 4,
                    _ => 0
                };

                await CalculatedQuarter(tempSubmit, user, quarter, Year);

                // ✅ Cache detail quarter để HalfYear dùng
                _cachedQuarterHandleActual = tempSubmit.HandleActual
                    .Where(x => x.Quarter == quarter && x.Month == null)
                    .ToList();

                // ✅ Gán vào _handleKPISubmitForCalculate
                var quarterTotal = tempSubmit.KPITotalScore
                    .FirstOrDefault(x => x.Quarter == quarter && x.Month == null);

                if (quarterTotal != null)
                {
                    _handleKPISubmitForCalculate.KPITotalScore.Add(quarterTotal);
                    _handleKPISubmitForCalculate.HandleActual.AddRange(_cachedQuarterHandleActual);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] CalculateQuarterScores Failed: {ex.Message}");
            }
        }

        private async Task CalculatedQuarter(HandleKPISubmitDto handleKPISubmit, UserDto user, int quarter, int year)
        {
            var newHandleActualList = new List<HandleActualDto>();

            // 🔹 STEP 1: BUILD DATA - group by TaskId, tính theo CAL1/CAL2/CAL3
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

                // ✅ Dùng HelperCalculated giống file 3
                decimal achivement = await HelperCalculated(
                    first.KPIFormulaType, actual ?? 0, target ?? 0, first.Max ?? 0);

                if (first.Bottom.HasValue && achivement < first.Bottom) achivement = 0;
                if (first.Max.HasValue && achivement > first.Max) achivement = first.Max.Value;

                newHandleActualList.Add(new HandleActualDto
                {
                    TaskId = first.TaskId,
                    CreatedBy = user.Id,
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
                });
            }

            // 🔹 STEP 2: ADD DATA sau khi build xong
            handleKPISubmit.HandleActual.AddRange(newHandleActualList);

            var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList()
                                ?? new List<IndexWeightDto>();
            decimal omgWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
            decimal kpiWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
            decimal keyTaskWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

            // 🔹 STEP 3: CALCULATE SCORE từ newHandleActualList vừa build
            //            ✅ Giống file 3: filter theo Quarter == quarter
            decimal sumOMG = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.OMG).Sum(x => x.Score ?? 0);
            decimal sumKPI = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.KPI).Sum(x => x.Score ?? 0);
            decimal sumKeyTask = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).Sum(x => x.Score ?? 0);

            decimal omgscore = sumOMG * omgWeight;
            decimal kpiScore = sumKPI * kpiWeight;
            decimal keytaskscore = sumKeyTask * keyTaskWeight;

            decimal totalActualScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore);
            decimal totalScore = totalActualScore;

            if (totalScore > CeilingLevel.CeilingLevelValue && CeilingLevel.CeilingLevelValue > 0)
                totalScore = CeilingLevel.CeilingLevelValue;

            handleKPISubmit.KPITotalScore.Add(new KPITotalScoreDto
            {
                Title = $"EMC - {year} - Q{quarter} - {user.FullName}",
                Year = year,
                Quarter = quarter,
                KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                TotaleScore = totalScore,
                TotalActualScore = totalActualScore,
                CreatedBy = user.Id
            });

            await Task.CompletedTask;
        }

        //private async Task CalculateHalfYearScoresFromQuarters(int? month, UserDto user)
        //{
        //    if (!month.HasValue) return;

        //    int halfYear = month == 6 ? 1 : 2;
        //    int prevQuarter = month == 6 ? 1 : 3;

        //    var tempSubmit = new HandleKPISubmitDto();

        //    // ✅ Load quarter trước từ DB
        //    var totalQuarterFromDb = await Http.GetFromJsonAsync<KPITotalScoreDto>(
        //        $"api/kpiSubmit/get-result-total-score-by-quarter/{user.Id}/{Year}/{prevQuarter}");
        //    if (totalQuarterFromDb != null)
        //        tempSubmit.KPITotalScore.Add(totalQuarterFromDb);

        //    var detailsFromDb = await Http.GetFromJsonAsync<List<HandleActualDto>>(
        //        $"api/kpiSubmit/get-result-detail-by-quarter/{user.Id}/{Year}/{prevQuarter}");
        //    if (detailsFromDb != null)
        //        tempSubmit.HandleActual.AddRange(detailsFromDb);

        //    // ✅ Thêm detail quarter vừa tính từ cache
        //    tempSubmit.HandleActual.AddRange(_cachedQuarterHandleActual);

        //    if (tempSubmit.KPITotalScore.Count < 1) return;

        //    await CalculateHalfYear(tempSubmit, user, halfYear, Year);

        //    // ✅ Gán vào _handleKPISubmitForCalculate
        //    var halfYearTotal = tempSubmit.KPITotalScore
        //        .FirstOrDefault(x => x.HalfYear == halfYear && x.Month == null);

        //    var halfYearHandleActual = tempSubmit.HandleActual
        //        .Where(x => x.HalfYear == halfYear && x.Month == null)
        //        .ToList();

        //    if (halfYearTotal != null)
        //    {
        //        _handleKPISubmitForCalculate.KPITotalScore.Add(halfYearTotal);
        //        _handleKPISubmitForCalculate.HandleActual.AddRange(halfYearHandleActual);
        //    }
        //}
        private async Task CalculateHalfYearScoresFromQuarters(int? month, UserDto user)
        {

            int halfYear = month == 6 ? 1 : 2;
            int prevQuarter = month == 6 ? 1 : 3;

            var tempSubmit = new HandleKPISubmitDto();

            // ✅ Load quarter trước từ DB
            var totalQuarterFromDb = await SafeGetFromJsonAsync<KPITotalScoreDto>(
                    $"api/kpiSubmit/get-result-total-score-by-quarter/{user.Id}/{Year}/{prevQuarter}");
            if (totalQuarterFromDb != null) tempSubmit.KPITotalScore.Add(totalQuarterFromDb);

            var detailsFromDb = await SafeGetFromJsonAsync<List<HandleActualDto>>(
                    $"api/kpiSubmit/get-result-detail-quarter/{user.Id}/{Year}/{prevQuarter}");
                if (detailsFromDb != null)
                    tempSubmit.HandleActual.AddRange(detailsFromDb);

            // ✅ Thêm detail quarter vừa tính từ cache
                tempSubmit.HandleActual.AddRange(_cachedQuarterHandleActual);

            if (tempSubmit.KPITotalScore.Count < 1) return;

                await CalculateHalfYear(tempSubmit, user, halfYear, Year);

            // ✅ Gán vào _handleKPISubmitForCalculate
                var halfYearTotal = tempSubmit.KPITotalScore
                    .FirstOrDefault(x => x.HalfYear == halfYear && x.Month == null);

                var halfYearHandleActual = tempSubmit.HandleActual
                    .Where(x => x.HalfYear == halfYear && x.Month == null)
                    .ToList();

                if (halfYearTotal != null)
                {
                    _handleKPISubmitForCalculate.KPITotalScore.Add(halfYearTotal);
                    _handleKPISubmitForCalculate.HandleActual.AddRange(halfYearHandleActual);
            }
        }
        private async Task CalculateHalfYear(
            HandleKPISubmitDto handleKPISubmit, UserDto user, int halfYear, int year)
        {
            var newHandleActualList = new List<HandleActualDto>();

            // 🔹 STEP 1: BUILD DATA
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

                // ✅ Dùng HelperCalculated giống file 3 (Quarter cũng dùng KF1/KF2/KF3/KF4)
                decimal achivement = await HelperCalculated(
                    first.KPIFormulaType, actual ?? 0, target ?? 0, first.Max ?? 0);

                if (first.Bottom.HasValue && achivement < first.Bottom) achivement = 0;
                if (first.Max.HasValue && achivement > first.Max) achivement = first.Max.Value;

                newHandleActualList.Add(new HandleActualDto
                {
                    TaskId = first.TaskId,
                    Year = year,
                    Quarter = null,
                    Month = null,
                    HalfYear = halfYear,
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
                });
            }

            // 🔹 STEP 2: ADD
            handleKPISubmit.HandleActual.AddRange(newHandleActualList);

            var indexWeights = IndexWeight?.Where(x => x.PeriodType == PeriodType.Quarter).ToList()
                                ?? new List<IndexWeightDto>();
            decimal omgWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.Weight ?? 0;
            decimal kpiWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.Weight ?? 0;
            decimal keyTaskWeight = indexWeights.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.Weight ?? 0;

            // 🔹 STEP 3: CALCULATE từ newHandleActualList vừa build
            decimal sumOMG = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.OMG).Sum(x => x.Score ?? 0);
            decimal sumKPI = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.KPI).Sum(x => x.Score ?? 0);
            decimal sumKeyTask = newHandleActualList.Where(x => x.KPIIndexType == KPIIndexType.KeyTask).Sum(x => x.Score ?? 0);

            decimal omgscore = sumOMG * omgWeight;
            decimal kpiScore = sumKPI * kpiWeight;
            decimal keytaskscore = sumKeyTask * keyTaskWeight;
            decimal totalScore = ConvertNumberCommon.ConvertNumber(kpiScore + keytaskscore + omgscore);

            if (totalScore > CeilingLevel.CeilingLevelValue && CeilingLevel.CeilingLevelValue > 0)
                totalScore = CeilingLevel.CeilingLevelValue;

            handleKPISubmit.KPITotalScore.Add(new KPITotalScoreDto
            {
                Title = $"EMC - {year} - H{halfYear} - {user.FullName}",
                Year = year,
                HalfYear = halfYear,   // ✅ Gán HalfYear thay vì Quarter
                KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                TotaleScore = totalScore,
                CreatedBy = user.Id
            });

            await Task.CompletedTask;
        }

        private async Task<decimal> HelperCalculated(
            KPIFormulaType kPIFormulaType, decimal actual, decimal target, decimal max)
        {
            decimal achievement = 0m;

            if (kPIFormulaType == KPIFormulaType.KF1)
                achievement = target == 0
                    ? (actual >= 0 ? max * 100 : 0)
                    : actual / target;

            else if (kPIFormulaType == KPIFormulaType.KF2)
                achievement = target == 0
                    ? (actual > 0 ? 0 : max * 100)
                    : 2 - (actual / target);

            else if (kPIFormulaType == KPIFormulaType.KF3)
                achievement = target == 0
                    ? (actual > 0 ? max * 100 : 0)
                    : (target > 0 ? actual / target : 2 - (actual / target));

            else if (kPIFormulaType == KPIFormulaType.KF4)
                achievement = actual > target ? 0 : 1;

            return achievement;
        }

        #endregion
        #region Helper
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