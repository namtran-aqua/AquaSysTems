using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.FlowApprovals
{
    public class FlowApprovalResponse
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? SectionId { get; set; }
        public string? SectionName { get; set; }
        public Guid FactoryId { get; set; }
        public string FactoryName { get; set; } = string.Empty;
        public List<FlowApprovalScrapDto> Steps { get; set; } = new();
    }
}
