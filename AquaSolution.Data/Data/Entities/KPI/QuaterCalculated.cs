using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class QuaterCalculated
    {
        public Guid Id { get; set; }
        public string Calculated { get; set; }
        public KPIQuarterCalculateType KPIQuarterCalculateType { get; set; }
    }
}
