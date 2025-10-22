using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPIMonthlyTarget
{
    public class UpdateTargetDto
    {
        public Guid TaskId { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int Year { get; set; }
        public decimal TargetValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UserId { get; set; }
    }
}
