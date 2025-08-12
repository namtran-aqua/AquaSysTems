using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Prescriptions;

namespace AquaSolution.Shared.ManageMedicalRooms.RequestClinics
{
    public class MedicalHistoryDto
    {
        public Guid Id { get; set; }
        public string WorkDayUserRequestId { get; set; }
        public string? Diagnose { get; set; }
        public string? Treatments { get; set; } = string.Empty;
        public Guid? TreatmentId { get; set; } 
        public string? TreatmentNote { get; set; }
        public DateTime? CheckInTime { get; set; }
        public TreatmentType? TreatmentType { get; set; }
        public Guid UserRequestId { get; set; }
        public string UserRequestName { get; set; }
        public string RequestTitle { get; set; }
        public PurposeType PurposeType { get; set; }
        public Guid ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? EmailRequestter { get; set; }
        public string? WorkDayManager { get; set; }
        public StatusClinicType Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public Guid? ApprovalById { get; set; }
        public string? ApprovalByName { get; set; }
        public DateTime? RejectDate { get; set; }
        public Guid? RejectById { get; set; }
        public string? RejectByName { get; set; }
        public DateTime? SuccesDate { get; set; }
        public Guid? PharmacyManagerId { get; set; }
        public string? PharmacyManagerName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? Note { get; set; }
        public string? HistoryReuqest { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedName { get; set; }
        public string CreatedWorkDay { get; set; }
        public Guid? PrescriptionId { get; set; }
        public string? PrescriptionName { get; set; }
        public string? PrescriptionCode { get; set; }
        public DateTime? PrescriptionCreatedTime { get; set; }
        public List<PrescriptionDetailDto>? PrescriptionDetail { get; set; } 
    }
}
