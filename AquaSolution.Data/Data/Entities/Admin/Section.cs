using System;

namespace AquaSolution.Data.Data.Entities.Admin
{
    public class Section
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
