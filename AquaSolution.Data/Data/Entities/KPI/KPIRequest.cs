using AquaSolution.Shared.Enum.KPIType;

namespace AquaSolution.Data.Data.Entities
{
    public class KPIRequest
    {
        public Guid Id { get; set; }
        public Guid SubmitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }      
        public StatusKPIRequestType RequestStatus { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public Guid? ApprovalBy { get; set; }

        public DateTime? RejectDate { get; set; }
        public Guid? RejectBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }

        public string? Note { get; set; }
    }
}
