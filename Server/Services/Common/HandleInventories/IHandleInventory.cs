using AquaSolution.Shared.CommonDto;

namespace AquaSolution.Server.Services.Common.HandleInventories
{
    public interface IHandleInventory
    {
        Task<bool> MinusInventory(HandleInventoryDto handleInventoryDto);
        Task<bool> AddInventory(HandleInventoryDto handleInventoryDto);
        Task<string> GenerateInventoryCodeAsync();
        Task<decimal?> GetActualInventory(HandleInventoryDto handleInventoryDto);
    }
}
