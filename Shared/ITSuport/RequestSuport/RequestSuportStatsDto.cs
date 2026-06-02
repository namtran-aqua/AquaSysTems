using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ITSuport.RequestSuport
{
    public class RequestSuportStatsDto
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
        public int Cancel { get; set; }
    }
}
