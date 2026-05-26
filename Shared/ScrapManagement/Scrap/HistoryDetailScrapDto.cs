using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class HistoryDetailScrapDto
    {
        public Guid Id { get; set; }

        public Guid ScrapHistoryId { get; set; }

        public Guid MaterialId { get; set; }

        public string BOMHead { get; set; }

        public string BOMDescription { get; set; }

        public string TYPE { get; set; }

        public PlantType Plant { get; set; }

        public string Code { get; set; }

        public string Unit { get; set; }

        public string Name { get; set; }

        public decimal Quantity { get; set; }

        public decimal Weight { get; set; }
        public decimal TotalWeight { get; set; }
        public string? Reson { get; set; } = null;
    }
}
