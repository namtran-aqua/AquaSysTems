using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.InventoryPeriod
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryPeriodController : ControllerBase
    {
        private readonly IInventoryPeriodService _inventoryPeriodService;
        public InventoryPeriodController(IInventoryPeriodService inventoryPeriodService)
        {
            _inventoryPeriodService = inventoryPeriodService;
        }
        [HttpPost("create-inventoryPeriod")]
        public async Task<IActionResult> Create([FromBody] CreatedInventoryPeriodDto createdInventoryPeriodDto)
        {
            var inventoryPeriodId = await _inventoryPeriodService.InsertDetailInventoryPeriod(createdInventoryPeriodDto);

            if (inventoryPeriodId == Guid.Empty)
                return BadRequest($"Inventory check for month {createdInventoryPeriodDto.InventoryPeriodDto.Month} of year {createdInventoryPeriodDto.InventoryPeriodDto.Year} already exists. The month can be changed");

            return Ok(inventoryPeriodId);
        }
        [HttpGet("get-inventory-period-detail/{inventoryPeriodId}")]
        public async Task<IActionResult> GetInventoryPeriodDetail(Guid inventoryPeriodId)
        {
            var result = await _inventoryPeriodService.GetInventoryPeriodDetail(inventoryPeriodId);
            return Ok(result);
        }
        [HttpGet("get-inventory-period")]
        public async Task<IActionResult> GetAllInventoryPeriod()
        {
            var result = await _inventoryPeriodService.GetAllInventoryPeriod();
            return Ok(result);
        }
        [HttpPut("update-actual-inventory-period")]
        public async Task<IActionResult> InsertActualInventoryPeriod([FromBody] List<InventoryPeriodDetailDto> inventoryPeriodDetailDtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryPeriodService.InsertActualInventoryPeriod(inventoryPeriodDetailDtos);

            if (result)
                return Ok(new { success = true, message = "Cập nhật thành công" });

            return BadRequest(new { success = false, message = "Cập nhật thất bại" });
        }

    }
}
