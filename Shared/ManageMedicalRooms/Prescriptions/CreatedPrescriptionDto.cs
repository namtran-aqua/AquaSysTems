using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.Prescriptions
{
    public class CreatedPrescriptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedTime { get; set; }
        public List<CreatedPrescriptionDetailDto> CreatedPrescriptionDetail {get;set;} = new List<CreatedPrescriptionDetailDto>();
    }
}
