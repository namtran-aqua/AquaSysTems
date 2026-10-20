using System;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Shared.KPI.Scope
{
    public class KPIComplianceMonthDto
    {
        public int Month { get; set; }
        public MonthKPIStatus Status { get; set; }
        public string StatusName { get; set; }
        public double? TotalScore { get; set; }
        public Guid? TaskSubmitedId { get; set; }
        public string Note { get; set; }
    }
}
