using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.RequestClinics
{
    public class UpdateRequestClinicStatusDto
    {
        public Guid RequestClinicId { get; set; }
        public Guid UserId { get; set; }
    }

}
