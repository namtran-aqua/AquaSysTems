using AquaSolution.Shared.Enum;

namespace AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod
{
    public class InventoryPeriodDetailDto
    {
        public Guid InventoryId { get; set; }
        public Guid InventoryPeriodId { get; set; }
        public Guid ProductId { get; set; }
        public string? Unit { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal? ActualQuantity { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal? RemainingValidity { get; set; } 
        public DateTime? DateManufacture { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductType ProductType { get; set; }
    }
}
