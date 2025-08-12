using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities
{
    public class Treatment
    {
       public Guid RequestId { get; set; }
       public string? Diagnose { get; set; }
       public string Treatments { get; set; } = string.Empty;
       public string? Note { get; set; }
       public DateTime CheckInTime { get; set; }
       public Guid PharmacyManagerId { get; set; }
       public TreatmentType TreatmentType { get; set; }

    }
}
