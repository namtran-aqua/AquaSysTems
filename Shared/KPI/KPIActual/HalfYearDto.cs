using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPIActual
{
    public class HalfYearDto
    {
        public int HalfYear { get; set; } 
        public decimal? KPIScore { get; set; }
        public decimal? KeyTaskScore { get; set; }
        public decimal? OMGScore { get; set; }
        public decimal? TotalScore { get; set; }

        public List<QuarterDto> Quarters { get; set; } = new List<QuarterDto>();
    }
}
