using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;

namespace AquaSolution.Data.Data.Entities.KPI
{
    public class KPIEmployeeScope
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public KPIScopeType ScopeType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<KPIEmployeeExemptMonth> ExemptMonths { get; set; } = new List<KPIEmployeeExemptMonth>();
    }
}
