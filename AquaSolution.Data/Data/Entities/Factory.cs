using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities
{
    public class Factory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
        public FactoryType FactoryType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
