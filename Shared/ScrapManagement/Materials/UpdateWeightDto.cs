using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Materials
{
    public class UpdateWeightDto
    {
        public Guid MaterialId { get; set; }
        public Guid WeightId { get; set; }
        public decimal WeightValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
