using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class SysTemHistory
    {
        public Guid Id { get; set; }
        public string? HistoryFlow { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
