using System;

namespace AquaSolution.Data.Entities.Scraps
{
    public class WasteItem
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
