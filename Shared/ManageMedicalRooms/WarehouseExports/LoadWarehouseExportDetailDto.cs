using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseExports
{
    public class LoadWarehouseExportDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode{ get; set; }
        public string Unit { get; set; }

        public DateTime? DateManufacture { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public ProductType ProductType { get; set; }

    }
}
