using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class RequestClinic
    {
        public Guid Id { get; set; }
        public string WorkDayUserRequestId { get; set; }
        public Guid UserRequestId { get; set; }
        public string UserRequestName { get; set; }
        public string RequestTitle { get; set; }
        public PurposeType PurposeType { get; set; }
        public Guid? ManagerId { get; set; }
        public StatusClinicType Status { get; set; }
        public  DateTime? ApprovalDate { get; set; }
        public Guid? ApprovalBy {  get; set; }
        public DateTime? RejectDate { get; set; }
        public Guid? RejectBy { get; set; }
        public DateTime? SuccesDate { get; set; }
        public Guid? PharmacyManagerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Note { get; set; }
        public string? HistoryReuqest { get; set; }
        public Guid CreatedBy { get; set; }

    }
}