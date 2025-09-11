using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities
{
    public class RequestSuport
    {
        public Guid Id { get; set; }
        public string RequestTitle { get; set; }    
        public Guid RequestSuportCategoryId { get; set; }
        public RequestSuportStatusType Status { get; set; }
        public Guid RequestBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequestDescription { get; set; } =string.Empty;
        public string? RequestSolution { get; set; } 
        public Guid? TechnicianId { get; set; }
        public DateTime? InProgessDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? OnHoldDate { get; set; }


    }
}
