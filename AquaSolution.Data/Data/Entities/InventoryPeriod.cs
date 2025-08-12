using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities
{
    public class InventoryPeriod
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Month { get; set; } 
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
        public string? Note { get; set; }
    }

}
