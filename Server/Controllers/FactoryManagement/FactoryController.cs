using AquaSolution.Server.Services.FactoryService;
using AquaSolution.Shared.Factory;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.FactoryManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryController : ControllerBase
    {
        private readonly IFactoryService _factoryService;

        public FactoryController(IFactoryService factoryService)
        {
            _factoryService = factoryService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _factoryService.GetListFactory();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FactoryDto factoryDto)
        {
            var success = await _factoryService.CreatedFactory(factoryDto);
            if (!success)
                return BadRequest("Failed to create factory.");
            return Ok(true);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] FactoryDto factoryDto)
        {
            var success = await _factoryService.UpdateFactory(factoryDto);
            if (!success)
                return BadRequest("Failed to update factory.");
            return Ok(true);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _factoryService.DeleteFactory(id);
            if (!success)
                return BadRequest("Failed to delete factory.");
            return Ok(true);
        }
    }
}
