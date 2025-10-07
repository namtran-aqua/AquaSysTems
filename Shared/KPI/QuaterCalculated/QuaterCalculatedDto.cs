using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.QuaterCalculated
{
    public class QuaterCalculatedDto
    {
        public Guid Id { get; set; }
        public string Calculated { get; set; }
        public KPIQuarterCalculateType KPIQuarterCalculateType { get; set; }
    }
}
