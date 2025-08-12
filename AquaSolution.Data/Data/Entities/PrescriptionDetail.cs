using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities
{
    public class PrescriptionDetail
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
    }
}
