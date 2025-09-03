using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;


namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseImports
{
    public class WarehouseImportDetailDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime? DateManufacture { get; set; }
        public Guid ProductId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public ProductType ProductType { get; set; }
        public string Unit { get; set; }
        //public ProductDto productDto 
        //{ get; set; } = new();
    }
}
