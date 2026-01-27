using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPIMonthlyTarget
{
    public class IndexWeightDto
    {
        public Guid TaskId { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int Year { get; set; }
        public int? Index { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UserId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public KPIIndexType KPIIndexType { get; set; }
        public KPICategoryType KPICategory { get; set; }
        public decimal Max { get; set; }
        public decimal Bottom { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; }
        public string PIC { get; set; }
        public string DataSource { get; set; }
        public string Formula { get; set; }
        public string Calculated { get; set; }

        public string Factory { get; set; }
        public string Department { get; set; }
        public decimal TargetValue1 { get; set; }
        public decimal TargetValue2 { get; set; }
        public decimal TargetValue3 { get; set; }
        public decimal TargetValue4 { get; set; }
        public decimal TargetValue5 { get; set; }
        public decimal TargetValue6 { get; set; }
        public decimal TargetValue7 { get; set; }
        public decimal TargetValue8 { get; set; }
        public decimal TargetValue9 { get; set; }
        public decimal TargetValue10 { get; set; }
        public decimal TargetValue11 { get; set; }
        public decimal TargetValue12 { get; set; }
        public decimal TargetQarter1 { get; set; }
        public decimal TargetQarter2 { get; set; }
        public decimal TargetQarter3 { get; set; }
        public decimal TargetQarter4 { get; set; }
        public decimal TargetHaftYear1 { get; set; }
        public decimal TargetHaftYear2 { get; set; }
        public decimal TargetYear { get; set; }

        public QuarterCalculateType QuarterCalculateType { get; set; }
    }
}
