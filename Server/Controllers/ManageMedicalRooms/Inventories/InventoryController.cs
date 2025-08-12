using AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.Inventories
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
    
        private readonly IInventoryService _inventoryService;
        public InventoryController(IInventoryService inventoryService) 
        {
            _inventoryService = inventoryService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _inventoryService.LoadListAsync();
            return Ok(result);
        }

    }
}
