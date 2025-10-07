using AquaSolution.Server.Services.KPi.QuaterCalculatedService;
using AquaSolution.Shared.KPI.QuaterCalculated;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.QuaterCalculated
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuaterCalculatedController : ControllerBase
    {
        private readonly IQuaterCalculatedService _quaterCalculatedService;

        public QuaterCalculatedController(IQuaterCalculatedService quaterCalculatedService)
        {
            _quaterCalculatedService = quaterCalculatedService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetListAsync()
        {
            var list = await _quaterCalculatedService.LoadListAsync();
            return Ok(list);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] QuaterCalculatedDto QuaterCalculatedDto)
        {
            var result = await _quaterCalculatedService.CreatedAsync(QuaterCalculatedDto);
            return result ? Ok(true) : BadRequest("New creation failed");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] QuaterCalculatedDto QuaterCalculatedDto)
        {
            var result = await _quaterCalculatedService.UpdateAsync(QuaterCalculatedDto);
            return result ? Ok(true) : BadRequest("Update failed");
        }

        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> DeleteAsync(Guid Id)
        {
            var result = await _quaterCalculatedService.DeletedAsync(Id);
            return result ? Ok(true) : BadRequest("Delete failed or not found");
        }
    }
}
