using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Prescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.Treatments
{
    public class CreatedTreatmentDto
    {
        public Guid RequestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? Diagnose { get; set; }
        public string Treatments { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CheckInTime { get; set; }
        public Guid PharmacyManagerId { get; set; }
        public TreatmentType TreatmentType { get; set; }
        public CreatedPrescriptionDto Prescription { get; set; } = new();
    }
}
