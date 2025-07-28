using AquaSolution.Server.Services.PositionService;
using AquaSolution.Shared.Position;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.PositionManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _positionService.GetListPosition();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PositionDto positionDto)
        {
            var success = await _positionService.CreatedPosition(positionDto);
            if (!success)
                return BadRequest("Failed to create position.");
            return Ok(true);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] PositionDto positionDto)
        {
            var success = await _positionService.UpdatePosition(positionDto);
            if (!success)
                return BadRequest("Failed to update position.");
            return Ok(true);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _positionService.DeletePosition(id);
            if (!success)
                return BadRequest("Failed to delete position.");
            return Ok(true);
        }
    }
}
