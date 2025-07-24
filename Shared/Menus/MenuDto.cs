using AquaSolution.Shared.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Menus
{
    public class MenuDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public List<PageDto> Pages { get; set; } = new();
    }
}
