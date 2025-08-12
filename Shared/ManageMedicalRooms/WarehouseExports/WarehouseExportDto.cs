using AquaSolution.Shared.Enum;

namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseExports
{
    public class WarehouseExportDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public WarehouseExportType WarehouseExportType { get; set; }
    }
}
