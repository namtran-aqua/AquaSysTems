using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest
{
    public class MedicineSupplyRequestDetailDto
    {
        public Guid Id { get; set; }
        public Guid MedicineSupplyRequestId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime? DateManufactured { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? RequestedQuantity { get; set; }
        public decimal? QuantityIssued { get; set; }
        public string? Note { get; set; }
        public ProductExportDto Product { get; set; }
    }
}
