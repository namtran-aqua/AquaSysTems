using System;

namespace AquaSolution.Shared.ScrapManagement.Receivers
{
    public class ScrapReceiverDto
    {
        public Guid Id { get; set; }
        public Guid FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public Guid UserId { get; set; }
        public string? UserWorkDayId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SaveScrapReceiverDto
    {
        public Guid? Id { get; set; }
        public Guid FactoryId { get; set; }
        public Guid UserId { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
