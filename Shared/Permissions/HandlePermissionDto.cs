using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Permissions
{
    public class HandlePermissionDto
    {
        public Guid Id { get; set; }
        public PermissionType Type { get; set;}
        public PermissionActionType Action { get; set;}
        public Guid? MenuId { get; set;}
        public Guid? PageId { get; set;}
       
    }
}
