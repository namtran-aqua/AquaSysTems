namespace AquaSolution.Data.KPI.Entities
{
    public class UserTask
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid KPITaskId { get; set; }
        public decimal Weight { get; set; }
        public bool IsActive { get; set; }
        public int? Index { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
    }
}
