using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdateBy { get; set; }
        public string Unit { get; set; }
        public bool IsHide { get; set; } 
    }
}
