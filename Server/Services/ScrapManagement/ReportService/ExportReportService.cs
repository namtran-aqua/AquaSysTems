using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.ScrapManagetment.ReportServices;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ReportDto;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;

namespace AquaSolution.Server.Services.ScrapManagetment.ReportServices
{
    public class ExportReportService : IExportReportService
    {
        private readonly IReportService _reportService;

        // ── Màu ────────────────────────────────────────────────────────────────
        private static readonly XLColor CHeaderBg = XLColor.FromHtml("#1A3C6B");
        private static readonly XLColor CHeaderFg = XLColor.White;
        private static readonly XLColor CSubHdrBg = XLColor.FromHtml("#2E75B6");
        private static readonly XLColor CAltRow = XLColor.FromHtml("#EBF3FB");
        private static readonly XLColor CTotalBg = XLColor.FromHtml("#D6E4F0");
        private static readonly XLColor CTotalFg = XLColor.FromHtml("#1A3C6B");
        private static readonly XLColor CGreen = XLColor.FromHtml("#1E7145");
        private static readonly XLColor COrange = XLColor.FromHtml("#C55A11");
        private static readonly XLColor CRed = XLColor.FromHtml("#C00000");
        private static readonly XLColor CBorder = XLColor.FromHtml("#BDD7EE");

        public ExportReportService(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<byte[]> ExportAsync(ReportFilterDto filter, string factoryName)
        {
            var data = await _reportService.GetReportPageAsync(filter);

            var periodLabel = filter.Period switch
            {
                FilterPeriod.Week => $"Tuần {filter.Week}/{filter.Year}",
                FilterPeriod.Month => $"Tháng {filter.Month}/{filter.Year}",
                FilterPeriod.Year => $"Năm {filter.Year}",
                _ => filter.Year.ToString()
            };

            using var wb = new XLWorkbook();
            BuildSheet1(wb, data, factoryName, periodLabel);
            BuildSheet2(wb, data, factoryName, periodLabel);
            BuildSheet3(wb, data, factoryName, periodLabel);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════════
        // SHEET 1 — TỔNG QUAN + PHÒNG BAN
        // ═══════════════════════════════════════════════════════════════════
        private void BuildSheet1(IXLWorkbook wb, ReportPageDto data,
                                  string factoryName, string periodLabel)
        {
            var ws = wb.Worksheets.Add("Tổng quan");
            ws.ShowGridLines = false;
            ws.Column(1).Width = 3;

            int r = 1;

            // Tiêu đề
            MergeTitle(ws, r, 2, 9,
                $"BÁO CÁO SCRAP — {factoryName.ToUpper()}",
                CHeaderBg, 15, height: 30);
            r++;

            MergeTitle(ws, r, 2, 9,
                $"Kỳ báo cáo: {periodLabel}  |  Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm}",
                CSubHdrBg, 10, height: 20);
            r++;

            ws.Row(r).Height = 8; r++;

            // KPI labels
            ws.Row(r).Height = 18;
            MergeCell(ws, r, 2, 4, "Tổng đơn Scrap", XLColor.FromHtml("#F2F2F2"), XLColor.FromHtml("#595959"), 9);
            MergeCell(ws, r, 5, 7, "Tổng khối lượng (kg)", XLColor.FromHtml("#F2F2F2"), XLColor.FromHtml("#595959"), 9);
            MergeCell(ws, r, 8, 9, "Nhà rác nhận (kg)", XLColor.FromHtml("#F2F2F2"), XLColor.FromHtml("#595959"), 9);
            r++;

            // KPI values
            ws.Row(r).Height = 36;
            var s = data.Summary;
            MergeCellValue(ws, r, 2, 4, (double)s.TotalOrders, "#,##0", CHeaderBg, CHeaderFg, 18);
            MergeCellValue(ws, r, 5, 7, (double)s.TotalWeight, "#,##0.0", CHeaderBg, CHeaderFg, 18);
            MergeCellValue(ws, r, 8, 9, (double)s.ConfirmedWeight, "#,##0.0", CHeaderBg, CHeaderFg, 18);
            r++;

            // Confirm rate bar
            ws.Row(r).Height = 20;
            var crr = ws.Range(r, 2, r, 9);
            crr.Merge();
            crr.Value =
                $"Tỉ lệ xác nhận: {s.ConfirmRate}%   |   " +
                $"Đơn chờ duyệt: {s.PendingOrders}   |   " +
                $"Chờ quá 3 ngày: {s.OverduePendingOrders} đơn";
            crr.Style.Font.Bold = true;
            crr.Style.Font.FontSize = 10;
            crr.Style.Font.FontColor = CGreen;
            crr.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF3DE");
            crr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            crr.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(crr);
            r++;

            ws.Row(r).Height = 8; r++;

            // Section header
            ws.Row(r).Height = 20;
            MergeTitle(ws, r, 2, 9, "BÁO CÁO THEO PHÒNG BAN", CSubHdrBg, 11, height: 20);
            r++;

            // Table headers
            string[] hDept = { "Phòng ban", "Tổng Số đơn","Tổng số đã Duyệt","Tổng số từ chối","Tổng số chờ","Tổng số hoàn thành", "Số lượng",
                                "KL Đăng ký (kg)", "KL Nhà rác nhận (kg)" };
            int[] cDept = { 2, 3, 4, 5, 6, 7, 8,9,10 };
            double[] wDept = { 22, 10, 20, 20, 20, 20, 12, 20, 24};
            for (int i = 0; i < hDept.Length; i++)
            {
                Th(ws, r, cDept[i], hDept[i]);
                ws.Column(cDept[i]).Width = wDept[i];
            }
            ws.Row(r).Height = 22;
            int deptStart = r + 1;
            r++;

            // Data rows
            for (int i = 0; i < data.DepartmentReport.Count; i++)
            {
                var d = data.DepartmentReport[i];
                bool alt = i % 2 == 1;
                ws.Row(r).Height = 18;

                Td(ws, r, 2, d.DepartmentName, alt, bold: true);
                TdNum(ws, r, 3, d.TotalOrders, alt, "#,##0");
                TdNum(ws, r, 4, d.TotalOrderApproval, alt, "#,##0");
                TdNum(ws, r, 5, d.TotalOrderReject, alt, "#,##0");
                TdNum(ws, r, 6, d.TotalOrderPending, alt, "#,##0");
                TdNum(ws, r, 7, d.TotalOrderDone, alt, "#,##0");

                TdNum(ws, r, 8, d.TotalQuantity, alt, "#,##0");
                TdNum(ws, r, 9, d.TotalWeight, alt, "#,##0.0");
                TdNum(ws, r, 10, d.ConfirmedWeight, alt, "#,##0.0");
                //// Tỉ lệ xác nhận — formula
                //var rc = ws.Cell(r, 10);
                //rc.FormulaA1 = $"=IFERROR(J{r}/I{r},0)";
                //rc.Style.NumberFormat.Format = "0.0%";
                //rc.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
                //rc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                //rc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //rc.Style.Font.FontName = "Arial";
                //rc.Style.Font.FontSize = 10;
                //Border(rc.AsRange());

                //// Trạng thái
                //var sc = ws.Cell(r, 11);
                //sc.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
                //sc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //sc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //sc.Style.Font.FontName = "Arial";
                //sc.Style.Font.FontSize = 10;
                //sc.Style.Font.Bold = true;
                //Border(sc.AsRange());

                r++;
            }

            // Total row
            ws.Row(r).Height = 20;
            int last = r - 1;
            TotalRow(ws, r, 2, 8);
            ws.Cell(r, 2).Value = "TỔNG CỘNG";
            ws.Cell(r, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            TotalFormula(ws, r, 3, $"=SUM(C{deptStart}:C{last})", "#,##0");
            TotalFormula(ws, r, 4, $"=SUM(D{deptStart}:D{last})", "#,##0");
            TotalFormula(ws, r, 5, $"=SUM(E{deptStart}:E{last})", "#,##0");
            TotalFormula(ws, r, 6, $"=SUM(F{deptStart}:F{last})", "#,##0");
            TotalFormula(ws, r, 7, $"=SUM(G{deptStart}:G{last})", "#,##0");

            TotalFormula(ws, r, 8, $"=SUM(H{deptStart}:H{last})", "#,##0");
            TotalFormula(ws, r, 9, $"=SUM(I{deptStart}:I{last})", "#,##0.0");
            TotalFormula(ws, r, 10, $"=SUM(J{deptStart}:J{last})", "#,##0.0");
            //TotalFormula(ws, r, 11, $"=IFERROR(J{r}/I{r},0)", "0.0%");
        }

        // ═══════════════════════════════════════════════════════════════════
        // SHEET 2 — VẬT LIỆU
        // ═══════════════════════════════════════════════════════════════════
        private void BuildSheet2(IXLWorkbook wb, ReportPageDto data,
                                  string factoryName, string periodLabel)
        {
            var ws = wb.Worksheets.Add("Vật liệu");
            ws.ShowGridLines = false;
            ws.Column(1).Width = 3;

            int r = 1;

            MergeTitle(ws, r, 2, 9,
                $"BÁO CÁO VẬT LIỆU — {factoryName.ToUpper()} — {periodLabel.ToUpper()}",
                CHeaderBg, 13, height: 28);
            r++;

            ws.Row(r).Height = 8; r++;

            string[] hMat = { "Mã vật liệu", "Tên vật liệu", "Loại", "ĐVT",
                               "Số lượng", "KL Đăng ký (kg)", "KL Nhà rác nhận (kg)", "Chênh lệch (kg)" };
            double[] wMat = { 14, 24, 14, 8, 12, 20, 24, 18 };
            for (int i = 0; i < hMat.Length; i++)
            {
                Th(ws, r, i + 2, hMat[i]);
                ws.Column(i + 2).Width = wMat[i];
            }
            ws.Row(r).Height = 22;
            int matStart = r + 1;
            r++;

            for (int i = 0; i < data.MaterialReport.Count; i++)
            {
                var m = data.MaterialReport[i];
                bool alt = i % 2 == 1;
                ws.Row(r).Height = 18;

                Td(ws, r, 2, m.Code, alt);
                Td(ws, r, 3, m.Name, alt, bold: true);
                Td(ws, r, 4, m.Type, alt);
                Td(ws, r, 5, m.Unit, alt, align: XLAlignmentHorizontalValues.Center);
                TdNum(ws, r, 6, m.TotalQuantity, alt, "#,##0");
                TdNum(ws, r, 7, m.TotalWeight, alt, "#,##0.0");
                TdNum(ws, r, 8, m.ConfirmedWeight, alt, "#,##0.0");

                // Chênh lệch formula
                var dc = ws.Cell(r, 9);
                dc.FormulaA1 = $"=H{r}-G{r}";
                dc.Style.NumberFormat.Format = @"#,##0.0;[Red]-#,##0.0";
                dc.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
                dc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                dc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dc.Style.Font.FontName = "Arial";
                dc.Style.Font.FontSize = 10;
                Border(dc.AsRange());

                r++;
            }

            int last = r - 1;
            ws.Row(r).Height = 20;
            TotalRow(ws, r, 2, 9);
            ws.Cell(r, 2).Value = "TỔNG CỘNG";
            ws.Cell(r, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Cột 3,4,5 chỉ fill màu
            for (int c = 3; c <= 5; c++)
            {
                ws.Cell(r, c).Style.Fill.BackgroundColor = CTotalBg;
                Border(ws.Cell(r, c).AsRange());
            }
            TotalFormula(ws, r, 6, $"=SUM(F{matStart}:F{last})", "#,##0");
            TotalFormula(ws, r, 7, $"=SUM(G{matStart}:G{last})", "#,##0.0");
            TotalFormula(ws, r, 8, $"=SUM(H{matStart}:H{last})", "#,##0.0");
            TotalFormula(ws, r, 9, $"=SUM(I{matStart}:I{last})", "#,##0.0");
        }

        // ═══════════════════════════════════════════════════════════════════
        // SHEET 3 — XU HƯỚNG + PIPELINE
        // ═══════════════════════════════════════════════════════════════════
        private void BuildSheet3(IXLWorkbook wb, ReportPageDto data,
                                  string factoryName, string periodLabel)
        {
            var ws = wb.Worksheets.Add("Xu hướng & Phê duyệt");
            ws.ShowGridLines = false;
            ws.Column(1).Width = 3;

            int r = 1;

            MergeTitle(ws, r, 2, 8,
                $"XU HƯỚNG SCRAP — {factoryName.ToUpper()} — {periodLabel.ToUpper()}",
                CHeaderBg, 13, height: 28);
            r++;
            ws.Row(r).Height = 8; r++;

            string[] hT = { "Kỳ", "Số đơn", "KL Đăng ký (kg)", "KL Nhà rác nhận (kg)" };
            double[] wT = { 12, 12, 22, 26 };
            for (int i = 0; i < hT.Length; i++)
            {
                Th(ws, r, i + 2, hT[i], CSubHdrBg);
                ws.Column(i + 2).Width = wT[i];
            }
            ws.Row(r).Height = 22;
            r++;

            foreach (var t in data.Trend.Select((v, i) => (v, i)))
            {
                bool alt = t.i % 2 == 1;
                ws.Row(r).Height = 18;
                Td(ws, r, 2, t.v.Label, alt, align: XLAlignmentHorizontalValues.Center);
                TdNum(ws, r, 3, t.v.TotalOrders, alt, "#,##0");
                TdNum(ws, r, 4, t.v.TotalWeight, alt, "#,##0.0");
                TdNum(ws, r, 5, t.v.ConfirmedWeight, alt, "#,##0.0");
                r++;
            }

            ws.Row(r).Height = 10; r++;

            // Thống kê phê duyệt
            MergeTitle(ws, r, 2, 5, "THỐNG KÊ PHÊ DUYỆT", CSubHdrBg, 11, height: 20);
            r++;

            Th(ws, r, 2, "Trạng thái");
            Th(ws, r, 3, "Số đơn");
            Th(ws, r, 4, "Tỉ lệ");
            ws.Row(r).Height = 20;
            int statStart = r + 1;
            r++;

            int total = data.ApprovalStatus.Approved + data.ApprovalStatus.Pending + data.ApprovalStatus.Rejected;
            var statRows = new[]
            {
                ("Đã duyệt",   data.ApprovalStatus.Approved, CGreen),
                ("Đang duyệt", data.ApprovalStatus.Pending,  COrange),
                ("Từ chối",    data.ApprovalStatus.Rejected, CRed),
            };

            foreach (var (label, val, color) in statRows)
            {
                ws.Row(r).Height = 18;

                var lc = ws.Cell(r, 2);
                lc.Value = label;
                lc.Style.Fill.BackgroundColor = XLColor.White;
                lc.Style.Font.Bold = true;
                lc.Style.Font.FontColor = color;
                lc.Style.Font.FontName = "Arial";
                lc.Style.Font.FontSize = 10;
                lc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                lc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Border(lc.AsRange());

                var vc = ws.Cell(r, 3);
                vc.Value = val;
                vc.Style.Fill.BackgroundColor = XLColor.White;
                vc.Style.NumberFormat.Format = "#,##0";
                vc.Style.Font.FontName = "Arial";
                vc.Style.Font.FontSize = 10;
                vc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                vc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Border(vc.AsRange());

                var pc = ws.Cell(r, 4);
                pc.FormulaA1 = total == 0 ? "=0" : $"=C{r}/{total}";
                pc.Style.NumberFormat.Format = "0.0%";
                pc.Style.Fill.BackgroundColor = XLColor.White;
                pc.Style.Font.FontName = "Arial";
                pc.Style.Font.FontSize = 10;
                pc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                pc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Border(pc.AsRange());

                r++;
            }

            ws.Row(r).Height = 10; r++;

            // Pipeline
            MergeTitle(ws, r, 2, 8, "PIPELINE PHÊ DUYỆT (20 ĐƠN GẦN NHẤT)", CSubHdrBg, 11, height: 20);
            r++;

            string[] hPl = { "Tiêu đề đơn", "Phòng ban","Người duyệt", "Ngày tạo", "Trạng thái" };
            double[] wPl = { 32, 16,18, 22, 14 };
            for (int i = 0; i < hPl.Length; i++)
            {
                Th(ws, r, i + 2, hPl[i]);
                ws.Column(i + 2).Width = wPl[i];
            }
            ws.Row(r).Height = 22;
            r++;

            foreach (var p in data.Pipeline.Select((v, i) => (v, i)))
            {
                bool alt = p.i % 2 == 1;
                ws.Row(r).Height = 18;

                Td(ws, r, 2, p.v.Title, alt, bold: true);
                Td(ws, r, 3, p.v.DepartmentName, alt, align: XLAlignmentHorizontalValues.Center);
                Td(ws, r, 5, p.v.DecisionMakerName, alt);
                Td(ws, r, 6, p.v.CreatedDate.ToString("dd/MM/yyyy HH:mm"), alt, align: XLAlignmentHorizontalValues.Center);

                var statusText = p.v.Status switch
                {
                    StatusScrap.Approved => "Đã duyệt",
                    StatusScrap.Pending => "Chờ duyệt",
                    StatusScrap.Rejected => "Từ chối",
                    _ => p.v.Status.ToString()
                };
                var statusColor = p.v.Status switch
                {
                    StatusScrap.Approved => CGreen,
                    StatusScrap.Pending => COrange,
                    StatusScrap.Rejected => CRed,
                    _ => XLColor.Black
                };

                var sc = ws.Cell(r, 7);
                sc.Value = statusText;
                sc.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
                sc.Style.Font.Bold = true;
                sc.Style.Font.FontColor = statusColor;
                sc.Style.Font.FontName = "Arial";
                sc.Style.Font.FontSize = 10;
                sc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                sc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Border(sc.AsRange());

                r++;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // HELPER — không dùng fluent chain trên IXLStyle
        // ═══════════════════════════════════════════════════════════════════

        private void MergeTitle(IXLWorksheet ws, int row, int c1, int c2,
                                  string text, XLColor bg, double fontSize, double height = 28)
        {
            var rng = ws.Range(row, c1, row, c2);
            rng.Merge();
            rng.Value = text;
            rng.Style.Font.Bold = true;
            rng.Style.Font.FontSize = fontSize;
            rng.Style.Font.FontColor = XLColor.White;
            rng.Style.Font.FontName = "Arial";
            rng.Style.Fill.BackgroundColor = bg;
            rng.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rng.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(rng);
            ws.Row(row).Height = height;
        }

        private void MergeCell(IXLWorksheet ws, int row, int c1, int c2,
                                string text, XLColor bg, XLColor fg, double fontSize)
        {
            var rng = ws.Range(row, c1, row, c2);
            rng.Merge();
            rng.Value = text;
            rng.Style.Font.FontSize = fontSize;
            rng.Style.Font.FontColor = fg;
            rng.Style.Font.FontName = "Arial";
            rng.Style.Fill.BackgroundColor = bg;
            rng.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rng.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(rng);
        }

        private void MergeCellValue(IXLWorksheet ws, int row, int c1, int c2,
                                     double value, string fmt, XLColor bg, XLColor fg, double fontSize)
        {
            var rng = ws.Range(row, c1, row, c2);
            rng.Merge();
            rng.Value = value;
            rng.Style.NumberFormat.Format = fmt;
            rng.Style.Font.Bold = true;
            rng.Style.Font.FontSize = fontSize;
            rng.Style.Font.FontColor = fg;
            rng.Style.Font.FontName = "Arial";
            rng.Style.Fill.BackgroundColor = bg;
            rng.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rng.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(rng);
        }

        private void Th(IXLWorksheet ws, int row, int col, string text, XLColor? bg = null)
        {
            var c = ws.Cell(row, col);
            c.Value = text;
            c.Style.Font.Bold = true;
            c.Style.Font.FontSize = 10;
            c.Style.Font.FontColor = XLColor.White;
            c.Style.Font.FontName = "Arial";
            c.Style.Fill.BackgroundColor = bg ?? CHeaderBg;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            c.Style.Alignment.WrapText = true;
            Border(c.AsRange());
        }

        private void Td(IXLWorksheet ws, int row, int col, string text, bool alt,
                         bool bold = false,
                         XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Left)
        {
            var c = ws.Cell(row, col);
            c.Value = text;
            c.Style.Font.FontName = "Arial";
            c.Style.Font.FontSize = 10;
            c.Style.Font.Bold = bold;
            c.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
            c.Style.Alignment.Horizontal = align;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(c.AsRange());
        }

        // Nhận object, cast nội bộ → tránh ambiguous giữa int/decimal/double
        private void TdNum(IXLWorksheet ws, int row, int col, object value, bool alt, string fmt)
        {
            var c = ws.Cell(row, col);
            c.Value = Convert.ToDouble(value);
            c.Style.NumberFormat.Format = fmt;
            c.Style.Font.FontName = "Arial";
            c.Style.Font.FontSize = 10;
            c.Style.Fill.BackgroundColor = alt ? CAltRow : XLColor.White;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(c.AsRange());
        }

        private void TotalRow(IXLWorksheet ws, int row, int colFrom, int colTo)
        {
            var rng = ws.Range(row, colFrom, colTo, colTo);
            for (int c = colFrom; c <= colTo; c++)
            {
                var cell = ws.Cell(row, c);
                cell.Style.Fill.BackgroundColor = CTotalBg;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = CTotalFg;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Font.FontSize = 10;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Border(cell.AsRange());
            }
        }

        private void TotalFormula(IXLWorksheet ws, int row, int col, string formula, string fmt)
        {
            var c = ws.Cell(row, col);
            c.FormulaA1 = formula;
            c.Style.NumberFormat.Format = fmt;
            c.Style.Fill.BackgroundColor = CTotalBg;
            c.Style.Font.Bold = true;
            c.Style.Font.FontColor = CTotalFg;
            c.Style.Font.FontName = "Arial";
            c.Style.Font.FontSize = 10;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Border(c.AsRange());
        }

        // Border — truy cập trực tiếp sub-property, không dùng fluent chain trên IXLStyle
        private void Border(IXLRange rng)
        {
            rng.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.OutsideBorderColor = CBorder;
            rng.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.InsideBorderColor = CBorder;
        }
    }
}
