using AquaSolution.Shared.Enum;

namespace AquaSolution.Shared.Position
{
    public class PositionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
        public PositionType PositionType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
