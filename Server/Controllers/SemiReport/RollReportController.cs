using AquaSolution.Server.Services.SemiReport.RollReportService;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.SemiReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Controllers.SemiReport
{
    [ApiController]
    [Route("api/[controller]")]
    public class RollReportController : ControllerBase
    {
        private readonly IRollReportService _rollService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public RollReportController(IRollReportService rollService, IHubContext<SignalrHub> hubContext)
        {
            _rollService = rollService;
            _hubContext = hubContext;
        }
        [HttpGet("get-all-roll-data")]
        public async Task<List<RollReportDto?>> GetAll()
        {
            var data = await _rollService.GetAllAsync();
            await _hubContext.Clients.All.SendAsync("LoadRollReport");
            return data;
        }
    }
}
