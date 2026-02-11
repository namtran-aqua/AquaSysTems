using AntDesign;
using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Server.Services.ePAD;
using AquaSolution.Server.Services.SemiReport.CusPackService;
using AquaSolution.Server.Services.SemiReport.SemiReportService;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.SemiReport;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NPOI.HSSF.Record.Chart;

namespace AquaSolution.Server.Controllers.SemiReport
{
    [ApiController]
    [Route("api/[controller]")]
    public class SemiController : ControllerBase
    { 
        private readonly ISemiService _semiService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public SemiController(ISemiService semiService, IHubContext<SignalrHub> hubContext)
        {
            _semiService = semiService;
            _hubContext = hubContext;
        }
        [HttpGet("get-all-semi-data")]
        public async Task<ActionResult> GetAll()
        {
            var data = await _semiService.GetAllAsync();
            var isInner4Hour = await _semiService.GetInner4HourStatusAsync();

            await _hubContext.Clients.All.SendAsync("LoadSemiReport");

            return Ok(new
            {
                Data = data,
                IsInner4Hour = isInner4Hour
            });
        }
        [HttpPost("update-active-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] bool isActive)
        {
            var success = await _semiService.UpdateInner4HourStatusAsync(isActive);

            if (success)
            {
                await _hubContext.Clients.All.SendAsync("StatusChanged", isActive);
                return Ok();
            }

            return BadRequest("Cập nhật database thất bại.");
        }
    }
    
}
