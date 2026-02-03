using AquaSolution.Shared.Enum;
using System.Text.Json.Serialization;


namespace AquaSolution.Shared.ApprovalFlows
{
    public class ApprovalFlowDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? DecisionMaker { get; set; }
        public string? DecisionMakerName { get; set; } 
        public int? FlowApproval { get; set; } 
        public string? DesCription { get; set; }
        public int? CurrentStep { get; set; }
        public int? NextStep { get; set; }
        public ApprovalSettingType? ApprovalSettingType { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
