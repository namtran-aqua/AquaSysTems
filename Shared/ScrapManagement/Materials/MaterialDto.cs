using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Materials
{

    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string BOMHead { get; set; }
        public string BOMDescription { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TYPE { get; set; }
        public string Unit { get; set; }
        public PlantType Plant { get; set; }
        public Guid? WeightId { get; set; }
        public decimal? WeightValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
    }

}
