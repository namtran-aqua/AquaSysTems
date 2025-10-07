using AquaSolution.Server.Services.KPi.FormulaService;
using AquaSolution.Shared.KPI.Formula;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.Formula
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormulaController : ControllerBase
    {
        private readonly IFormulaService _formulaService;

        public FormulaController(IFormulaService formulaService)
        {
            _formulaService = formulaService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetListAsync()
        {
            var list = await _formulaService.LoadListAsync();
            return Ok(list);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] FormulaDto formulaDto)
        {
            var result = await _formulaService.CreatedAsync(formulaDto);
            return result ? Ok(true) : BadRequest("New creation failed");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] FormulaDto formulaDto)
        {
            var result = await _formulaService.UpdateAsync(formulaDto);
            return result ? Ok(true) : BadRequest("Update failed");
        }

        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> DeleteAsync(Guid Id)
        {
            var result = await _formulaService.DeletedAsync(Id);
            return result ? Ok(true) : BadRequest("Delete failed or not found");
        }
    }
}
