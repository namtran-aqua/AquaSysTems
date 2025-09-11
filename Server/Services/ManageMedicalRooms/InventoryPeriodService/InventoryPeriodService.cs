using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod;

namespace AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService
{
    public class InventoryPeriodService : IInventoryPeriodService
    {
        private readonly IRepository<InventoryPeriodDetail> _inventoryPeriodDetailRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<InventoryPeriod> _inventoryPeriodRepo;
        private readonly IRepository<User> _userRepo;

        public InventoryPeriodService(
            IRepository<InventoryPeriodDetail> inventoryPeriodDetailRepo,
            IRepository<InventoryPeriod> inventoryPeriodRepo,
            IRepository<Product> productRepo,
            IRepository<User> userRepo
            )
        {
            _inventoryPeriodDetailRepo = inventoryPeriodDetailRepo;
            _productRepo = productRepo;
            _inventoryPeriodRepo = inventoryPeriodRepo;
            _userRepo = userRepo;
        }

        public async Task<List<InventoryPeriodDto>> GetAllInventoryPeriod()
        {
            var listInventoryPeriod = from inventoryPeriod in await _inventoryPeriodRepo.GetQueryableAsync()
                                      join user in await _userRepo.GetQueryableAsync()
                                      on inventoryPeriod.CreatedById equals user.Id
                                      select new InventoryPeriodDto
                                      {
                                          Id = inventoryPeriod.Id,
                                          Code = inventoryPeriod.Code,
                                          Name = inventoryPeriod.Name,
                                          Year = inventoryPeriod.Year,
                                          Month = inventoryPeriod.Month,
                                          CreatedDate = inventoryPeriod.CreatedDate,
                                          CreatedById = inventoryPeriod.CreatedById,
                                          Note = inventoryPeriod.Note,
                                          CreatedByName = user.FullName,
                                      };
            return listInventoryPeriod.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public async Task<List<InventoryPeriodDetailDto>> GetInventoryPeriodDetail(Guid inventoryPeriodId)
        {
            var queryData = from inventoryPeriodDetail in await _inventoryPeriodDetailRepo.GetQueryableAsync()
                            join product in await _productRepo.GetQueryableAsync()
                            on inventoryPeriodDetail.ProductId equals product.Id
                            where inventoryPeriodDetail.InventoryPeriodId == inventoryPeriodId
                            select new InventoryPeriodDetailDto
                            {
                                InventoryId = inventoryPeriodDetail.InventoryId,
                                InventoryPeriodId = inventoryPeriodId,
                                ProductId = product.Id,
                                Unit = product.Unit,
                                ProductCode = product.Code,
                                ProductName = product.Name,
                                ProductType = product.ProductType,
                                ActualQuantity = inventoryPeriodDetail.ActualQuantity,
                                SystemQuantity = inventoryPeriodDetail.SystemQuantity,
                                RemainingValidity = inventoryPeriodDetail.RemainingValidity,
                                DateManufacture = inventoryPeriodDetail.DateManufacture,
                                ExpiryDate = inventoryPeriodDetail.ExpiryDate,
                            };
            if (queryData.ToList().Any())
            {
                return queryData.ToList();
            }
            return new List<InventoryPeriodDetailDto>();
        }

        public async Task<bool> InsertActualInventoryPeriod(List<InventoryPeriodDetailDto> inventoryPeriodDetailDtos)
        {
            foreach (var item in inventoryPeriodDetailDtos)
            {
                var inventoryPeriodDetail = await _inventoryPeriodDetailRepo.FirstOrDefaultAsync(x => x.InventoryId == item.InventoryId);
                if (inventoryPeriodDetail != null)
                {
                    inventoryPeriodDetail.ActualQuantity = item.ActualQuantity ?? 0;
                    inventoryPeriodDetail.RemainingValidity = item.RemainingValidity;
                    await _inventoryPeriodDetailRepo.UpdateAsync(inventoryPeriodDetail);
                }
            }
            return true;
        }

        public async Task<Guid> InsertDetailInventoryPeriod(CreatedInventoryPeriodDto createdInventoryPeriodDto)
        {
            var checkMonth = await _inventoryPeriodRepo.FirstOrDefaultAsync
                (x => x.Month == createdInventoryPeriodDto.InventoryPeriodDto.Month &&
                x.Year == createdInventoryPeriodDto.InventoryPeriodDto.Year);
            if (checkMonth != null)
            {
                return Guid.Empty;
            }
            var inventoryPeriod = new InventoryPeriod
            {
                Id = Guid.NewGuid(),
                Code = createdInventoryPeriodDto.InventoryPeriodDto.Code,
                Name = createdInventoryPeriodDto.InventoryPeriodDto.Name,
                Year = createdInventoryPeriodDto.InventoryPeriodDto.Year,
                Month = createdInventoryPeriodDto.InventoryPeriodDto.Month,
                Note = createdInventoryPeriodDto.InventoryPeriodDto.Note,
                CreatedDate = DateTime.Now,
                CreatedById = createdInventoryPeriodDto.InventoryPeriodDto.CreatedById,
            };

            await _inventoryPeriodRepo.InsertAsync(inventoryPeriod);
            foreach (var item in createdInventoryPeriodDto.InventoryPeriodDetailDtos)
            {
                var inventory = new InventoryPeriodDetail
                {
                    InventoryId = item.InventoryId,
                    InventoryPeriodId = inventoryPeriod.Id,
                    ProductId = item.ProductId,
                    SystemQuantity = item.SystemQuantity,
                    DateManufacture = item.DateManufacture,
                    ExpiryDate = item.ExpiryDate,
                    ProductType = item.ProductType,
                };
                await _inventoryPeriodDetailRepo.InsertAsync(inventory);
            }
            await _inventoryPeriodDetailRepo.SaveChangesAsync();
            return inventoryPeriod.Id;
        }
    }
}
