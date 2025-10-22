using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class IndexWeightDto
    {
        public decimal Weight { get; set; }
        public PeriodType PeriodType { get; set; }
        public PositionType PositionType { get; set; }
        public KPIIndexType KPIIndexType { get; set; }
    }
}
