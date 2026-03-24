namespace AquaSolution.Data.KPI.Entities
{
    public class CeilingLevel
    {
        public Guid Id { get; set; }
        public Guid FactoryId { get; set; }
        public decimal CeilingLevelValue { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
