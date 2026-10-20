using AntDesign;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Modals.KPI.UserTask
{
    public partial class PeriodYearCalculateModal
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IMessageService Message { get; set; } = default!;

        [Parameter] public EventCallback OnSubmitted { get; set; }

        private bool IsModalVisible { get; set; }
        private UserDto? targetUser;
        private int selectedYear = DateTime.Now.Year;
        private bool isLoading;
        private bool isSubmitting;

        private SpecialPeriodYearResultDto? dataResult;
        private bool includeQ4;
        private bool includeH2;

        public async Task ShowModalAsync(UserDto user, int? year = null)
        {
            targetUser = user;
            selectedYear = year ?? DateTime.Now.Year;
            IsModalVisible = true;
            dataResult = null;
            includeQ4 = false;
            includeH2 = false;

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (targetUser == null) return;

            isLoading = true;
            StateHasChanged();

            try
            {
                var response = await Http.GetFromJsonAsync<SpecialPeriodYearResultDto>(
                    $"api/kpisubmit/check-special-period-year-eligibility/{targetUser.Id}/{selectedYear}");

                dataResult = response;
                if (dataResult != null)
                {
                    includeQ4 = dataResult.CanCalculateQ4;
                    includeH2 = dataResult.CanCalculateH2;
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi khi kiểm tra dữ liệu: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (dataResult == null || !dataResult.IsEligible || targetUser == null)
            {
                await Message.Warning("Chưa đủ điều kiện để gửi phê duyệt.");
                return;
            }

            isSubmitting = true;
            StateHasChanged();

            try
            {
                var dto = new SubmitSpecialPeriodYearDto
                {
                    UserId = targetUser.Id,
                    Year = selectedYear,
                    IncludeQ4 = includeQ4 && dataResult.CanCalculateQ4,
                    IncludeH2 = includeH2 && dataResult.CanCalculateH2,
                    IncludeYear = true,
                    Q4Score = dataResult.Q4Score,
                    H2Score = dataResult.H2Score,
                    YearScore = dataResult.YearScore ?? new KPITotalScoreDto()
                };

                var response = await Http.PostAsJsonAsync("api/kpisubmit/submit-special-period-year", dto);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success($"Đã tạo phiếu chốt kỳ và gửi phê duyệt thành công cho nhân viên {targetUser.FullName}!");
                    IsModalVisible = false;
                    if (OnSubmitted.HasDelegate)
                    {
                        await OnSubmitted.InvokeAsync();
                    }
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Gửi phê duyệt thất bại: {err}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Có lỗi xảy ra: {ex.Message}");
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        private void Close()
        {
            IsModalVisible = false;
        }
    }
}
