using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.RequestClinics
{
    public class HandleMyRequestClinicDto
    {
        public Guid Id { get; set; }
        public string WorkDayUserRequestId { get; set; }
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


    }
}
