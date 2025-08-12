using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;

namespace AquaSolution.Server.Services.ManageMedicalRooms.WarehouseExportService
{
    public interface IWarehouseExportService
    {                 
        Task<bool> WarehouseExport(CreatedWarehouseExportDto createdWarehouseExportDto);
        Task<List<LoadWarehouseExportDto>> GetWarehouseExport();
        Task<List<LoadWarehouseExportDetailDto>> GetWarehouseExportDetail(Guid warehouseImportId);
    }
}
