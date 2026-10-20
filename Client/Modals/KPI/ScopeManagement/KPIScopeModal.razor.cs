using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Scope;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;

namespace AquaSolution.Client.Modals.KPI.ScopeManagement
{
    public partial class KPIScopeModal
    {
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }

        private bool IsModalVisible = false;
        private bool isEdit = false;
        private bool isSaving = false;
        private string modalTitle = "Thêm mới phạm vi đánh giá KPI";
        private string selectedUserDisplayName = string.Empty;

        private Form<KPIScopeCreateOrUpdateDto> formRef;
        private KPIScopeCreateOrUpdateDto model = new();

        private List<UserDto> users = new();
        private List<KPIScopeType> scopeTypes = new();
        private int selectedYear = DateTime.Now.Year;

        public class MonthCheckboxItem
        {
            public int Month { get; set; }
            public bool IsChecked { get; set; }
            public string Note { get; set; }
        }

        private List<MonthCheckboxItem> checkedMonths = new();

        public async Task ShowModal(KPIScopeDto? existing = null)
        {
            await LoadUsers();
            scopeTypes = Enum.GetValues(typeof(KPIScopeType)).Cast<KPIScopeType>().ToList();
            InitMonthCheckboxes();

            if (existing != null)
            {
                isEdit = true;
                modalTitle = "Chỉnh sửa phạm vi đánh giá KPI";
                selectedUserDisplayName = $"{existing.UserName} - {existing.UserFullName}";
                model = new KPIScopeCreateOrUpdateDto
                {
                    Id = existing.Id,
                    UserId = existing.UserId,
                    ScopeType = existing.ScopeType,
                    EffectiveDate = existing.EffectiveDate,
                    StartDate = existing.StartDate,
                    EndDate = existing.EndDate,
                    Reason = existing.Reason,
                    IsActive = existing.IsActive
                };

                if (existing.EffectiveDate.HasValue)
                {
                    selectedYear = existing.EffectiveDate.Value.Year;
                }
                else if (existing.StartDate.HasValue)
                {
                    selectedYear = existing.StartDate.Value.Year;
                }
                else if (existing.ExemptMonths.Any())
                {
                    selectedYear = existing.ExemptMonths.First().Year;
                }

                // Map existing exempt months into checkboxes
                foreach (var em in existing.ExemptMonths.Where(x => x.Year == selectedYear))
                {
                    var cb = checkedMonths.FirstOrDefault(m => m.Month == em.Month);
                    if (cb != null)
                    {
                        cb.IsChecked = true;
                        cb.Note = em.Note;
                    }
                }
            }
            else
            {
                isEdit = false;
                modalTitle = "Thêm mới phạm vi đánh giá KPI";
                selectedUserDisplayName = string.Empty;
                selectedYear = DateTime.Now.Year;
                model = new KPIScopeCreateOrUpdateDto
                {
                    ScopeType = KPIScopeType.NewEmployee,
                    IsActive = true
                };
            }

            IsModalVisible = true;
            StateHasChanged();
        }

        public async Task ShowModalForUser(Guid userId, string userName, string fullName)
        {
            await ShowModal();
            model.UserId = userId;
            selectedUserDisplayName = $"{userName} - {fullName}";
            StateHasChanged();
        }

        private void InitMonthCheckboxes()
        {
            checkedMonths = Enumerable.Range(1, 12).Select(m => new MonthCheckboxItem
            {
                Month = m,
                IsChecked = false,
                Note = string.Empty
            }).ToList();
        }

        private async Task LoadUsers()
        {
            if (users.Count == 0)
            {
                try
                {
                    var result = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
                    if (result != null)
                    {
                        users = result.Where(u => u.IsActive && u.PositionId.HasValue && u.PositionId.Value != Guid.Empty).OrderBy(u => u.FullName).ToList();
                    }
                }
                catch (Exception ex)
                {
                    await Message.Error($"Không thể tải danh sách nhân viên: {ex.Message}");
                }
            }
        }

        private void OnUserSelected(UserDto user)
        {
            if (user != null)
            {
                selectedUserDisplayName = $"{user.WorkDayId} - {user.FullName}";
            }
        }

        private void OnScopeTypeChanged(KPIScopeType type)
        {
            AutoGenerateExemptMonths();
        }

        private void OnEffectiveDateChanged(DateTime? date)
        {
            model.EffectiveDate = date;
            if (date.HasValue)
            {
                selectedYear = date.Value.Year;
                AutoGenerateExemptMonths();
            }
        }

        private void OnStartDateChanged(DateTime? date)
        {
            model.StartDate = date;
            if (date.HasValue)
            {
                selectedYear = date.Value.Year;
                AutoGenerateExemptMonths();
            }
        }

        private void OnEndDateChanged(DateTime? date)
        {
            model.EndDate = date;
            AutoGenerateExemptMonths();
        }

        private void AutoGenerateExemptMonths()
        {
            // Reset all check state
            foreach (var m in checkedMonths)
            {
                m.IsChecked = false;
                m.Note = string.Empty;
            }

            switch (model.ScopeType)
            {
                case KPIScopeType.NewEmployee:
                    if (model.EffectiveDate.HasValue)
                    {
                        int hireYear = model.EffectiveDate.Value.Year;
                        int hireMonth = model.EffectiveDate.Value.Month;

                        if (selectedYear == hireYear)
                        {
                            // Tháng vào làm (dù đầu hay giữa tháng) được miễn KPI
                            for (int m = 1; m <= hireMonth; m++)
                            {
                                var cb = checkedMonths.FirstOrDefault(x => x.Month == m);
                                if (cb != null)
                                {
                                    cb.IsChecked = true;
                                    cb.Note = $"Nhân viên mới (vào làm {model.EffectiveDate.Value:dd/MM/yyyy})";
                                }
                            }
                        }
                    }
                    break;

                case KPIScopeType.MaternityLeave:
                case KPIScopeType.LongTermLeave:
                case KPIScopeType.Other:
                    if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate <= model.EndDate)
                    {
                        var start = model.StartDate.Value;
                        var end = model.EndDate.Value;
                        string label = model.ScopeType == KPIScopeType.MaternityLeave ? "Nghỉ thai sản"
                            : (model.ScopeType == KPIScopeType.LongTermLeave ? "Nghỉ dài hạn" : "Miễn đặc biệt");

                        var cur = new DateTime(start.Year, start.Month, 1);
                        var endMonth = new DateTime(end.Year, end.Month, 1);

                        while (cur <= endMonth)
                        {
                            if (cur.Year == selectedYear)
                            {
                                var cb = checkedMonths.FirstOrDefault(x => x.Month == cur.Month);
                                if (cb != null)
                                {
                                    cb.IsChecked = true;
                                    cb.Note = $"{label} ({start:dd/MM/yyyy} - {end:dd/MM/yyyy})";
                                }
                            }
                            cur = cur.AddMonths(1);
                        }
                    }
                    break;
            }

            StateHasChanged();
        }

        private async Task SaveAsync()
        {
            if (model.UserId == Guid.Empty)
            {
                await Message.Warning("Vui lòng chọn nhân viên.");
                return;
            }

            if (model.ScopeType == KPIScopeType.NewEmployee && !model.EffectiveDate.HasValue)
            {
                await Message.Warning("Vui lòng nhập ngày bắt đầu vào làm (Effective Date).");
                return;
            }

            if (model.ScopeType != KPIScopeType.NewEmployee && (!model.StartDate.HasValue || !model.EndDate.HasValue))
            {
                await Message.Warning("Vui lòng nhập Từ ngày và Đến ngày.");
                return;
            }

            model.Reason = model.Reason?.Trim() ?? string.Empty;

            // Collect exempt months
            model.ExemptMonths = checkedMonths
                .Where(m => m.IsChecked)
                .Select(m => new ExemptMonthItem
                {
                    Year = selectedYear,
                    Month = m.Month,
                    Note = string.IsNullOrWhiteSpace(m.Note) ? GetScopeTypeName(model.ScopeType) : m.Note.Trim()
                }).ToList();

            isSaving = true;
            try
            {
                HttpResponseMessage response;
                if (isEdit)
                {
                    response = await Http.PutAsJsonAsync("api/kpiscope/update", model);
                }
                else
                {
                    response = await Http.PostAsJsonAsync("api/kpiscope/create", model);
                }

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(isEdit ? "Cập nhật hồ sơ thành công." : "Tạo hồ sơ thành công.");
                    IsModalVisible = false;
                    await OnSaved.InvokeAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Lưu thất bại: {error}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Có lỗi xảy ra: {ex.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
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
    }
}
