using AquaSolution.Shared.Enum.Scrap;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class HandleScrapDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public StatusScrap Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
        public string? CreatedByName { get; set; }        
        public Guid FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public decimal? TotalAmount { get; set; }          
        public decimal? ConfirmAmount { get; set; }        
        public ConfirmationStatusType ConfirmationStatusType { get; set; } 
        public string? Notes { get; set; }              
        public List<HistoryDetailScrapDto>? HistoryDetails { get; set; }
        public List<RequestApprovalDto>? Approvals { get; set; }
    }
}