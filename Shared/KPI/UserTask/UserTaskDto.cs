using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.UserTask
{
    public class UserTaskDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid KPITaskId { get; set; }
        public bool IsActive { get; set; }
        public int? Index { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
