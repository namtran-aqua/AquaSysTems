using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities
{
    public class ApprovalFlow
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public string Name { get; set; }
        public Guid? UserApproveId { get; set; }
        public string? DesCription { get; set; }
        public int CurrentStep { get; set; }
        public int NextStep { get; set; }
        public SystemType System { get; set; }
        public ApprovalSettingType ApprovalSettingType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
