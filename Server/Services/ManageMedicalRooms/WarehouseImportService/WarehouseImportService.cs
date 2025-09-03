using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Common.HandleInventories;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;

namespace AquaSolution.Server.Services.ManageMedicalRooms.WarehouseImportService
{
    public class WarehouseImportService : IWarehouseImportService
    {
        private readonly IRepository<Inventories> _inventoryRepo;
        private readonly IRepository<WarehouseImport> _warehouseImportRepo;
        private readonly IRepository<WarehouseImportDetail> _warehouseImportDetailRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IHandleInventory _handleInventory;

        private readonly AquaDbContext _context;

        public WarehouseImportService
            (
            IRepository<Inventories> inventoryRepo,
             IRepository<WarehouseImport> warehouseImportRepo,
             IRepository<Product> productRepo,
                AquaDbContext context,
                 IHandleInventory handleInventory,
                IRepository<User> userRepo,
        IRepository<WarehouseImportDetail> warehouseImportDetailRepo

            )
        {
            _inventoryRepo = inventoryRepo;
            _context = context;
            _warehouseImportRepo = warehouseImportRepo;
            _warehouseImportDetailRepo = warehouseImportDetailRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _handleInventory = handleInventory;
        }

        public async Task<List<LoadWarehouseImportDto>> GetWarehouseImport()
        {
            var warehouseImportQuery = from warehouseImport in await _warehouseImportRepo.GetQueryableAsync()
                                       join user in await _userRepo.GetQueryableAsync()
                                       on warehouseImport.CreatedBy equals user.Id
                                       select new LoadWarehouseImportDto
                                       {
                                           Id = warehouseImport.Id,
                                           Code = warehouseImport.Code,
                                           Name = warehouseImport.Name,
                                           Description = warehouseImport.Description,
                                           Note = warehouseImport.Note,
                                           CreatedDate = warehouseImport.CreatedDate,
                                           CreatedBy = warehouseImport.CreatedBy,
                                           UpdatedBy = warehouseImport.UpdatedBy,
                                           UpdatedDate = warehouseImport.UpdatedDate,
                                           WarehouseImportType = warehouseImport.WarehouseImportType,
                                           CreatedByName = user.FullName,

                                       };
            var datareturn = warehouseImportQuery.OrderBy(x=>x.CreatedDate).ToList();
            if (datareturn.Any()) { return datareturn; }
            return new List<LoadWarehouseImportDto>();
        }

        public async Task<List<LoadWarehouseImportDetailDto>> GetWarehouseImportDetail(Guid warehouseImportId)
        {
            var warehouseImportDetailQuery = from warehouseImportDetail in await _warehouseImportDetailRepo.GetQueryableAsync()
                                             join product in await _productRepo.GetQueryableAsync()
                                             on warehouseImportDetail.ProductId equals product.Id
                                             where warehouseImportDetail.WarehouseImportId == warehouseImportId
                                             select new LoadWarehouseImportDetailDto
                                             {
                                                 Id = warehouseImportDetail.Id,
                                                 ProductId = product.Id,
                                                 ProductName = product.Name,
                                                 DateManufacture = warehouseImportDetail.DateManufacture,
                                                 ExpiryDate = warehouseImportDetail.ExpiryDate,
                                                 Quantity = warehouseImportDetail.Quantity,
                                                 ProductType = warehouseImportDetail.ProductType,
                                                 Unit = product.Unit,
                                             };
            var dataReturn = warehouseImportDetailQuery.OrderBy(x => x.ProductName).ToList();
                if(dataReturn.Any()) { return dataReturn; }
            return new List<LoadWarehouseImportDetailDto>();
        }

        public async Task<bool> WarehouseImport(CreatedWarehouseImportDto createdWarehouseImportDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var inventoryQuery = await _inventoryRepo.GetQueryableAsync();

                var warehouseImport = new WarehouseImport
                {
                    Id = Guid.NewGuid(),
                    Code = createdWarehouseImportDto.WarehouseImportDto.Code,
                    Name = createdWarehouseImportDto.WarehouseImportDto.Name,
                    Description = createdWarehouseImportDto.WarehouseImportDto.Description,
                    Note = createdWarehouseImportDto.WarehouseImportDto.Note,
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdWarehouseImportDto.WarehouseImportDto.CreatedBy,
                    WarehouseImportType = createdWarehouseImportDto.WarehouseImportDto.WarehouseImportType,
                };

                await _warehouseImportRepo.InsertAsync(warehouseImport);

                foreach (var item in createdWarehouseImportDto.WarehouseImportDetailDtos)
                {
                    var detail = new WarehouseImportDetail
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        WarehouseImportId = warehouseImport.Id,
                        DateManufacture = item.DateManufacture,
                        ExpiryDate = item.ExpiryDate,
                        Quantity = item.Quantity,
                        ProductType = item.ProductType
                    };

                    await _warehouseImportDetailRepo.InsertAsync(detail);
                    var handleInventorydto = new HandleInventoryDto
                    {
                        ProductId = item.ProductId,
                        ExpirationDate = item.ExpiryDate,
                        Quantity = item.Quantity,
                        ManufacturingDate = item.DateManufacture,
                    };
                    await _handleInventory.AddInventory(handleInventorydto);
                }

                // ✅ Gộp save changes lại
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
