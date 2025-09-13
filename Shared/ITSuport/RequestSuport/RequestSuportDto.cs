using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ITSuport.RequestSuport
{
    public class RequestSuportDto
    {
        public Guid Id { get; set; }
        public string RequestTitle { get; set; } = string.Empty;
        public Guid RequestSuportCategoryId { get; set; }
        public string RequestSuportCategoryName { get; set; } = string.Empty;
        public RequestSuportStatusType Status { get; set; }
        public Guid RequestById { get; set; }
        public string RequestByName { get; set; } = string.Empty;
        public string RequestByEmail { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string RequestDescription { get; set; } = string.Empty;
        public string? RequestSolution { get; set; }
        public Guid? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public string? TechnicianEmail { get; set; }
        public DateTime? InProgessDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? Department { get; set; }
        public string? Factory { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedName { get; set; }
        public string CreatedEmail { get; set; }
    }
}
