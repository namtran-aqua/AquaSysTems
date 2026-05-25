using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AquaSolution.Shared.Enum.Scrap;

namespace AquaSolution.Shared.ScrapManagement.Materials
{
    public class ImportExcelDto
    {
        // Material
        public string BOMHead { get; set; }
        public string BOMDescription { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TYPE { get; set; }
        public string Unit { get; set; }
        public PlantType? Plant { get; set; }

        // Weight
        public decimal WeightValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
