using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.UserManagements
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string WorkDayId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string ManagerWorkDay { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime Created { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public List<RoleDto> Roles { get; set; } = new(); 
    }

}
