using System;
using System.Collections.Generic;

namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class SpecialPeriodYearResultDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int Year { get; set; }

        public bool IsEligible { get; set; }
        public string? ErrorMessage { get; set; }

        public List<int> ApprovedMonths { get; set; } = new();
        public List<int> EffectiveExemptMonths { get; set; } = new();
        public List<int> InvalidMonths { get; set; } = new();

        // Chi tiết từng tháng (để hiển thị bảng trực quan trên UI)
        public List<MonthScoreSummaryItem> MonthSummaries { get; set; } = new();

        // 1. Quý 4 (Q4)
        public bool CanCalculateQ4 { get; set; }
        public string Q4Reason { get; set; } = string.Empty;
        public KPITotalScoreDto? Q4Score { get; set; }

        // 2. Bán niên 2 (H2)
        public bool CanCalculateH2 { get; set; }
        public string H2Reason { get; set; } = string.Empty;
        public KPITotalScoreDto? H2Score { get; set; }

        // 3. Năm (Year)
        public bool CanCalculateYear { get; set; }
        public string YearReason { get; set; } = string.Empty;
        public KPITotalScoreDto? YearScore { get; set; }
    }

    public class MonthScoreSummaryItem
    {
        public int Month { get; set; }
        public string StatusName { get; set; } = string.Empty; // "Đã duyệt", "Được miễn", "Chưa có điểm"
        public bool IsApproved { get; set; }
        public bool IsExempt { get; set; }
        public decimal? Score { get; set; }
        public string? Note { get; set; }
    }

    public class SubmitSpecialPeriodYearDto
    {
        public Guid UserId { get; set; }
        public int Year { get; set; }
        public bool IncludeQ4 { get; set; }
        public bool IncludeH2 { get; set; }
        public bool IncludeYear { get; set; } = true;
        public KPITotalScoreDto? Q4Score { get; set; }
        public KPITotalScoreDto? H2Score { get; set; }
        public KPITotalScoreDto YearScore { get; set; } = new();
    }
}
