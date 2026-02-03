using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities
{
    public class ApprovalFlow
    {
        public Guid Id { get; set; }
        public int FlowApproval { get; set; }
        public string Name { get; set; }
        public Guid? DecisionMaker { get; set; }
        public string? DesCription { get; set; }
        public int? CurrentStep { get; set; }
        public int? NextStep { get; set; }
        public ApprovalSettingType ApprovalSettingType { get; set; }
        public DateTime CreatedDate { get; set; }
        public SystemType SystemType { get; set; }
    }
}
