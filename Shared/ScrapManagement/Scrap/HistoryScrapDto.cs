using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class HistoryScrapDto
    {
        public Guid Id { get; set; } 
        public string Title { get; set; }
        public StatusScrap Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? LastActionBy { get; set; }
        public string LastActionByName { get; set; } = string.Empty;
        public DateTime? LastActionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public Guid FactoryId { get; set; }
        public string FactoryName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set;} = string.Empty;
        public string? Notes { get; set; }
        public ConfirmationStatusType ConfirmationStatusType { get; set; }
        public decimal? ConfirmAmount { get; set; } = 0;
        public decimal? TotalAmount { get; set; } = 0;
        public Guid? ConfirmerId { get; set; }
        public string? ConfirmerName { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public List<HistoryDetailScrapDto> HistoryDetails { get; set; } = new List<HistoryDetailScrapDto>();
        public List<RequestApprovalDto> Approvals { get; set; } = new List<RequestApprovalDto>();
    }
}
