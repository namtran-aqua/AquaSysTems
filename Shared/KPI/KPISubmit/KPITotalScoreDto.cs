using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class KPITotalScoreDto
    {
        public decimal KPIScore { get; set; }
        public decimal KeyTaskScore { get; set; }
        public decimal OMGScore { get; set; }
        public Guid CreatedBy { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }
        public StatusKPIRequestType Status { get; set; }
        public decimal TotaleScore { get; set; }
        public bool IsActive { get; set; }
    }
}
