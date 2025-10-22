
using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities.KPI
{
    public class KPIApprovalTask
    {
        public Guid Id { get; set; }
        public Guid SubmitId { get; set; }
        public Guid RequesterId { get; set; }
        public EApprovalStatusType StatusType { get; set; }
        public string? Comment { get; set; }
        public  Guid? ApprovedBy { get; set; }
        public  DateTime? ApprovalDate { get; set; }
        public Guid? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
    }
}
