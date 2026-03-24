using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class InventoryPeriodDetail
    {
        public Guid InventoryId { get; set; }
        public Guid InventoryPeriodId { get; set; }
        public Guid ProductId { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal SystemQuantity { get; set; } 
        public decimal? RemainingValidity { get; set; }
        public DateTime? DateManufacture { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductType ProductType { get; set; }
    }

}
