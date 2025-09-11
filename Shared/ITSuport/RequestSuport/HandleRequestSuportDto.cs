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
    public class HandleRequestSuportDto
    {
        public Guid Id { get; set; }
        public string RequestTitle { get; set; }
        public Guid RequestSuportCategoryId { get; set; }
        public RequestSuportStatusType Status { get; set; }
        public Guid RequestBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequestDescription { get; set; } = string.Empty;
        public string? RequestSolution { get; set; }
        public Guid? TechnicianId { get; set; }
        public DateTime? InProgessDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? OnHoldDate { get; set; }
        public List<AttachmentDto>? Attachments { get; set; }
    }
}
