using AquaSolution.Data.Data.Entities.Clinic;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.CommonDto;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Server.Services.Common.HandleInventories
{
    public class HandleInventory : IHandleInventory
    {
        private readonly IRepository<Inventories> _inventoryRepo;
        private readonly IRepository<InventoryPeriod> _inventoryPeriodRepo;

        private readonly IRepository<Product> _productRepo;
        public HandleInventory(IRepository<Inventories> inventoryRepo,
            IRepository<Product> productRepo,
            IRepository<InventoryPeriod> inventoryPeriodRepo)
        {
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _inventoryPeriodRepo = inventoryPeriodRepo;
        }
        public  async Task<bool> AddInventory(HandleInventoryDto handleInventoryDto)
        {
            var inventoryQuery = await _inventoryRepo.GetQueryableAsync();
            var product = await _productRepo.GetQueryableAsync();
            var existingInventory = inventoryQuery.FirstOrDefault(i =>
                i.ProductId == handleInventoryDto.ProductId &&
                (
      
                    (!handleInventoryDto.ExpirationDate.HasValue && !i.ExpirationDate.HasValue)
                    ||
                    (handleInventoryDto.ExpirationDate.HasValue && i.ExpirationDate.HasValue &&
                     i.ExpirationDate.Value.Date == handleInventoryDto.ExpirationDate.Value.Date)
                )
                &&
                (
                    (!handleInventoryDto.ManufacturingDate.HasValue && !i.ManufacturingDate.HasValue)
                    ||
                    (handleInventoryDto.ManufacturingDate.HasValue && i.ManufacturingDate.HasValue &&
                     i.ManufacturingDate.Value.Date == handleInventoryDto.ManufacturingDate.Value.Date)
                ));

            if (existingInventory != null)
            {
                existingInventory.Quantity += handleInventoryDto.Quantity;
                return await _inventoryRepo.UpdateAsync(existingInventory);
            }
            else
            {
                var newInventory = new Inventories
                {
                    Id = Guid.NewGuid(),
                    ProductId = handleInventoryDto.ProductId,
                    Quantity = handleInventoryDto.Quantity,
                    ExpirationDate = handleInventoryDto.ExpirationDate,
                    ManufacturingDate = handleInventoryDto.ManufacturingDate
                };
                await _inventoryRepo.InsertAsync(newInventory);
                return await _inventoryRepo.SaveChangesAsync() > 0;
            }
        }
        public async Task<bool> MinusInventory(HandleInventoryDto handleInventoryDto)
        {

            var inventoryQuery = await _inventoryRepo.GetQueryableAsync();
            var product = await _productRepo.GetQueryableAsync();
            var existingInventory = inventoryQuery.FirstOrDefault(i =>
                i.ProductId == handleInventoryDto.ProductId &&
                (
                    (!handleInventoryDto.ExpirationDate.HasValue && !i.ExpirationDate.HasValue)
                    ||
                    (handleInventoryDto.ExpirationDate.HasValue && i.ExpirationDate.HasValue &&
                     i.ExpirationDate.Value.Date == handleInventoryDto.ExpirationDate.Value.Date)
                )
                &&
                (
                    (!handleInventoryDto.ManufacturingDate.HasValue && !i.ManufacturingDate.HasValue)
                    ||
                    (handleInventoryDto.ManufacturingDate.HasValue && i.ManufacturingDate.HasValue &&
                     i.ManufacturingDate.Value.Date == handleInventoryDto.ManufacturingDate.Value.Date)
                ));


            if (existingInventory != null)
            {
                if (existingInventory.Quantity < handleInventoryDto.Quantity)
                {
                    var productName = product.FirstOrDefault(x=>x.Id == handleInventoryDto.ProductId).Name.ToString();
                    throw new InvalidOperationException($"{productName} -Insufficient inventory");
                }
                existingInventory.Quantity -= handleInventoryDto.Quantity;
               await _inventoryRepo.UpdateAsync(existingInventory);
                var deleteInventoryZero = await _inventoryRepo.WhereAsync(x => x.ProductId == handleInventoryDto.ProductId);
                if (deleteInventoryZero.Count>1) 
                {
                    foreach ( var item in deleteInventoryZero )
                    {
                        if (item.Quantity == 0)
                        {
                            //await _inventoryRepo.DeleteAsync(item);
                            item.IsActive = false;
                            await _inventoryRepo.UpdateAsync(item);
                        }
                    }    
                }
                return true;
            }
            return false;
        }
        public async Task<string> GenerateInventoryCodeAsync()
        {
            try
            {
                var query = await _inventoryPeriodRepo.GetQueryableAsync();

                // Lấy record mới nhất theo CreatedDate
                var lastRecord = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                string prefixStr = "INV";
                string datePartStr = DateTime.Now.ToString("yyyyMMdd");
                int nextNumber = 1;

                // Nếu có record cũ thì tách lấy số và tăng lên 1
                if (lastRecord != null && !string.IsNullOrEmpty(lastRecord.Code))
                {
                    var parts = lastRecord.Code.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                string numberPartStr = nextNumber.ToString("D5");
                return $"{prefixStr}-{datePartStr}-{numberPartStr}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating code: {ex}");
                throw;
            }
        }
        public async Task<decimal?> GetActualInventory(HandleInventoryDto handleInventoryDto)
        {
            var inventoryQuery = await _inventoryRepo.GetQueryableAsync();
            var existingInventory = inventoryQuery.FirstOrDefault(i =>
                i.ProductId == handleInventoryDto.ProductId &&
                (
                    (!handleInventoryDto.ExpirationDate.HasValue && !i.ExpirationDate.HasValue)
                    ||
                    (handleInventoryDto.ExpirationDate.HasValue && i.ExpirationDate.HasValue &&
                     i.ExpirationDate.Value.Date == handleInventoryDto.ExpirationDate.Value.Date)
                )
                &&
                (
                    (!handleInventoryDto.ManufacturingDate.HasValue && !i.ManufacturingDate.HasValue)
                    ||
                    (handleInventoryDto.ManufacturingDate.HasValue && i.ManufacturingDate.HasValue &&
                     i.ManufacturingDate.Value.Date == handleInventoryDto.ManufacturingDate.Value.Date)
                ));

            if (existingInventory != null)
            {
                return existingInventory.Quantity;
            }
            return 0;
        }
    }
}
