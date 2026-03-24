
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.HRMS;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.HRMS
{
    public partial class BonusYearManagement
    {

        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<BonusYearDto>? listBonusYear = new();
        private List<BonusYearDto>? listBonusYearFilter = new();

        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadData();
            await LoadDataFilterAsync();
        }
        private async Task GetPage()
        {
            var url = "user-management";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        private async Task LoadData()
        {
            try
            {

                StateHasChanged();
                if (Http != null) listBonusYear = await Http.GetFromJsonAsync<List<BonusYearDto>>("api/user/get-all");
                listBonusYear = listBonusYearFilter;
        
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        private async Task CheckPermission()
        {
            //if (Http != null)
            //{
            //    var currenUserClass = new CurrenUser(Http, AuthStateProvider);
            //    CurrenUser = await currenUserClass.LoadCurrenUser();
            //}

           
        }
        #endregion
        #region Action
        private async Task DetailUser(BonusYearDto user)
        {
            //await _detailModal?.ShowModal(user, new CurrentUserInfo(), false)!;
        }
        #endregion
        #region Handle Data
        private async Task HandleSaved()
        {
            await LoadData();
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
        private Task Search()
        {
            try
            {
                //var workDayId = StringHelper.NormalizeText(WorkDayId?.Trim());
                //var fullName = StringHelper.NormalizeText(FullName?.Trim());

                //if (_users != null)
                //{
                //    var filtered = _users
                //        .Where(x =>
                //            (string.IsNullOrWhiteSpace(workDayId) || (!string.IsNullOrEmpty(x.WorkDayId) && StringHelper.NormalizeText(x.WorkDayId).Contains(workDayId))) &&
                //            (string.IsNullOrWhiteSpace(fullName) || (!string.IsNullOrEmpty(x.FullName) && StringHelper.NormalizeText(x.FullName).Contains(fullName)))
                //        )
                //        .ToList();

                //    if (string.IsNullOrWhiteSpace(workDayId) &&
                //        string.IsNullOrWhiteSpace(fullName))
                //    {
                //        filtered = _users;
                //    }

                //    _userFilter = filtered;
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }

            return Task.CompletedTask;
        }
        private async Task Reset()
        {
            WorkDayId = null;
            FullName = null;
            //_userFilter = _users;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        private Table<BonusYearDto>? _tableRef;
        private List<DepartmentDto> _listDepartment = new();
        private List<FactoryDto> _listFactory = new();
        private List<PositionDto> _listPosition = new();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _positionFilter = Array.Empty<TableFilter<string>>();

        private async Task LoadDataFilterAsync()
        {
            if (Http != null)
            {
                _listDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ??
                                  new List<DepartmentDto>();
                _departmentFilter = _listDepartment
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();

                _listFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ??
                               new List<FactoryDto>();
                _factoryFilter = _listFactory
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();

                _listPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all") ??
                                new List<PositionDto>();
            }

            _positionFilter = _listPosition
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new TableFilter<string>
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = false
                })
                .ToArray();
            //if (_users != null)
            //    foreach (var user in _users)
            //    {
            //        user.FactoryName ??= string.Empty;
            //        user.DepartmentName ??= string.Empty;
            //        user.PositionName ??= string.Empty;
            //    }
        }
        #endregion
        #region Import
        private List<BonusYearDto> BonusYears = new();

        private async Task HandleBonusYearFile(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file == null)
                    return;

                using var ms = await file.ToMemoryStreamAsync();

                BonusYears = await ExcelImportHelper.ReadFromExcelAsync<BonusYearDto>(
                    ms,
                    (sheet, row) =>
                    {
                        if (string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text))
                            return null;

                        DateTime.TryParse(sheet.Cells[row, 5].Text, out var joinDate);

                        return new BonusYearDto
                        {
                            EmpWorkDay = sheet.Cells[row, 1].Text.Trim(),
                            EmpName = sheet.Cells[row, 2].Text.Trim(),
                            EmpCMND = sheet.Cells[row, 3].Text.Trim(),
                            EmpDept = sheet.Cells[row, 4].Text.Trim(),
                            EmpJoinDate = joinDate,

                            Q1Rated = sheet.Cells[row, 6].Text.Trim(),
                            Q2Rated = sheet.Cells[row, 7].Text.Trim(),
                            Q3Rated = sheet.Cells[row, 8].Text.Trim(),
                            Q4Rated = sheet.Cells[row, 9].Text.Trim(),

                            Q1Ratio = sheet.Cells[row, 10].Text.Trim(),
                            Q2Ratio = sheet.Cells[row, 11].Text.Trim(),
                            Q3Ratio = sheet.Cells[row, 12].Text.Trim(),
                            Q4Ratio = sheet.Cells[row, 13].Text.Trim(),

                            YearRation = sheet.Cells[row, 14].Text.Trim(),
                            NoteRatio = sheet.Cells[row, 15].Text.Trim(),
                            WorkTimeRation = sheet.Cells[row, 16].Text.Trim(),
                            AwardYearRatio = sheet.Cells[row, 17].Text.Trim(),
                            BonusYear = sheet.Cells[row, 18].Text.Trim(),
                        };
                    });
                var response = await Http.PostAsJsonAsync(
                   "api/hrms/import-bonus-year",
                   BonusYears);

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success($"Import thành công {BonusYears.Count} dòng");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error(error);
                }
            }
            catch (Exception ex)
            {
                await Message.Error(ex.Message);
            }
        }

        #endregion

    }
}
