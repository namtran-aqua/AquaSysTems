
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
        public async Task<IActionResult> ConfirmScrap([FromBody] ActionConfirmScrapDto request)
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

        [HttpGet("get-all-waste-items")]
        public async Task<IActionResult> GetAllWasteItems()
        {
            try
            {
                var result = await _scrapService.GetAllWasteItemsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpGet("preview-flow")]
        public async Task<IActionResult> PreviewFlow(
            [FromQuery] Guid factoryId,
            [FromQuery] Guid departmentId,
            [FromQuery] Guid? sectionId,
            [FromQuery] Guid creatorId)
        {
            try
            {
                var result = await _scrapService.PreviewApprovalFlow(factoryId, departmentId, sectionId, creatorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("resubmit-scrap")]
        public async Task<IActionResult> ResubmitScrap([FromBody] HandleScrapDto resubmitDto)
        {
            try
            {
                await _scrapService.ResubmitScrap(resubmitDto);
                await _hubContext.Clients.All.SendAsync("LoadScrap");
                return Ok(new { message = "Cập nhật và nộp lại thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-receivers")]
        public async Task<IActionResult> GetReceivers()
        {
            try
            {
                var result = await _scrapService.GetScrapReceivers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("save-receiver")]
        public async Task<IActionResult> SaveReceiver([FromBody] AquaSolution.Shared.ScrapManagement.Receivers.SaveScrapReceiverDto dto)
        {
            try
            {
                await _scrapService.SaveScrapReceiver(dto);
                return Ok(new { message = "Lưu cấu hình người nhận hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-receiver/{id}")]
        public async Task<IActionResult> DeleteReceiver(Guid id)
        {
            try
            {
                await _scrapService.DeleteScrapReceiver(id);
                return Ok(new { message = "Xóa cấu hình thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
