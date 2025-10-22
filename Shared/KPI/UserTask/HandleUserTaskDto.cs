using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.UserTask
{
    public class HandleUserTaskDto
    {
        public Guid UserId { get; set; }
        public Guid TaskIds { get; set; } 
        public decimal Weight { get; set; }
        public int? Index { get; set; }
    }
}
