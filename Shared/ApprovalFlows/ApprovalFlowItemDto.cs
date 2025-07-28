using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ApprovalFlows
{
    public class ApprovalFlowItemDto
    {
        public int CurrentStep { get; set; }
        public int NextStep { get; set; }
        public string? UserApproveName { get; set; } 
        public ApprovalSettingType ApprovalSettingType { get; set; }
    }

}
