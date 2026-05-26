using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.FlowApprovals
{
    public class FlowApprovalScrapDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DecisionMaker { get; set; }
        public string DecisionMakerName { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public string DecisionMakerWorkId { get; set; } = string.Empty;  // thêm
        public string DecisionMakerEmail { get; set; } = string.Empty;
        public int Step { get; set; }
    }
}
