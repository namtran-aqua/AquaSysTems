namespace AquaSolution.Shared.KPI.KPIActual
{
    public class YearDto
    {
        public int Year { get; set; }
        public decimal? KPIScore { get; set; }
        public decimal? KeyTaskScore { get; set; }
        public decimal? OMGScore { get; set; }
        public decimal? TotalScore { get; set; }
        public List<HalfYearDto> HalfYears { get; set; } = new List<HalfYearDto>();
    }
}
