using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Administration.UserManagements
{
    public class UserSelectedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public string WorkDayId { get; set; }
        public string DisplayLabel => $"{WorkDayId} - {Name}";
        public string? ManagerName { get; set; }
        public Guid ManagerId { get; set; }
        public string WorkDayManager { get; set; }
        public string? Email { get; set; }

    }
}
