using AquaSolution.Server.Services.Common.HandleInventories;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController :ControllerBase
    {
        private readonly IHandleInventory _handleInventory;
        public CommonController
            (
            IHandleInventory handleInventory
            ) 
        {
            _handleInventory = handleInventory;
        }
        [HttpGet("get-code-inventoryPeriod")]
        public async Task<IActionResult> GetCodeInventoryPeriod()
        {
            var result = await _handleInventory.GenerateInventoryCodeAsync();
            return new JsonResult(result);
        }
    }

}
