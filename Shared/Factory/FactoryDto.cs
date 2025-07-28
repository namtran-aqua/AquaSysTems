using AquaSolution.Shared.Enum;

namespace AquaSolution.Shared.Factory
{
    public class FactoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
        public FactoryType FactoryType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
