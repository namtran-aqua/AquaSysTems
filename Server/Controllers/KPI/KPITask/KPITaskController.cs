using AquaSolution.Server.Services.KPi.KPITaskService;
using AquaSolution.Shared.KPI.KPITasks;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.KPITask
{
    [ApiController]
    [Route("api/[controller]")]
    public class KPITaskController : ControllerBase
    {
        private readonly IKPITaskService _kpiTaskService;

        public KPITaskController(IKPITaskService kpiTaskService)
        {
            _kpiTaskService = kpiTaskService;
        }

        // GET: api/kpitask
        [HttpGet("get-list")]
        public async Task<IActionResult> GetListAsync()
        {
            var list = await _kpiTaskService.LoadListAsync();
            return Ok(list);
        }

        // POST: api/kpitask
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] HandleTaskDto dto)
        {
            var result = await _kpiTaskService.CreatedAsync(dto);
            return result ? Ok(true) : BadRequest("Creating KPI Task failed.");
        }

        // PUT: api/kpitask
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] HandleTaskDto dto)
        {
            var result = await _kpiTaskService.UpdateAsync(dto);
            return result ? Ok(true) : BadRequest("Update KPI Task failed.");
        }

        // DELETE: api/kpitask/{id}
        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _kpiTaskService.DeletedAsync(id);
            return result ? Ok(true) : BadRequest("Delete failed or not found KPI Task.");
        }
    }
}
