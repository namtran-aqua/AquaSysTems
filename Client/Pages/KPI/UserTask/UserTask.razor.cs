using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Client.Components.KPI.Target;
using AquaSolution.Client.Components.KPI.UserTask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using NPOI.OpenXmlFormats.Spreadsheet;
using System;
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
        private bool IsLock { get;set;  }
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
                var data = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                var result = data.Where(x=>x.IsActive == true && x.PositionId !=null).ToList();

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
                    if(CurrenUser.Roles.Any(x => x.Name == "Admin") || CurrenUser.Roles.Any(x => x.Name == "HR"))
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
        private async Task CalculateQuarterPoints(UserDto user)
        {
            CalculateQuarterPoint = new List<CalculateQuarterPointDto>();
            var Confirm = await MessageBox.Confirm(modal, "Are you sure you want calculate Quarter score this user?");
            if (!Confirm) return;
            await GetIndexWeight(user.PositionType);
            if (Month == 4 || Month == 7 || Month == 10 || Month == 1)
            {
                await CalculateQuarterScores(Month, user);
                if (Month == 7 || Month == 1)
                {
                    await CalculateHalfYearScoresFromQuarters(CalculateQuarterPoint, Month, user);
                }
                var response = await Http.PostAsJsonAsync($"api/kpisubmit/calculate-point-quarter", CalculateQuarterPoint);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("calculate point quarter successfully.");
                }
                else
                {
                    await Message.Error($"Lỗi: calculate point quarter faile");
                }
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

                if (string.IsNullOrWhiteSpace(workDayId) &&
                    string.IsNullOrWhiteSpace(fullName))
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
                .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
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
        private async Task GetIndexWeight(PositionType? positionType)
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<IndexWeightDto>>($"api/kpisubmit/get-indexweight/{positionType}/{PeriodType.Quarter}");
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
        private List<int> GetMonthsInQuarter(int month)
        {
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
                var listMonths = GetMonthsInQuarter(month);
                var actuals = new List<KPITotalScoreDto>();
                var listResultDetail = new List<HandleActualDto>();
                foreach (var m in listMonths)
                {  // lấy Total để validate đủ 2 tahngs trở lên không
                    var result = await Http.GetFromJsonAsync<List<KPITotalScoreDto>>(
                            $"api/kpiSubmit/get-result-kpi/{user.Id}/{Year}/{m}");

                    if (result != null && result.Any())
                    {
                        actuals.AddRange(result);
                    }
                    // lấy Total để validate đủ 2 tahngs trở lên không
                    var result2 = await Http.GetFromJsonAsync<List<HandleActualDto>>(
                          $"api/kpiSubmit/get-result-omg/{user.Id}/{Year}/{m}");
                    if (result2 != null && result2.Any())
                    {
                        listResultDetail.AddRange(result2);
                    }
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

                int quarter = ((month + 8) % 12) / 3 + 1;

                // Kiểm tra đã có chưa
                bool alreadyExists = CalculateQuarterPoint.Any(x => x.Quarter == quarter && x.Month == null && x.HalfYear == null);
                if (!alreadyExists)
                {
                    var quarterScore = new CalculateQuarterPointDto
                    {
                        KPIScore = ConvertNumberCommon.ConvertNumber(kpiScore),
                        KeyTaskScore = ConvertNumberCommon.ConvertNumber(keytaskscore),
                        OMGScore = ConvertNumberCommon.ConvertNumber(omgscore),
                        TotaleScore = ConvertNumberCommon.ConvertNumber(totalScore),
                        Title = $"EMC - {Year} - Q{quarter} - {user.FullName} ",
                        Year = Year,
                        Month = null,
                        Quarter = quarter,
                        CreatedBy = user.Id,
                        ApprovedBy =CurrenUser.Id,
                    };

                    CalculateQuarterPoint.Add(quarterScore);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] CalculateQuarterScores Failed: {ex.Message}");
            }
        }
        private async Task CalculateHalfYearScoresFromQuarters(List<CalculateQuarterPointDto>  calculateQuarterPoint, int? month, UserDto user)
        {
            if (calculateQuarterPoint == null)
                return;
            int halfYear = (month.Value <= 7) ? 1 : 2;
            var quarterNumbers = month == 7 ? 1 : 3;
            var quarterScores = new List<KPITotalScoreDto>();

            var result = await Http.GetFromJsonAsync<KPITotalScoreDto>(
                $"api/kpisubmit/get-result-quarter/{user.Id}/{Year}/{quarterNumbers}");
            if (result != null)
            {
                quarterScores.Add(result);
            }
            if (calculateQuarterPoint.Any())
            {
                var currenQuarter = calculateQuarterPoint.First(x => x.Quarter == (quarterNumbers + 1));
                quarterScores.Add(new KPITotalScoreDto
                {
                    Title = currenQuarter.Title,
                    KPIScore = currenQuarter.KPIScore,
                    KeyTaskScore = currenQuarter.KeyTaskScore,
                    OMGScore = currenQuarter.OMGScore,
                    CreatedBy = CurrenUser.Id,
                    Month = currenQuarter.Month,
                    Quarter = currenQuarter.Quarter,
                    HalfYear = currenQuarter.HalfYear,
                    Year = currenQuarter.Year,
                    TotaleScore = currenQuarter.TotaleScore,
                });
            }
            if (quarterScores.Count != 2)
                return;

            // Trung bình cộng
            decimal avgKPI = quarterScores.Average(x => x.KPIScore);
            decimal avgKeyTask = quarterScores.Average(x => x.KeyTaskScore);
            decimal avgOMG = quarterScores.Average(x => x.OMGScore);
            decimal avgTotal = quarterScores.Average(x => x.TotaleScore);

            var halfYearScore = new CalculateQuarterPointDto
            {
                KPIScore = Math.Round(avgKPI, 2),
                KeyTaskScore = Math.Round(avgKeyTask, 2),
                OMGScore = Math.Round(avgOMG, 2),
                TotaleScore = Math.Round(avgTotal, 2),
                Title = $"EMC - {Year} - H{halfYear} - {CurrenUser.FullName} ",
                Year =Year,
                HalfYear = halfYear,
                CreatedBy = user.Id,
                ApprovedBy = CurrenUser.Id
            };
            calculateQuarterPoint.Add(halfYearScore);
        }
    }
    #endregion

}
