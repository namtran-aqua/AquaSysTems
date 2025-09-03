using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseExports
{
    public class WarehouseExportDetailDto
    {
        public Guid Id { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime? DateManufacture { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductType ProductType { get; set; }
    }
}
