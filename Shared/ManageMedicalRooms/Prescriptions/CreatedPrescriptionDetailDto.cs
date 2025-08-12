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
        public ProductExportDto productDto { get; set; } = new();
    }
}
