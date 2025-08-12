using AquaSolution.Shared.ManageMedicalRooms.Inventories;

namespace AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService
{
    public interface IInventoryService
    {
        Task<List<InventoryDto>> LoadListAsync();
       
    }
}
