using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class MedicineSupplyRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }    
        public string? Description { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid FactoryId { get; set; }
        public Guid UserRequestId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ApprovalId { get; set; }
        public DateTime? ApprovedDate { get;set; }
        public Guid? RejectId { get; set; }
        public DateTime? RejectDate { get; set; }
        public Guid? PharmacyManagerId { get; set; }
        public DateTime? MedicineDispensingDate { get; set; }
        public MedicineSupplyRequestType RequestType { get; set; }
        public string? Note { get; set; }
    }
}
