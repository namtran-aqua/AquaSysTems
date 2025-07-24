using AquaSolution.Shared.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Pages
{
    public class PageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? Order { get; set; }
        public string? Icon { get; set; }
        public List<string> Permissions { get; set; } = new ();
        public string? JoinPermissions { get; set; }
        public  List<HandlePermissionDto>? HandlePermissionDtos { get; set; }
    }
}
