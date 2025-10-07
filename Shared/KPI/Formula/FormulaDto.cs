using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.Formula
{
    public class FormulaDto
    {
        public Guid Id { get; set; }
        public string FormulaName { get; set; }
        public string Description { get; set; }
        public KPIFormulaType KPIFormulaType { get; set; }
    }
}
