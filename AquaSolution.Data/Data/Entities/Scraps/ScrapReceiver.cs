using System;

namespace AquaSolution.Data.Data.Entities.Scraps
{
    public class ScrapReceiver
    {
        public Guid Id { get; set; }
        public Guid FactoryId { get; set; }
        public Guid UserId { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
