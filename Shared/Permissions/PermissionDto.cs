using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Permissions
{
    public class PermissionDto
    {
        public Guid PermissionId { get; set; }
        public Guid? MenuId { get; set; }
        public string MenuName { get; set; } = default!;
        public Guid? PageId { get; set; }
        public string PageName { get; set; } = default!;
        public string? Action { get; set; } = default!;
        public string PermissionName => $"{Action}: {(string.IsNullOrWhiteSpace(PageName) ?"-----Menu - " + MenuName : PageName)}";

        public bool IsChecked { get; set; }
    }
}
