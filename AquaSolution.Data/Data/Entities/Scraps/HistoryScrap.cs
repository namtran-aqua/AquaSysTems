using AquaSolution.Shared.Enum.Scrap;

namespace AquaSolution.Data.Data.Entities.Scraps
{
    public class HistoryScrap
    {
        public Guid Id { get; set; }
        public string Title { get; set; }   
        public StatusScrap Status { get; set; }
        public string Description { get; set; } 
        public Guid? LastActionBy {  get; set; }
        public DateTime? LastActionDate { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid FactoryId { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Notes { get; set; }
        public Guid? Confirmer { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public ConfirmationStatusType ConfirmationStatusType { get; set;  }
        public decimal? ConfirmAmount { get; set; } = 0;
        public decimal? TotalAmount { get; set; } = 0;

    }
}
