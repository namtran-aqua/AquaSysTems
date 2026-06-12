
using AquaSolution.Server.Services.ScrapManagetment.ScapServices;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.ScrapManagement.Scrap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;


namespace AquaSolution.Server.Controllers.ScrapManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapController : ControllerBase
    {
        private readonly IScrapService _scrapService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public ScrapController(IScrapService scrapService,
            IHubContext<SignalrHub> hubContext)
        {
            _scrapService = scrapService;
            _hubContext = hubContext;
        }


        [HttpGet("get-all-scraps")]
        public async Task<IActionResult> GetAllScraps()
        {
            try
            {
                var result = await _scrapService.GetHistory();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpPost("create-scrap")]
        public async Task<IActionResult> CreateScrap([FromBody] HandleScrapDto createScrapDto)
        {
            try
            {
                await _scrapService.CreateScrap(createScrapDto);
                await _hubContext.Clients.All.SendAsync("LoadScrap");
                return Ok(new { message = "Tạo scrap thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        //[HttpGet("get-scrap-for-approval/{userId}")]
        //public async Task<IActionResult> GetScrapForApproval(Guid userId)
        //{
        //    try
        //    {
        //        var historyScraps = await _scrapService.GetScrapForApproval(userId);
        //        return Ok(historyScraps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("get-scrap-for-approval")]
        public async Task<IActionResult> GetScrapForApproval(
            [FromQuery] Guid userId,
            [FromQuery] bool isAdmin)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new { message = "UserId không hợp lệ." });

                var result = await _scrapService.GetScrapForApproval(userId, isAdmin);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("action-approval")]
        public async Task<IActionResult> ActionApproval([FromBody] ApprovalActionDto request)
        {
            try
            {
                await _scrapService.ActionApproval(request);
                await _hubContext.Clients.All.SendAsync("LoadScrap");
                return Ok(new { message = "Thao tác thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-scrap-for-confirm")]
        public async Task<IActionResult> GetScrapForConfirm()
        {
            try
            {
                var result = await _scrapService.GetScrapForConfirm();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpPost("confirm-scrap")]
        public async Task<IActionResult> ConfirmScrap([FromBody] ConfirmScrapDto request)
        {
            try
            {
                await _scrapService.ConfirmScrap(request);
                return Ok(new { message = "Xác nhận thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
