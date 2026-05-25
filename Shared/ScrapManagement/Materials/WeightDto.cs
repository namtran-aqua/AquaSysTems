using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Materials
{
    public class WeightDto
    {
        public Guid MaterialId { get; set; }
        public Guid? WeightId { get; set; }
        public decimal? WeightValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
