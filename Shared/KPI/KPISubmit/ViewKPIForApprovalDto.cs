using AquaSolution.Shared.Enum.KPIType;
namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class ViewKPIForApprovalDto
    {
        public Guid Id { get; set; }
        public Guid SubmitId { get; set; }
        public int Step { get; set; }
        public string Title { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public EApprovalStatusType EApprovalStatusType { get; set; }
        public string Position { get; set; }
        public Guid PositionId { get; set; }
        public Guid? DecisionMaker { get; set; }
        public int Month { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? RejectDate { get; set; }
    }
}
