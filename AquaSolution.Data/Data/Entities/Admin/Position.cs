using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Admin
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Note { get; set; }
        public PositionType Type { get; set; }
        public string? DesCription { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
