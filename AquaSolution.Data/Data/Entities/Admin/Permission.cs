using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Admin
{
    public class Permission
    {
        public Guid Id { get; set; }
        public PermissionActionType Action { get; set; }           
        public PermissionType Type { get; set; }           

        public Guid? MenuId { get; set; }
        public Guid? PageId { get; set; }
    }

}
