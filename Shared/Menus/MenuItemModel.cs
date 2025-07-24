using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Menus
{
    public class MenuItemModel
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Permission { get; set; }
        public List<MenuItemModel>? Children { get; set; }
    }

}
