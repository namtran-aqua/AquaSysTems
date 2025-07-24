using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities
{
    public class Page
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? Order { get; set; }
        public string? Icon { get; set; }
    }

}
