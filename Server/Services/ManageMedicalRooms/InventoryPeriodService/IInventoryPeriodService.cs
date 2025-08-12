using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod;

namespace AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService
{
    public interface IInventoryPeriodService
    {
        Task<Guid> InsertDetailInventoryPeriod(CreatedInventoryPeriodDto createdInventoryPeriodDto);
        Task<List<InventoryPeriodDetailDto>> GetInventoryPeriodDetail(Guid inventoryPeriodId);
        Task<List<InventoryPeriodDto>> GetAllInventoryPeriod();
        Task<bool> InsertActualInventoryPeriod(List<InventoryPeriodDetailDto> inventoryPeriodDetailDtos);

    }
}
