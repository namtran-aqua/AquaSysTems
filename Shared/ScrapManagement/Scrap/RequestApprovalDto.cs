using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class RequestApprovalDto
    {
        public Guid Id { get; set; }
        public Guid HistoryScrapId { get; set; }
        public StatusScrap Status { get; set; }
        public string? Comment { get; set; }
        public Guid? ActionBy { get; set; }
        public string? ActionByName { get; set; }
        public string? ActionByWorkDayId { get; set; }
        public string? ActionByEmail { get; set; }
        public DateTime? ActionDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Step { get; set; }
        public Guid DecisionMaker { get; set; }
        public string? DecisionMakerName { get; set; }
        public string? DecisionMakerWorkDayId { get; set; }
        public string? DecisionMakerEmail { get; set; }
    }
}
