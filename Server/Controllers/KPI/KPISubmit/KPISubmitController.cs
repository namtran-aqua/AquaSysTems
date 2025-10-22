using AquaSolution.Server.Services.KPI.KPISubmit;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPISubmit;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.KPISubmit
{
    [ApiController]
    [Route("api/[controller]")]
    public class KPISubmitController : ControllerBase
    {
        private readonly IKPISubmitService _kPISubmitService;

        public KPISubmitController(IKPISubmitService kPISubmitService)
        {
            _kPISubmitService = kPISubmitService;
        }
        [HttpGet("get-kpi-submit/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetAsync( Guid userId ,int year, int? month)
        {
            var result = await _kPISubmitService.GetHandleKPISubmitByUserId( userId,year,month);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] List<HandleKPISubmitDto> submitKPIDto)
        {
            var result = await _kPISubmitService.SubmitKPIAsync(submitKPIDto);
            return result ? Ok(true) : BadRequest("New creation failed");
        }
        [HttpGet("get-resul-kpi/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetResult(Guid userId, int year, int? month)
        {
            var result = await _kPISubmitService.GetKPITotalScoreByUserId(userId, year, month);
            return Ok(result);
        }
        [HttpGet("get-result-quarter/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetQuarter(Guid userId, int year, int? quarter)
        {
            var result = await _kPISubmitService.GetKPITotalScoreQuarterByUserId(userId, year, quarter);
            return Ok(result);
        }
        [HttpGet("get-indexweight/{positionType}/{periodType}")]
        public async Task<IActionResult> GetIndexWeight(PositionType positionType,PeriodType periodType )
        {
            var result = await _kPISubmitService.GetIndexWeight(positionType, periodType);
            return Ok(result);
        }
    }
}
