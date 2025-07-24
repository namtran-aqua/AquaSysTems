using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Permissions
{
    public class GroupedPermissionDto
    {
        public Guid PermissionId { get; set; }
        public Guid MenuId { get; set; }
        public string MenuName { get; set; } = default!;
        public bool IsChecked { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
