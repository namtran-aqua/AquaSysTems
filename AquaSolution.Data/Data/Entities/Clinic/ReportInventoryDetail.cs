using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class ReportInventoryDetail
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }=string.Empty;
        public Guid ReportInventoryId { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal BeginningInventory { get; set; }
        public decimal NewInbound { get; set; }
        public decimal ConsumPosition { get; set; }
        public decimal TotalStock { get; set; }
        public string Unit { get; set; }
    }
}
