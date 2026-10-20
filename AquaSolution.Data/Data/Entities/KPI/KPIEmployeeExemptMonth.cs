using System;

namespace AquaSolution.Data.Data.Entities.KPI
{
    public class KPIEmployeeExemptMonth
    {
        public Guid Id { get; set; }
        public Guid ScopeId { get; set; }
        public Guid UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public KPIEmployeeScope? Scope { get; set; }
    }
}
