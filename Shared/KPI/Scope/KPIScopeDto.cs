using System;
using System.Collections.Generic;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Shared.KPI.Scope
{
    public class KPIScopeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string DepartmentName { get; set; }
        public KPIScopeType ScopeType { get; set; }
        public string ScopeTypeName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<KPIExemptMonthDto> ExemptMonths { get; set; } = new();
    }
}
