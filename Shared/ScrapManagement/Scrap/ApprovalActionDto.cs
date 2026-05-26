using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class ApprovalActionDto
    {
        public Guid HistoryScrapId { get; set; }
        public Guid ActionBy { get; set; }
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
    }
}
