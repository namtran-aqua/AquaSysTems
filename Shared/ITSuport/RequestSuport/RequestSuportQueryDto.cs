using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ITSuport.RequestSuport
{
    public class RequestSuportQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? RequesterName { get; set; }
        public string? RequesterEmail { get; set; }
        public int? TicketCode { get; set; }
        public Guid? CurrentUserId { get; set; }   // ← thêm vào
        public bool IsITOrAdmin { get; set; }
        public List<string>? Statuses { get; set; }
        public List<string>? TechnicianNames { get; set; }
    }
}
