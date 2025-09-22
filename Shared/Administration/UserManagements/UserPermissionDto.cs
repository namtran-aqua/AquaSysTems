using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Administration.UserManagements
{
    public class UserPermissionDto
    {
        public Guid PageId { get; set; }
        public string PageName { get; set; }
        public PermissionActionType Action { get; set; }
    }
}
