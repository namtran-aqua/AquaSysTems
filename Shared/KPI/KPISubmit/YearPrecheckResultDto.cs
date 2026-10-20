using System;
using System.Collections.Generic;

namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class YearPrecheckResultDto
    {
        public Guid UserId { get; set; }
        public int Year { get; set; }
        public bool IsEligible { get; set; } // True nếu cả 11 tháng trước đều đã Approved hoặc Exempt
        public List<int> InvalidMonths { get; set; } = new(); // Danh sách tháng thiếu cần thông báo
        public List<int> ApprovedMonths { get; set; } = new(); // Các tháng có điểm đã duyệt (T1..T11)
        public List<int> EffectiveExemptMonths { get; set; } = new(); // Các tháng miễn thực tế (đã loại trừ tháng có điểm)
        
        // Dữ liệu điểm Năm tính toán trước từ Backend
        public bool IsFullYearScenario { get; set; } // True = Kịch bản 1 (đủ 12 tháng điểm), False = Kịch bản 2 (có tháng miễn)
        public KPITotalScoreDto ProjectedYearScore { get; set; } = new();
        public List<HandleActualDto> ProjectedYearDetails { get; set; } = new();
    }
}
