using AquaSolution.Server.Services.KPi.CeilingLevelService;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.CeilingLevel
{
    [ApiController]
    [Route("api/[controller]")]
    public class CeilingLevelController : ControllerBase
    {
        private readonly ICeilingLevelService _CeilingLevelService;

        public CeilingLevelController(ICeilingLevelService CeilingLevelService)
        {
            _CeilingLevelService = CeilingLevelService;
        }

        [HttpGet("ceilingLevel-by-userId/{userId}")]
        public async Task<IActionResult> CeilingLevelByUserId(Guid userId)
        {
            var list = await _CeilingLevelService.CeilingLevelByUserId(userId);
            return Ok(list);
        }

       
    }
}
