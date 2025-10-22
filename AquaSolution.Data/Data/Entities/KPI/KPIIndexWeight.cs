using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class KPIIndexWeight
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public PeriodType PeriodType { get; set; }
        public PositionType PositionType { get; set; }
        public KPIIndexType KPIIndexType { get; set; }
    }
}
