using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.UserManagements
{
    public class UserContributerDto :BaseDto
    {
        public Guid? DepartmentId { get;set; }
        public DepartmentType DepartmentType { get; set; }
        public Guid? FactoryId { get;set; }

    }
}
