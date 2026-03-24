using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Admin
{
    public class Groups
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdateBy { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? Description { get; set; } 
        
    }

}
