using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ITSuport.RequestSuportCategory
{
    public class RequestSuportCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
