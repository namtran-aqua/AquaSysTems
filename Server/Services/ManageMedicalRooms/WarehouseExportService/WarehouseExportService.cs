using AquaSolution.Client.Pages.ManageMedicalRooms.Inventories;
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Common.HandleInventories;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using System.Text;

namespace AquaSolution.Server.Services.ManageMedicalRooms.WarehouseExportService
{
    public class WarehouseExportService : IWarehouseExportService
    {
        private readonly IRepository<Inventories> _inventoryRepo;
        private readonly IRepository<WarehouseExport> _warehouseExportRepo;
        private readonly IRepository<WarehouseExportDetail> _warehouseExportDetailRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IHandleInventory _handleInventory;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<SysTemHistory> _sysTemHistory;

        private readonly AquaDbContext _context;

        public WarehouseExportService
            (
            IRepository<Inventories> inventoryRepo,
             IRepository<WarehouseExport> warehouseExportRepo,
             IRepository<Product> productRepo,
                AquaDbContext context,
                  IHandleInventory handleInventory,
                IRepository<User> userRepo,
                IRepository<SysTemHistory> sysTemHistory,
        IRepository<WarehouseExportDetail> warehouseExportDetailRepo

            )
        {
            _inventoryRepo = inventoryRepo;
            _context = context;
            _warehouseExportRepo = warehouseExportRepo;
            _warehouseExportDetailRepo = warehouseExportDetailRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _handleInventory = handleInventory;
            _sysTemHistory = sysTemHistory;
        }

        public async Task<List<LoadWarehouseExportDto>> GetWarehouseExport()
        {
            try
            {
                var warehouseExportQuery = from warehouseExport in await _warehouseExportRepo.GetQueryableAsync()
                                           join user in await _userRepo.GetQueryableAsync()
                                           on warehouseExport.CreatedBy equals user.Id
                                           select new LoadWarehouseExportDto
                                           {
                                               Id = warehouseExport.Id,
                                               Code = warehouseExport.Code,
                                               Name = warehouseExport.Name,
                                               Description = warehouseExport.Description,
                                               Note = warehouseExport.Note,
                                               CreatedDate = warehouseExport.CreatedDate,
                                               CreatedBy = warehouseExport.CreatedBy,
                                               UpdatedBy = warehouseExport.UpdatedBy,
                                               UpdatedDate = warehouseExport.UpdatedDate,
                                               WarehouseExportType = warehouseExport.WarehouseExportType,
                                               CreatedByName = user.FullName,
                                           };
                var datareturn = warehouseExportQuery.OrderBy(x => x.CreatedDate).ToList();
                if (datareturn.Any()) { return datareturn; }
                return new List<LoadWarehouseExportDto>();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<LoadWarehouseExportDetailDto>> GetWarehouseExportDetail(Guid warehouseExportId)
        {
            var warehouseExportDetailQuery = from warehouseExportDetail in await _warehouseExportDetailRepo.GetQueryableAsync()
                                             join product in await _productRepo.GetQueryableAsync()
                                             on warehouseExportDetail.ProductId equals product.Id
                                             where warehouseExportDetail.WarehouseExportId == warehouseExportId
                                             select new LoadWarehouseExportDetailDto
                                             {
                                                 Id = warehouseExportDetail.Id,
                                                 ProductId = product.Id,
                                                 ProductName = product.Name,
                                                 DateManufacture = warehouseExportDetail.DateManufacture,
                                                 ExpiryDate = warehouseExportDetail.ExpiryDate,
                                                 Quantity = warehouseExportDetail.Quantity,
                                                 ProductType = warehouseExportDetail.ProductType,
                                                 Unit = product.Unit,
                                             };
            var dataReturn = warehouseExportDetailQuery.OrderBy(x => x.ProductName).ToList();
            if (dataReturn.Any()) { return dataReturn; }
            return new List<LoadWarehouseExportDetailDto>();
        }

        public async Task<bool> WarehouseExport(CreatedWarehouseExportDto createdWarehouseExportDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                StringBuilder history = new StringBuilder();
                var inventoryQuery = await _inventoryRepo.GetQueryableAsync();

                var warehouseExport = new WarehouseExport
                {
                    Id = Guid.NewGuid(),
                    Code = createdWarehouseExportDto.WarehouseExportDto.Code,
                    Name = createdWarehouseExportDto.WarehouseExportDto.Name,
                    Description = createdWarehouseExportDto.WarehouseExportDto.Description,
                    Note = createdWarehouseExportDto.WarehouseExportDto.Note,
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdWarehouseExportDto.WarehouseExportDto.CreatedBy,
                    WarehouseExportType = createdWarehouseExportDto.WarehouseExportDto.WarehouseExportType,
                };
                var userName = await _userRepo.FirstOrDefaultAsync(x => x.Id == warehouseExport.CreatedBy);
                history.Append($"User {userName.FullName}");
                await _warehouseExportRepo.InsertAsync(warehouseExport);

                foreach (var item in createdWarehouseExportDto.WarehouseExportDetailDtos)
                {
                    var detail = new WarehouseExportDetail
                    {
                        Id = item.Id,
                        ProductId = item.productDto.Id,
                        WarehouseExportId = warehouseExport.Id,
                        DateManufacture = item.productDto.ManufacturingDate,
                        ExpiryDate = item.productDto.ExpirationDate,
                        Quantity = item.Quantity,
                        ProductType = item.productDto.ProductType
                    };

                    await _warehouseExportDetailRepo.InsertAsync(detail);
                    var handleInventorydto = new HandleInventoryDto
                    {
                        ProductId = item.productDto.Id,
                        ExpirationDate = item.productDto.ExpirationDate,
                        Quantity = item.Quantity,
                        ManufacturingDate = item.productDto.ManufacturingDate,
                    };
                    var oldInventory =await _handleInventory.GetActualInventory(handleInventorydto);
                    await _handleInventory.MinusInventory(handleInventorydto);
                    var oldInventoryFormatted = oldInventory?.ToString("0") ?? "0";
                    history.AppendLine($" - đã cập nhật tồn kho productName {item.productDto.Name} từ {oldInventoryFormatted} trừ đi {item.Quantity} - vào lúc {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }

                if (createdWarehouseExportDto.WarehouseExportDto.WarehouseExportType ==
                           WarehouseExportType.InventoryAdjustment)
                {
                    var sysTemHistory = new SysTemHistory
                    {
                        Id = Guid.NewGuid(),
                        HistoryFlow = history.ToString()
                    };
                    await _sysTemHistory.InsertAsync(sysTemHistory);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

    }
}
