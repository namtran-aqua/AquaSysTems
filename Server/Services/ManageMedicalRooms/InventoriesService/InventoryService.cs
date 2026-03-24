using AntDesign;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Clinic;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing.Printing;

public class InventoryService : IInventoryService
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Inventories> _inventoryRepo;
    private readonly IRepository<WarehouseExport> _warehouseExportRepo;
    private readonly IRepository<WarehouseExportDetail> _warehouseExporDetailRepo;
    private readonly IRepository<WarehouseImport> _warehouseImportRepo;
    private readonly IRepository<WarehouseImportDetail> _warehouseImportDetailRepo;
    private readonly IRepository<Prescription> _prescriptionRepo;
    private readonly IRepository<PrescriptionDetail> _prescriptionDetailRepo;

    private readonly IRepository<ReportInventoryDetail> _reportInventoryDetailRepo;
    private readonly IRepository<ReportInventory> _reportInventoryRepo;
    private readonly IRepository<User> _userRepo;


    public InventoryService(IRepository<Product> productRepo,
        IRepository<Inventories> inventoryRepo,
        IRepository<WarehouseExport> warehouseExportRepo,
        IRepository<WarehouseExportDetail> warehouseExporDetailRepo,
        IRepository<WarehouseImport> warehouseImportRepo,
        IRepository<WarehouseImportDetail> warehouseImportDetailRepo,
        IRepository<Prescription> prescriptionRepo,
        IRepository<PrescriptionDetail> prescriptionDetailRepo,
        IRepository<ReportInventoryDetail> reportInventoryDetailRepo,
        IRepository<ReportInventory> reportInventoryRepo, IRepository<User> userRepo


        )
    {
        _inventoryRepo = inventoryRepo;
        _productRepo = productRepo;
        _warehouseImportRepo = warehouseImportRepo;
        _warehouseImportDetailRepo = warehouseImportDetailRepo;
        _warehouseExportRepo = warehouseExportRepo;
        _warehouseExporDetailRepo = warehouseExporDetailRepo;
        _prescriptionRepo = prescriptionRepo;
        _prescriptionDetailRepo = prescriptionDetailRepo;
        _reportInventoryRepo = reportInventoryRepo;
        _reportInventoryDetailRepo = reportInventoryDetailRepo;
        _userRepo = userRepo;
    }

    public async Task<bool> InsertReportInventoryAsync(CreatedReportInventoryDto createdReportInventoryDto)
    {
        int sumContextChange = 0;
        var existReport = await _reportInventoryRepo
      .FirstOrDefaultAsync(x => x.Month == createdReportInventoryDto.Month
                             && x.Year == createdReportInventoryDto.Year);

        if (existReport != null)
        {
            
            return false;
        }
        var reportinventory = new ReportInventory
        {
            Id = Guid.NewGuid(),
            CreatedBy = createdReportInventoryDto.CreatedBy,
            CreatedDate = DateTime.Now,
            Month = createdReportInventoryDto.Month,
            Year =createdReportInventoryDto.Year,
        };
        await _reportInventoryRepo.InsertAsync(reportinventory);
        sumContextChange += await _reportInventoryRepo.SaveChangesAsync();
        foreach(var detail in createdReportInventoryDto.LoadReportInventoryDetail)
        {
            var reportInventoryDetail = new ReportInventoryDetail
            {
                Id = Guid.NewGuid(),
                ProductId = detail.ProductId,
                ProductName = detail.ProductName,
                ReportInventoryId = reportinventory.Id,
                ExpirationDate = detail.ExpirationDate,
                BeginningInventory = detail.BeginningInventory,
                NewInbound = detail.NewInbound,
                ConsumPosition = detail.ConsumPosition,
                TotalStock = detail.TotalStock,
                Unit = detail.Unit,
            };
            await _reportInventoryDetailRepo.InsertAsync(reportInventoryDetail);
            sumContextChange+= await _reportInventoryDetailRepo.SaveChangesAsync();
        }
        return sumContextChange > 0;
    }

    public async Task<List<InventoryDto>> LoadListAsync()
    {
        try
        {
            var productQuery = await _productRepo.GetQueryableAsync();

            var inventoryQuery = (await _inventoryRepo.GetQueryableAsync())
                                  .Where(x => x.IsActive == true);

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
                }).OrderBy(x => x.ExpirationDate)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<LoadReportInventoryDto> LoadReportAsync(int month, int year)
    {
        // Lấy báo cáo theo tháng + năm
        var reportInventory = await _reportInventoryRepo
            .FirstOrDefaultAsync(x => x.Month == month && x.Year == year);

        if (reportInventory == null)
            return null;

        // Join sang user để lấy CreatedByName
        var user = reportInventory.CreatedBy != Guid.Empty
            ? await _userRepo.FirstOrDefaultAsync(u => u.Id == reportInventory.CreatedBy)
            : null;

        // Lấy danh sách chi tiết
        var details = await _reportInventoryDetailRepo
            .GetListAsync(x => x.ReportInventoryId == reportInventory.Id);

        // Map DTO
        var result = new LoadReportInventoryDto
        {
            Id = reportInventory.Id,
            CreatedBy = reportInventory.CreatedBy,
            CreatedByName = user?.FullName ?? "Unknown",
            CreatedDate = reportInventory.CreatedDate,
            Month = reportInventory.Month,
            Year = reportInventory.Year,
            LoadReportInventoryDetail = details.Select(d => new LoadReportInventoryDetailDto
            {
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                ExpirationDate = d.ExpirationDate,
                BeginningInventory = d.BeginningInventory,
                NewInbound = d.NewInbound,
                ConsumPosition = d.ConsumPosition,
                TotalStock = d.TotalStock,
                Unit = d.Unit
            }).ToList()
        };

        return result;
    }
    public async Task<LoadReportInventoryDto> LoadReportInventoryAsync()
    {

        var reportInventorys= new LoadReportInventoryDto();
        var inventories = await _inventoryRepo.GetAllAsync();
        var importDetails = await _warehouseImportDetailRepo.GetAllAsync();
        var imports = await _warehouseImportRepo.GetAllAsync();
        var exports = await _warehouseExportRepo.GetAllAsync();
        var exportDetails = await _warehouseExporDetailRepo.GetAllAsync();
        var prescriptions = await _prescriptionRepo.GetAllAsync();
        var prescriptionDetails = await _prescriptionDetailRepo.GetAllAsync();
        var product = await _productRepo.GetAllAsync();

        var baseInventory = new List<BaseInventory>();
        var month = DateTime.Now.Month - 1;
        var year = DateTime.Now.Year;

        if (month == 0)
        {
            month = 12;      
            year -= 1;       
        }
        //Đang có báo cáo tồn kho tháng trước
        var prevMonth = month - 1;
        var prevYear = year;
        if (prevMonth == 0)
        {
            prevMonth = 12;
            prevYear -= 1;
        }
        var reports = await _reportInventoryRepo.GetAllAsync();
        var prevReport = reports.FirstOrDefault(r => r.Month == prevMonth && r.Year == prevYear);

        List<ReportInventoryDetail> prevDetails = new();
        if (prevReport != null)
        {
            var details = await _reportInventoryDetailRepo.GetAllAsync();
            prevDetails = details.Where(d => d.ReportInventoryId == prevReport.Id).ToList();
        }
        //------------

        var result = inventories
          .GroupBy(x => x.ProductId)
          .Select(g => new BaseInventory
          {
              ProductId = g.Key,
              TotalQuantity = g.Sum(x => x.Quantity),
              LatestExpirationDate = g.Max(x => x.ExpirationDate)
          })
          .ToList();
        baseInventory = result.ToList();

        if (baseInventory.Count > 0)
        {
             reportInventorys.CreatedDate = DateTime.Now;
            reportInventorys.Month = month;
            reportInventorys.Year = year;
            foreach (var item in baseInventory)
            {


                var reportInventory = new LoadReportInventoryDetailDto();

                reportInventory.ProductId = item.ProductId;
                reportInventory.ExpirationDate = item.LatestExpirationDate;

              //  reportInventory.BeginningInventory = item.TotalQuantity;
                reportInventory.ProductName = product.First(x => x.Id == item.ProductId).Name;
                reportInventory.Unit = product.First(x => x.Id == item.ProductId).Unit;
                // ===== Gán BeginningInventory từ tháng trước nếu có =====

                var prevDetail = prevDetails.FirstOrDefault(d => d.ProductId == item.ProductId);
                if (prevDetail != null)
                {
                    reportInventory.BeginningInventory = prevDetail.TotalStock;
                    #region Lấy tổng số lượng mới nhập (NewInbound)
                    var newInbound = importDetails
                        .Join(imports,
                              detail => detail.WarehouseImportId,
                              import => import.Id,
                              (detail, import) => new { detail, import })
                        .Where(x => x.detail.ProductId == item.ProductId
                                    && x.import.CreatedDate.Year == year
                                    && x.import.CreatedDate.Month == month)
                        .Sum(x => x.detail.Quantity);
                    reportInventory.NewInbound = newInbound;
                    #endregion
                    // Lấy tổng thuốc xuất kho trong tháng trước
                    var consumExport = exportDetails
                        .Join(exports,
                              detail => detail.WarehouseExportId,
                              export => export.Id,
                              (detail, export) => new { detail, export })
                        .Where(x => x.detail.ProductId == item.ProductId
                                    && x.export.CreatedDate.Year == year
                                    && x.export.CreatedDate.Month == month)
                        .Sum(x => x.detail.Quantity);

                    // Lấy tổng thuốc đã dùng trong đơn thuốc trong tháng trước
                    var consumPrescription = prescriptionDetails
                        .Join(prescriptions,
                              detail => detail.PrescriptionId,
                              pres => pres.Id,
                              (detail, pres) => new { detail, pres })
                        .Where(x => x.detail.ProductId == item.ProductId
                                    && x.pres.CreatedDate.Year == year
                                    && x.pres.CreatedDate.Month == month)
                        .Sum(x => x.detail.Quantity);

                    // Cộng lại
                    reportInventory.ConsumPosition = consumExport + consumPrescription;
                }    
                   

                else
                    reportInventory.BeginningInventory = item.TotalQuantity;
                //=================================================================
                //#region Lấy tổng số lượng mới nhập (NewInbound)
                //var newInbound = importDetails
                //    .Join(imports,
                //          detail => detail.WarehouseImportId,
                //          import => import.Id,
                //          (detail, import) => new { detail, import })
                //    .Where(x => x.detail.ProductId == item.ProductId
                //                && x.import.CreatedDate.Year == year
                //                && x.import.CreatedDate.Month == month)
                //    .Sum(x => x.detail.Quantity);
                //reportInventory.NewInbound = newInbound;
                //#endregion
                //// Lấy tổng thuốc xuất kho trong tháng trước
                //var consumExport = exportDetails
                //    .Join(exports,
                //          detail => detail.WarehouseExportId,
                //          export => export.Id,
                //          (detail, export) => new { detail, export })
                //    .Where(x => x.detail.ProductId == item.ProductId
                //                && x.export.CreatedDate.Year == year
                //                && x.export.CreatedDate.Month == month)
                //    .Sum(x => x.detail.Quantity);

                //// Lấy tổng thuốc đã dùng trong đơn thuốc trong tháng trước
                //var consumPrescription = prescriptionDetails
                //    .Join(prescriptions,
                //          detail => detail.PrescriptionId,
                //          pres => pres.Id,
                //          (detail, pres) => new { detail, pres })
                //    .Where(x => x.detail.ProductId == item.ProductId
                //                && x.pres.CreatedDate.Year == year
                //                && x.pres.CreatedDate.Month == month)
                //    .Sum(x => x.detail.Quantity);

                //// Cộng lại
                //reportInventory.ConsumPosition = consumExport + consumPrescription;
                reportInventory.TotalStock = reportInventory.BeginningInventory
                            + reportInventory.NewInbound
                            - reportInventory.ConsumPosition;
                ////data Symple


                //
                reportInventorys.LoadReportInventoryDetail.Add(reportInventory);
            }
        }


        return reportInventorys;

    }
    private class BaseInventory
    {
        public Guid ProductId { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateTime? LatestExpirationDate { get; set; }

    }
}
