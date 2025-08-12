using AquaSolution.Server.Services.ManageMedicalRooms.WarehouseImportService;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.WarehouseImport
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseImportController : ControllerBase
    {
        private readonly IWarehouseImportService _warehouseImportService;

        public WarehouseImportController(IWarehouseImportService warehouseImportService)
        {
            _warehouseImportService = warehouseImportService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportWarehouse([FromBody] CreatedWarehouseImportDto createdWarehouseImportDto)
        {
            if (createdWarehouseImportDto == null)
                return BadRequest("Invalid data");

            var result = await _warehouseImportService.WarehouseImport(createdWarehouseImportDto);

            if (result)
                return Ok(new { success = true, message = "Warehouse import successful" });
            else
                return StatusCode(500, new { success = false, message = "Warehouse import failed" });
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _warehouseImportService.GetWarehouseImport();
            return Ok(result);
        }
        [HttpGet("get-detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _warehouseImportService.GetWarehouseImportDetail(id);
            return Ok(result);
        }

    }
}
