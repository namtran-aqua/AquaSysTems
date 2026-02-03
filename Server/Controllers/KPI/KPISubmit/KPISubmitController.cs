using AquaSolution.Server.Services.KPI.KPISubmit;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Controllers.KPI.KPISubmit
{
    [ApiController]
    [Route("api/[controller]")]
    public class KPISubmitController : ControllerBase
    {
        private readonly IKPISubmitService _kPISubmitService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public KPISubmitController(IKPISubmitService kPISubmitService, IHubContext<SignalrHub> hubContext)
        {
            _kPISubmitService = kPISubmitService;
            _hubContext = hubContext;
        }
        [HttpGet("get-kpi-submit/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetAsync(Guid userId, int year, int? month)
        {
            var result = await _kPISubmitService.GetHandleKPISubmitByUserId(userId, year, month);
            return Ok(result);
        }
        [HttpPost("create/{month}")]
        public async Task<IActionResult> CreateAsync([FromBody] HandleKPISubmitDto submitKPIDto, [FromRoute] int month)
        {
            var result = await _kPISubmitService.SubmitKPIAsync(submitKPIDto,month);
            await _hubContext.Clients.All.SendAsync("ReloadKPIForUserApproval");
            return result ? Ok(true) : BadRequest("New creation failed");
        }
        [HttpGet("get-result-kpi/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetResult(Guid userId, int year, int? month)
        {
            var result = await _kPISubmitService.GetKPITotalScoreByUserId(userId, year, month);
            return Ok(result);
        }
        [HttpGet("get-result-quarter/{userId}/{year}/{quarter}")]
        public async Task<IActionResult> GetQuarter(Guid userId, int year, int? quarter)
        {
            var result = await _kPISubmitService.GetKPITotalScoreQuarterByUserId(userId, year, quarter);
            return Ok(result);
        }
        [HttpGet("get-indexweight/{positionType}/{periodType}")]
        public async Task<IActionResult> GetIndexWeight(PositionType positionType, PeriodType periodType)
        {
            var result = await _kPISubmitService.GetIndexWeight(positionType, periodType);
            return Ok(result);
        }
        [HttpGet("get-result-detail/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetOMG(Guid userId, int year, int month)
        {
            var result = await _kPISubmitService.GetResultDetail(userId, year, month);
            return Ok(result);
        }
        [HttpGet("get-kpi-total-score-by-userid/{userId}")]
        public async Task<IActionResult> GetKPITotalScoreByUserId(Guid userId)
        {
            var result = await _kPISubmitService.GetKPITotalScoreByUserId(userId);
            return Ok(result);
        }
        [HttpGet("get-kpi-approval")]
        public async Task<IActionResult> GetKPIForApproval()
        {
            var result = await _kPISubmitService.GetKPIForApproval();
            return Ok(result);
        }
        [HttpGet("get-process-by-submitid/{submitid}")]
        public async Task<IActionResult> GetProcessApprovalBySubmitIdAsync(Guid submitid)
        {
            var result = await _kPISubmitService.GetProcessApprovalBySubmitIdAsync(submitid);
            return Ok(result);
        }
        [HttpGet("get-detail-by-submitid/{submitid}")]
        public async Task<IActionResult> GetDetailKPIBySubmitId(Guid submitid)
        {

            var result2 = await _kPISubmitService.GetDetailKPIBySubmitId(submitid);
            return Ok(result2);
        }
        //Update-Status
        [HttpPut("update-status-request-kpi")]
        public async Task<IActionResult> UpdateAsync([FromBody] ApprovalInfo dto)
        {
            var result = await _kPISubmitService.HandleKpiForApproval(dto);
            await _hubContext.Clients.All.SendAsync("ReloadKPIForUserApproval");
            return result ? Ok(true) : BadRequest("Update KPI Task failed.");
        }
        //Get Result
        [HttpGet("get-result-kpi")]
        public async Task<IActionResult> ResultAllKpi()
        {
            var result = await _kPISubmitService.ResultAllKpi();
            return Ok(result);
        }
        //CalculatePointQuarter
        [HttpPost("calculate-point-quarter")]
        public async Task<IActionResult> CalculatePointQuarter([FromBody] List<CalculateQuarterPointDto> calculateQuarterPoint)
        {
            var result = await _kPISubmitService.CalculateQuarterPoint(calculateQuarterPoint);
            return result ? Ok(true) : BadRequest("New creation failed");
        }
    }
}
