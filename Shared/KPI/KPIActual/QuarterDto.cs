using AquaSolution.Shared.KPI.KPISubmit;

namespace AquaSolution.Shared.KPI.KPIActual
{
    public class QuarterDto
    {
        public int Quarter { get; set; } 

        public decimal? KPIScore { get; set; }
        public decimal? KeyTaskScore { get; set; }
        public decimal? OMGScore { get; set; }
        public decimal? TotalScore { get; set; }

        public List<HandleKPISubmitDto> Months { get; set; } = new List<HandleKPISubmitDto>();
    }
}
