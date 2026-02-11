using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.SemiReport
{
    public class RollReportDto
    {
        public string? PackNo { get; set; } = string.Empty;
        public bool Result { get; set; }
        public string ResultText => Result ? "OK" : "NG";
        public DateTime? ScanTime { get; set; }
        public string? CusPackno { get; set; }
        public string? ModelCode { get; set; }
    }
}
