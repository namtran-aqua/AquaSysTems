namespace AquaSolution.Data.Data.Entities
{
    public class KPIMonthlyActual
    {
        public Guid Id { get; set; }
        public Guid KPITargetId { get; set; }
        public Guid KPITotalScoreId { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int? Year { get; set; }
        public decimal? ActualValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
