using System;

namespace AquaSolution.Data.Data.Entities.Scraps
{
    public class ScrapFinalApprover
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
