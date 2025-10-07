using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class Formula
    {
        public Guid Id { get; set; }
        public string FormulaName { get; set; }
        public string Description { get; set; }
        public KPIFormulaType KPIFormulaType { get; set; }
    }
}
