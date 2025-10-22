using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class KPITask
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }=string.Empty;
        public KPICategoryType KPICategory { get; set; }
        public string TaskDescription { get; set; }
        public string CalculatedMdethod { get; set; }
        public string DataSource { get; set; }
        public string Unit { get; set; }
        public Guid OwnerId { get; set; }
        public KPIIndexType KPIIndexType { get; set; }
        public Guid FormulaId { get; set; }
        public decimal Max { get; set; }
        public decimal Bottom { get; set; }
        public Guid FactoryId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
