using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AquaSolution.Server.Services.KPI.KPIScope;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Scope;

namespace AquaSolution.Server.Controllers.KPI.KPIScope
{
    [ApiController]
    [Route("api/[controller]")]
    public class KPIScopeController : ControllerBase
    {
        private readonly IKPIScopeService _scopeService;

        public KPIScopeController(IKPIScopeService scopeService)
        {
            _scopeService = scopeService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetListAsync([FromQuery] int? year, [FromQuery] Guid? departmentId, [FromQuery] Guid? userId, [FromQuery] bool? isActive)
        {
            var result = await _scopeService.GetScopesAsync(year, departmentId, userId, isActive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _scopeService.GetScopeByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy hồ sơ.");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] KPIScopeCreateOrUpdateDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _scopeService.CreateScopeAsync(dto, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] KPIScopeCreateOrUpdateDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _scopeService.UpdateScopeAsync(dto, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("deactivate/{id}")]
        public async Task<IActionResult> DeactivateAsync(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _scopeService.DeactivateScopeAsync(id, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("suggest-months")]
        public IActionResult SuggestMonths([FromQuery] KPIScopeType scopeType, [FromQuery] DateTime? effectiveDate, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = _scopeService.SuggestExemptMonths(scopeType, effectiveDate, startDate, endDate);
            return Ok(result);
        }

        [HttpGet("compliance-matrix")]
        public async Task<IActionResult> GetComplianceMatrixAsync([FromQuery] int year, [FromQuery] Guid? departmentId)
        {
            if (year <= 2000) year = DateTime.Now.Year;
            var result = await _scopeService.GetMonthlyComplianceMatrixAsync(year, departmentId);
            return Ok(result);
        }

        [HttpGet("user-exempt-months/{userId}/{year}")]
        public async Task<IActionResult> GetUserExemptMonthsAsync(Guid userId, int year)
        {
            var result = await _scopeService.GetUserExemptMonthsAsync(userId, year);
            return Ok(result);
        }
    }
}
