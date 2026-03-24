using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.RequestITSuports
{
    public class RequestSuportCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? TechnicianId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
