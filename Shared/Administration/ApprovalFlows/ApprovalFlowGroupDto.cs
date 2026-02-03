using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ApprovalFlows
{
    public class ApprovalFlowGroupDto
    {
        public int? FlowApproval { get; set; }
        public List<ApprovalFlowDto> Items { get; set; } = new();
    }
}
