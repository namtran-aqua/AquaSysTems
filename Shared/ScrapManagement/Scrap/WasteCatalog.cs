using System;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class WasteItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string DisplayText => $"{Code} - {Name}";

        public WasteItemDto() { }

        public WasteItemDto(string code, string name, string category, int orderIndex = 0)
        {
            Code = code;
            Name = name;
            Category = category;
            OrderIndex = orderIndex;
        }
    }
}
