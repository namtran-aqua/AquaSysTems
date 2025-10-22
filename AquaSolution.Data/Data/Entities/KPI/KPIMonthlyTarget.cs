namespace AquaSolution.Data.Data.Entities
{
    public class KPIMonthlyTarget
    {
        public Guid Id { get; set; }
        public Guid UserTaskId { get; set; }
        public int? Month { get; set; }  
        public int Year { get; set; }
        public decimal TargetValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
