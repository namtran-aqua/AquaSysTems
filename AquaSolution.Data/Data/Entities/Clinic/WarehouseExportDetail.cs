using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class WarehouseExportDetail
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseExportId { get; set; }
        public DateTime? DateManufacture { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public ProductType ProductType { get; set; }
    }
}
