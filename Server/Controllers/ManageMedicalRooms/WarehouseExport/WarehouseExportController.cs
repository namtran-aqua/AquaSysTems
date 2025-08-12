using AquaSolution.Server.Services.ManageMedicalRooms.WarehouseExportService;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.WarehouseExport
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseExportController : ControllerBase
    {
        private readonly IWarehouseExportService _warehouseExportService;

        public WarehouseExportController(IWarehouseExportService warehouseExportService)
        {
            _warehouseExportService = warehouseExportService;
        }

        [HttpPost("export")]
        public async Task<IActionResult> ImportWarehouse([FromBody] CreatedWarehouseExportDto createdWarehouseExportDto)
        {
            try
            {
                var result = await _warehouseExportService.WarehouseExport(createdWarehouseExportDto);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                // Trả về lỗi tồn kho không đủ
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi chung
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _warehouseExportService.GetWarehouseExport();
            return Ok(result);
        }
        [HttpGet("get-detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _warehouseExportService.GetWarehouseExportDetail(id);
            return Ok(result);
        }

    }
}
