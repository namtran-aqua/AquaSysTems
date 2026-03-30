using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class KPITotalScore
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid SubmitId { get; set; }
        public decimal KPIScore { get; set; }
        public decimal KeyTaskScore { get; set; }
        public decimal OMGScore { get; set; }
        public Guid CreatedBy { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }    
        public int? HalfYear { get; set; }   
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }
        public StatusKPIRequestType Status { get; set; }
        public decimal? TotaleScore { get; set; }
        public decimal? TotalActualScore { get; set; }
        public bool IsActive { get; set; }
        public KPITotalScoreType kPITotalScoreType { get; set; }
    }
}
