using System;

namespace AquaSolution.Shared.KPI.Scope
{
    public class KPIExemptMonthDto
    {
        public Guid Id { get; set; }
        public Guid ScopeId { get; set; }
        public Guid UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
