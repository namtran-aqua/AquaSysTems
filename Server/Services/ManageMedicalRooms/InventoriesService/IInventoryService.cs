using AquaSolution.Shared.ManageMedicalRooms.Inventories;

namespace AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService
{
    public interface IInventoryService
    {
        Task<List<InventoryDto>> LoadListAsync();
        Task<LoadReportInventoryDto> LoadReportInventoryAsync();
        Task<LoadReportInventoryDto> LoadReportAsync(int month, int year);

        Task<bool> InsertReportInventoryAsync(CreatedReportInventoryDto createdReportInventoryDto);

    }
}
