using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.Prescriptions
{
    public class CreatedPrescriptionDetailDto
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime? DateManufacture { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductType ProductType { get; set; }
        //public ProductExportDto productDto { get; set; } = new();
    }
}
