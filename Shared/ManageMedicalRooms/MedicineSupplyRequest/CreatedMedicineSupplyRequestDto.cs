using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest
{
    public class CreatedMedicineSupplyRequestDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public Guid UserRequestId { get; set; }
        public string? UserRequestName { get; set; }
        public string UserRequestEmail { get; set; }
        public DateTime? CreatedDate { get; set; } =DateTime.Now;
        public Guid CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public Guid? ApprovalId { get; set; }
        public string? ApprovalName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public Guid? PharmacyManagerId { get; set; }
        public string? PharmacyManagerName { get; set; }
        public DateTime? MedicineDispensingDate { get; set; }
        public MedicineSupplyRequestType RequestType { get; set; }
        public string? Note { get; set; }
        public List<CreatedMedicineSupplyRequestDetailDto> CreatedMedicineSupplyRequestDetail { get; set; } = new();

    }
}
