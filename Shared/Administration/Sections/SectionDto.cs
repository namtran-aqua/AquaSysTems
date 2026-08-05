using System;

namespace AquaSolution.Shared.Administration.Sections
{
    public class SectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
