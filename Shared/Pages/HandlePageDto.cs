using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Pages
{
    public class HandlePageDto
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
