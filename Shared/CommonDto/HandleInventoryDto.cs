using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.CommonDto
{
    public class HandleInventoryDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
    }
}
