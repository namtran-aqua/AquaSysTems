using System;
using System.Collections.Generic;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Shared.KPI.Scope
{
    public class KPIScopeCreateOrUpdateDto
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public KPIScopeType ScopeType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Danh sách các tháng miễn được IT chọn: mỗi phần tử có Year, Month, Note (optional)
        /// </summary>
        public List<ExemptMonthItem> ExemptMonths { get; set; } = new();
    }

    public class ExemptMonthItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Note { get; set; } = string.Empty;
    }
}
