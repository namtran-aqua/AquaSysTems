using AquaSolution.Server.Services.KPi.KPITaskService;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.KPITask
{
    [ApiController]
    [Route("api/[controller]")]
    public class KPIMonthlyTargetController : ControllerBase
    {
        private readonly IKPIMonthlyTargetService _KPIMonthlyTargetService;

        public KPIMonthlyTargetController(IKPIMonthlyTargetService KPIMonthlyTargetService)
        {
            _KPIMonthlyTargetService = KPIMonthlyTargetService;
        }

        [HttpGet("get-kpi-target/{taskid}/{userId}")]
        public async Task<IActionResult> GetAsync(Guid taskid,Guid userId)
        {
            var result = await _KPIMonthlyTargetService.GetTargetByTask(taskid, userId);
            return Ok(result);
        }

    }
}
