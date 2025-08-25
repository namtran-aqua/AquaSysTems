using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class InventoryService : IInventoryService
{
    private readonly IRepository<Product> _productRepo;
 
    private readonly IRepository<Inventories> _inventoryRepo;
 
    public InventoryService(IRepository<Product> productRepo , 
        IRepository<Inventories> inventoryRepo)
    {
        _inventoryRepo = inventoryRepo;
        _productRepo = productRepo;
    }

    public async Task<List<InventoryDto>> LoadListAsync()
    {
        try
        {
            var productQuery = await _productRepo.GetQueryableAsync();
            var inventoryQuery =await _inventoryRepo.GetQueryableAsync() ;
            var inventoryList = await (from inventory in inventoryQuery
                                       join product in productQuery
                                           on inventory.ProductId equals product.Id
                                       select new { inventory, product }).ToListAsync();

            var result = inventoryList
               .GroupBy(x => new
               {
                   x.product.Id, 
                   ExpirationDate = x.inventory.ExpirationDate.HasValue ? x.inventory.ExpirationDate.Value.Date : (DateTime?)null,
                   ManufacturingDate = x.inventory.ManufacturingDate.HasValue ? x.inventory.ManufacturingDate.Value.Date : (DateTime?)null
               })

                .Select(g =>
                {
                    var first = g.First();
                    return new InventoryDto
                    {
                        Id = first.inventory.Id,
                        ProductId = first.product.Id,
                        ProductCode = first.product.Code ?? string.Empty,
                        ProductName = first.product.Name ?? string.Empty,
                        Unit = first.product.Unit ?? string.Empty,
                        Quantity = g.Sum(x => x.inventory.Quantity),
                        ProductType = first.product.ProductType,
                        ExpirationDate = g.Key.ExpirationDate,
                        ManufacturingDate = g.Key.ManufacturingDate,
                        expired = g.Key.ExpirationDate < DateTime.Now.Date
                    };
                })
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }



    }
}
