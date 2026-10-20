using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using AquaSolution.Client.Modals.KPI.ScopeManagement;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Scope;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AquaSolution.Client.Pages.KPI.ScopeManagement
{
    public partial class KPIScopeManagement
    {
        [Inject] private HttpClient Http { get; set; }

        private KPIScopeModal scopeModal;

        // Scopes data & filter
        private List<KPIScopeDto> allScopes = new();
        private List<KPIScopeDto> filteredScopes = new();
        private bool isLoadingScopes = false;

        private int filterYear = DateTime.Now.Year;
        private Guid? filterDepartmentId;
        private KPIScopeType? filterScopeType;
        private bool? filterIsActive;
        private string searchKeyword = string.Empty;

        public class FilterOptionItem
        {
            public string Value { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
        }

        private string filterScopeTypeStr = "ALL";
        private List<FilterOptionItem> scopeTypeOptions = new()
        {
            new() { Value = "ALL", Label = "-- Tất cả loại --" },
            new() { Value = nameof(KPIScopeType.NewEmployee), Label = "Nhân viên mới" },
            new() { Value = nameof(KPIScopeType.MaternityLeave), Label = "Nghỉ thai sản" },
            new() { Value = nameof(KPIScopeType.LongTermLeave), Label = "Nghỉ dài hạn" },
            new() { Value = nameof(KPIScopeType.Other), Label = "Khác" }
        };

        private string filterActiveStr = "ALL";
        private List<FilterOptionItem> activeOptions = new()
        {
            new() { Value = "ALL", Label = "-- Tất cả trạng thái --" },
            new() { Value = "ACTIVE", Label = "Đang hiệu lực" },
            new() { Value = "INACTIVE", Label = "Đã vô hiệu" }
        };

        private List<DepartmentDto> departments = new();

        // Matrix data & filter (Tab 2)
        private List<KPIComplianceMatrixDto> matrixData = new();
        private List<KPIComplianceMatrixDto> filteredMatrixData = new();
        private bool isLoadingMatrix = false;
        private int matrixYear = DateTime.Now.Year;
        private Guid? matrixDepartmentId;
        private string matrixComplianceFilterStr = "ALL";
        private string matrixSearchKeyword = string.Empty;

        private List<FilterOptionItem> complianceFilterOptions = new()
        {
            new() { Value = "ALL", Label = "-- Tất cả trạng thái --" },
            new() { Value = "MISSING", Label = "Đang thiếu KPI" },
            new() { Value = "PENDING", Label = "Đang chờ duyệt" },
            new() { Value = "COMPLIANT", Label = "Đầy đủ KPI" }
        };

        // Missing KPI Data & Filters (Tab 3)
        private int missingFilterYear = DateTime.Now.Year;
        private Guid? missingDepartmentId;
        private string missingMonthFilterStr = "0";
        private string missingSearchKeyword = string.Empty;
        private List<KPIComplianceMatrixDto> allMissingData = new();
        private List<KPIComplianceMatrixDto> filteredMissingData = new();
        private bool isLoadingMissing = false;

        private int missingTabTotalUsers = 0;
        private int missingTabCompliantUsers = 0;
        private int missingTabPendingUsers = 0;
        private int missingTabMissingUsers = 0;

        private List<FilterOptionItem> missingMonthOptions = new()
        {
            new() { Value = "0", Label = "-- Tất cả các tháng thiếu --" },
            new() { Value = "1", Label = "Thiếu Tháng 1" },
            new() { Value = "2", Label = "Thiếu Tháng 2" },
            new() { Value = "3", Label = "Thiếu Tháng 3" },
            new() { Value = "4", Label = "Thiếu Tháng 4" },
            new() { Value = "5", Label = "Thiếu Tháng 5" },
            new() { Value = "6", Label = "Thiếu Tháng 6" },
            new() { Value = "7", Label = "Thiếu Tháng 7" },
            new() { Value = "8", Label = "Thiếu Tháng 8" },
            new() { Value = "9", Label = "Thiếu Tháng 9" },
            new() { Value = "10", Label = "Thiếu Tháng 10" },
            new() { Value = "11", Label = "Thiếu Tháng 11" },
            new() { Value = "12", Label = "Thiếu Tháng 12" }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadDepartments();
            await LoadScopes();
            await LoadMatrixData();
            await LoadMissingData();
        }

        private async Task LoadDepartments()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all");
                if (result != null)
                {
                    departments = result.OrderBy(d => d.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải danh sách phòng ban: {ex.Message}");
            }
        }

        private async Task LoadScopes()
        {
            isLoadingScopes = true;
            try
            {
                var url = $"api/kpiscope/list?year={filterYear}";
                if (filterDepartmentId.HasValue) url += $"&departmentId={filterDepartmentId.Value}";
                if (filterIsActive.HasValue) url += $"&isActive={filterIsActive.Value}";

                var result = await Http.GetFromJsonAsync<List<KPIScopeDto>>(url);
                allScopes = result ?? new();
                ApplyScopeFilter();
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải danh sách hồ sơ: {ex.Message}");
            }
            finally
            {
                isLoadingScopes = false;
                StateHasChanged();
            }
        }

        private async Task OnFilterYearChanged(int val)
        {
            filterYear = val;
            await LoadScopes();
        }

        private async Task OnMatrixYearChanged(int val)
        {
            matrixYear = val;
            await LoadMatrixData();
        }

        private async Task OnDepartmentFilterChanged()
        {
            await LoadScopes();
        }

        private async Task OnActiveStatusFilterChanged()
        {
            if (filterActiveStr == "ALL") filterIsActive = null;
            else if (filterActiveStr == "ACTIVE") filterIsActive = true;
            else if (filterActiveStr == "INACTIVE") filterIsActive = false;

            await LoadScopes();
        }

        private void OnScopeTypeFilterChanged()
        {
            if (filterScopeTypeStr == "ALL") filterScopeType = null;
            else if (Enum.TryParse<KPIScopeType>(filterScopeTypeStr, out var val)) filterScopeType = val;

            ApplyScopeFilter();
        }

        private void OnFilterChanged()
        {
            ApplyScopeFilter();
        }

        private void SearchInputChanged(ChangeEventArgs e)
        {
            searchKeyword = e.Value?.ToString() ?? string.Empty;
            ApplyScopeFilter();
        }

        private void ApplyScopeFilter()
        {
            var query = allScopes.AsEnumerable();

            if (filterScopeType.HasValue)
            {
                query = query.Where(s => s.ScopeType == filterScopeType.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var kw = searchKeyword.Trim().ToLower();
                query = query.Where(s => (s.UserName != null && s.UserName.ToLower().Contains(kw))
                                      || (s.UserFullName != null && s.UserFullName.ToLower().Contains(kw))
                                      || (s.DepartmentName != null && s.DepartmentName.ToLower().Contains(kw)));
            }

            filteredScopes = query.ToList();
            StateHasChanged();
        }

        private async Task LoadMatrixData()
        {
            isLoadingMatrix = true;
            try
            {
                var url = $"api/kpiscope/compliance-matrix?year={matrixYear}";
                if (matrixDepartmentId.HasValue) url += $"&departmentId={matrixDepartmentId.Value}";

                var result = await Http.GetFromJsonAsync<List<KPIComplianceMatrixDto>>(url);
                matrixData = result ?? new();
                ApplyMatrixFilter();
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải ma trận tuân thủ: {ex.Message}");
            }
            finally
            {
                isLoadingMatrix = false;
                StateHasChanged();
            }
        }

        private void OnMatrixComplianceFilterChanged()
        {
            ApplyMatrixFilter();
        }

        private void MatrixSearchInputChanged(ChangeEventArgs e)
        {
            matrixSearchKeyword = e.Value?.ToString() ?? string.Empty;
            ApplyMatrixFilter();
        }

        private void ApplyMatrixFilter()
        {
            var query = matrixData.AsEnumerable();

            if (matrixComplianceFilterStr == "MISSING")
            {
                query = query.Where(m => m.TotalMissingMonths > 0);
            }
            else if (matrixComplianceFilterStr == "PENDING")
            {
                query = query.Where(m => m.TotalPendingMonths > 0);
            }
            else if (matrixComplianceFilterStr == "COMPLIANT")
            {
                query = query.Where(m => m.TotalMissingMonths == 0 && m.TotalPendingMonths == 0);
            }

            if (!string.IsNullOrWhiteSpace(matrixSearchKeyword))
            {
                var kw = matrixSearchKeyword.Trim().ToLower();
                query = query.Where(m => (m.UserName != null && m.UserName.ToLower().Contains(kw))
                                      || (m.UserFullName != null && m.UserFullName.ToLower().Contains(kw))
                                      || (m.DepartmentName != null && m.DepartmentName.ToLower().Contains(kw))
                                      || (m.PositionName != null && m.PositionName.ToLower().Contains(kw)));
            }

            filteredMatrixData = query.ToList();
            StateHasChanged();
        }

        // Tab 3: Missing KPI Handlers
        private async Task OnMissingYearChanged(int val)
        {
            missingFilterYear = val;
            await LoadMissingData();
        }

        private void OnMissingMonthFilterChanged()
        {
            ApplyMissingFilter();
        }

        private void MissingSearchInputChanged(ChangeEventArgs e)
        {
            missingSearchKeyword = e.Value?.ToString() ?? string.Empty;
            ApplyMissingFilter();
        }

        private async Task LoadMissingData()
        {
            isLoadingMissing = true;
            try
            {
                var url = $"api/kpiscope/compliance-matrix?year={missingFilterYear}";
                if (missingDepartmentId.HasValue) url += $"&departmentId={missingDepartmentId.Value}";

                var result = await Http.GetFromJsonAsync<List<KPIComplianceMatrixDto>>(url);
                var list = result ?? new();

                missingTabTotalUsers = list.Count;
                missingTabCompliantUsers = list.Count(m => m.TotalMissingMonths == 0 && m.TotalPendingMonths == 0);
                missingTabPendingUsers = list.Count(m => m.TotalPendingMonths > 0);
                allMissingData = list.Where(m => m.TotalMissingMonths > 0).ToList();
                missingTabMissingUsers = allMissingData.Count;

                ApplyMissingFilter();
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải danh sách thiếu KPI: {ex.Message}");
            }
            finally
            {
                isLoadingMissing = false;
                StateHasChanged();
            }
        }

        private void ApplyMissingFilter()
        {
            var query = allMissingData.AsEnumerable();

            if (int.TryParse(missingMonthFilterStr, out var m) && m > 0)
            {
                query = query.Where(x => x.MissingMonthNumbers.Contains(m));
            }

            if (!string.IsNullOrWhiteSpace(missingSearchKeyword))
            {
                var kw = missingSearchKeyword.Trim().ToLower();
                query = query.Where(x => (x.UserName != null && x.UserName.ToLower().Contains(kw))
                                      || (x.UserFullName != null && x.UserFullName.ToLower().Contains(kw))
                                      || (x.DepartmentName != null && x.DepartmentName.ToLower().Contains(kw))
                                      || (x.PositionName != null && x.PositionName.ToLower().Contains(kw)));
            }

            filteredMissingData = query.ToList();
            StateHasChanged();
        }

        private async Task OpenCreateModal()
        {
            await scopeModal.ShowModal();
        }

        private async Task OpenCreateModalForUser(KPIComplianceMatrixDto userItem)
        {
            if (userItem != null)
            {
                await scopeModal.ShowModalForUser(userItem.UserId, userItem.UserName, userItem.UserFullName);
            }
        }

        private async Task EditScope(KPIScopeDto scope)
        {
            await scopeModal.ShowModal(scope);
        }

        private async Task DeactivateScope(KPIScopeDto scope)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", $"Bạn có chắc chắn muốn hủy hồ sơ miễn KPI của {scope.UserFullName} không?");
            if (!confirm) return;

            try
            {
                var response = await Http.PostAsync($"api/kpiscope/deactivate/{scope.Id}", null);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Hủy hồ sơ thành công.");
                    await LoadScopes();
                    await LoadMatrixData();
                    await LoadMissingData();
                }
                else
                {
                    await Message.Error("Không thể hủy hồ sơ.");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
        }

        private async Task OnModalSaved()
        {
            await LoadScopes();
            await LoadMatrixData();
            await LoadMissingData();
        }

        private static string GetScopeTypeName(KPIScopeType type)
        {
            return type switch
            {
                KPIScopeType.NewEmployee => "Nhân viên mới",
                KPIScopeType.MaternityLeave => "Nghỉ thai sản",
                KPIScopeType.LongTermLeave => "Nghỉ dài hạn",
                KPIScopeType.Other => "Khác",
                _ => type.ToString()
            };
        }

        private static string GetActiveStatusLabel(bool? status)
        {
            if (!status.HasValue) return "-- Tất cả --";
            return status.Value ? "Đang hiệu lực" : "Đã vô hiệu";
        }
    }
}
