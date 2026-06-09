using AntDesign;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.ReportDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Scrap
{
    public partial class ReportManagement : ComponentBase, IAsyncDisposable
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private IMessageService Message { get; set; } = default!;

        // ─── Filter state ─────────────────────────────────────────────────────
        private ReportFilterDto _filter = new()
        {
            Period = FilterPeriod.Month,
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            Week = ISOWeek.GetWeekOfYear(DateTime.Today)
        };

        private ReportPageDto? _data;
        private bool _loading;
        private bool _exporting;
        private bool _domReady;

        // ─── Source lists ─────────────────────────────────────────────────────
        private List<FactoryDto> _factories = new();
        private List<int> _years = Enumerable.Range(DateTime.Now.Year - 4, 5).Reverse().ToList();

        // Tháng
        public class OptionItem
        {
            public int Value { get; set; }
            public string Label { get; set; } = string.Empty;
        }
        private List<OptionItem> _months = Enumerable.Range(1, 12)
            .Select(m => new OptionItem { Value = m, Label = $"Tháng {m}" })
            .ToList();
        private int _monthVal = DateTime.Now.Month;

        // Tuần
        public class WeekItem
        {
            public int Value { get; set; }
            public string Label { get; set; } = string.Empty;
        }
        private List<WeekItem> _weeks = new();
        private int _weekVal = ISOWeek.GetWeekOfYear(DateTime.Today);

        // ─── Lifecycle ────────────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            BuildWeekList(); // khởi tạo danh sách tuần ngay từ đầu
            await LoadFactoriesAsync();
            await LoadReportAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_data is not null && !_domReady)
            {
                _domReady = true;
                await RenderChartsAsync();
            }
        }

        // ─── Build danh sách tuần theo năm ───────────────────────────────────
        private void BuildWeekList()
        {
            // Ngày 28/12 luôn thuộc tuần cuối cùng của năm ISO
            int totalWeeks = ISOWeek.GetWeekOfYear(new DateTime(_filter.Year, 12, 28));

            _weeks = Enumerable.Range(1, totalWeeks).Select(w =>
            {
                var start = ISOWeek.ToDateTime(_filter.Year, w, DayOfWeek.Monday);
                var end = start.AddDays(6); // inclusive end (Chủ Nhật)
                return new WeekItem
                {
                    Value = w,
                    Label = $"Tuần {w}  ({start:dd/MM} – {end:dd/MM})"
                };
            }).ToList();
        }

        // ─── Data loading ─────────────────────────────────────────────────────
        private async Task LoadFactoriesAsync()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all");
                _factories = result ?? new();
            }
            catch
            {
                _factories = new();
            }
        }

        private async Task LoadReportAsync()
        {
            _loading = true;
            StateHasChanged();

            try
            {
                // Sync từ picker về filter trước khi build query
                if (_filter.Period == FilterPeriod.Month)
                    _filter.Month = _monthVal;
                if (_filter.Period == FilterPeriod.Week)
                    _filter.Week = _weekVal;

                var query = BuildQueryString();
                _data = await Http.GetFromJsonAsync<ReportPageDto>($"api/report/page?{query}");
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải báo cáo: {ex.Message}");
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }

            if (_domReady && _data is not null)
                await RenderChartsAsync();
        }

        private string BuildQueryString()
        {
            var parts = new List<string>
            {
                $"Period={_filter.Period}",
                $"Year={_filter.Year}"
            };

            if (_filter.FactoryId.HasValue)
                parts.Add($"FactoryId={_filter.FactoryId}");

            if (_filter.Period == FilterPeriod.Month && _filter.Month.HasValue)
                parts.Add($"Month={_filter.Month}");

            if (_filter.Period == FilterPeriod.Week && _filter.Week.HasValue)
                parts.Add($"Week={_filter.Week}");

            return string.Join("&", parts);
        }

        // ─── Event handlers ───────────────────────────────────────────────────
        private async Task OnFactoryChanged(FactoryDto? factory)
        {
            _filter.FactoryId = factory?.Id;
            await LoadReportAsync();
        }

        private async Task OnFilterChangedFilter(FilterPeriod value)
        {
            _filter.Period = value;

            if (value == FilterPeriod.Week)
            {
                BuildWeekList();
                // Nếu tuần hiện tại vượt quá tổng tuần của năm → về tuần 1
                if (_weekVal > _weeks.Count)
                    _weekVal = 1;
                _filter.Week = _weekVal;
            }

            await LoadReportAsync();
        }

        private async Task OnWeekChanged()
        {
            _filter.Week = _weekVal;
            await LoadReportAsync();
        }

        private async Task OnFilterChanged()
        {
            // Khi đổi năm mà đang ở tab Tuần → rebuild danh sách tuần của năm mới
            if (_filter.Period == FilterPeriod.Week)
            {
                BuildWeekList();
                if (_weekVal > _weeks.Count)
                    _weekVal = 1;
                _filter.Week = _weekVal;
            }

            await LoadReportAsync();
        }

        private async Task ExportReport()
        {
            _exporting = true;
            try
            {
                var factoryName = _filter.FactoryId.HasValue
                    ? _factories.FirstOrDefault(f => f.Id == _filter.FactoryId)?.Name ?? "Tất cả nhà máy"
                    : "Tất cả nhà máy";

                var query = BuildQueryString();
                var encodedName = Uri.EscapeDataString(factoryName);
                var response = await Http.GetAsync($"api/report/export?{query}&factoryName={encodedName}");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                                ?? response.Content.Headers.ContentDisposition?.FileName
                                ?? $"BaoCaoScrap_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    await JS.InvokeVoidAsync("downloadFile", fileName,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", bytes);

                    await Message.Success("Xuất báo cáo thành công!");
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Xuất file thất bại: {err}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _exporting = false;
            }
        }

        // ─── Charts ───────────────────────────────────────────────────────────
        private async Task RenderChartsAsync()
        {
            if (_data is null) return;

            try { await JS.InvokeVoidAsync("destroyAllCharts"); } catch { }

            var deptLabels = _data.DepartmentReport.Select(x => x.DepartmentName).ToArray();
            var deptWeights = _data.DepartmentReport.Select(x => (double)x.TotalWeight).ToArray();
            await JS.InvokeVoidAsync("renderPieChart", "deptPieChart", deptLabels, deptWeights);

            var trendLabels = _data.Trend.Select(x => x.Label).ToArray();
            var trendWeight = _data.Trend.Select(x => (double)x.TotalWeight).ToArray();
            var trendConfirm = _data.Trend.Select(x => (double)x.ConfirmedWeight).ToArray();
            await JS.InvokeVoidAsync("renderCompareBarChart", "compareBarChart",
                trendLabels, trendWeight, trendConfirm);

            var top5 = _data.MaterialReport.Take(5).ToList();
            var matLabels = top5.Select(x => x.Name).ToArray();
            var matWeights = top5.Select(x => (double)x.TotalWeight).ToArray();
            await JS.InvokeVoidAsync("renderHBarChart", "matBarChart", matLabels, matWeights);

            var trendOrders = _data.Trend.Select(x => (double)x.TotalOrders).ToArray();
            await JS.InvokeVoidAsync("renderLineChart", "trendLineChart", trendLabels, trendOrders);

            await JS.InvokeVoidAsync("renderDoughnutChart", "statusDoughnutChart",
                new[] { "Đã duyệt", "Đang duyệt", "Từ chối" },
                new[] { _data.ApprovalStatus.Approved, _data.ApprovalStatus.Pending, _data.ApprovalStatus.Rejected });
        }

        public async ValueTask DisposeAsync()
        {
            try { await JS.InvokeVoidAsync("destroyAllCharts"); } catch { }
        }
    }
}
