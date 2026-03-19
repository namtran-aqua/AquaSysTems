using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.SemiReport
{
    public class SemiReportDto
    {
        public string InnerBarcode { get; set; } = string.Empty;
        public DateTime ScanTimeInner { get; set; }
        public string? ScrapBarcode { get; set; }
        public string? ScrapDescription { get; set; }
        public DateTime? ScanTimeScrap { get; set; }
        public string? OuterBarcode { get; set; }
        public DateTime? ScanTimeOuter { get; set; }
        public string? MotorBarcode { get; set; }
        public DateTime? ScanTimeMotor { get; set; }
        public string? Model { get; set; }
    }
}
