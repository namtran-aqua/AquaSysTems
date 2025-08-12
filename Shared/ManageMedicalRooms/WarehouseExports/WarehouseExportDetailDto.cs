using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseExports
{
    public class WarehouseExportDetailDto
    {
        public Guid Id { get; set; }
        public decimal Quantity { get; set; }
        public ProductExportDto productDto { get; set; } = new();
    }
}
