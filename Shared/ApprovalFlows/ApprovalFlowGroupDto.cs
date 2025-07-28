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
        public string PositionName { get; set; } = string.Empty;
        public SystemType System { get; set; }
        public List<ApprovalFlowItemDto> Items { get; set; } = new();
    }
}
