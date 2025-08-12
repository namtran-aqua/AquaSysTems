using AquaSolution.Shared.UserManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.RequestClinics
{
    public class ParamShowModal
    {
      public  UserDto currenUser { get; set; }
        public List<UserDto>users { get; set; } =new List<UserDto>();
        public bool IsEdit { get; set; }
        public HandleMyRequestClinicDto handleMyRequestClinicDto { get; set; } = new HandleMyRequestClinicDto();
    }
}
