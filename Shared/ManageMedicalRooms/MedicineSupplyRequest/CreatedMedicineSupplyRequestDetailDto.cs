using AquaSolution.Shared.ManageMedicalRooms.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest
{
    public class CreatedMedicineSupplyRequestDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public DateTime? DateManufactured { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal? QuantityIssued { get; set; }
        public string? Note { get; set; }
        private ProductExportDto product = new();
        public ProductExportDto Product
        {
            get => product;
            set
            {
                if (product != value)
                {
                    product = value;
                    if (value != null)
                    {

                        ProductId = product.Id;
                        DateManufactured = product.ManufacturingDate;
                        ExpiryDate = product.ExpirationDate;
                      
                    }

                }
            }
        }
    }
}
