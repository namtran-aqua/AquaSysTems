using System;
using System.Collections.Generic;

namespace AquaSolution.Shared.KPI.Scope
{
    public class KPIComplianceMatrixDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int Year { get; set; }
        public List<KPIComplianceMonthDto> Months { get; set; } = new();

        public Guid? PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int TotalExemptMonths { get; set; }
        public int TotalSubmittedMonths { get; set; }
        public int TotalApprovedMonths { get; set; }
        public int TotalPendingMonths { get; set; }
        public int TotalMissingMonths { get; set; }
        public int TotalOpenMonths { get; set; }

        public List<int> MissingMonthNumbers => Months
            .Where(m => m.Status == AquaSolution.Shared.Enum.KPIType.MonthKPIStatus.Missing)
            .Select(m => m.Month)
            .OrderBy(m => m)
            .ToList();
    }
}
