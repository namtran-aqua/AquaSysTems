using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Menus
{
    public class HandleMenuDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Icon { get; set; }
        public int Order { get; set; }
        public PermissionType Type { get; set; }
        public PermissionActionType Action { get; set; }

    }
}
