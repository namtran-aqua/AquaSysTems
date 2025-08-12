using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.Prescriptions
{
    public class PrescriptionDetailDto
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public decimal Quantity { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
    }
}
