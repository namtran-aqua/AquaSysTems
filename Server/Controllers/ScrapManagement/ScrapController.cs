
using AquaSolution.Server.Services.ScrapManagetment.ScapServices;
using AquaSolution.Shared.ScrapManagement.Scrap;
using Microsoft.AspNetCore.Mvc;


namespace AquaSolution.Server.Controllers.ScrapManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapController : ControllerBase
    {
        private readonly IScrapService _scrapService;

        public ScrapController(IScrapService scrapService)
        {
            _scrapService = scrapService;
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
                return Ok(new { message = "Tạo scrap thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpGet("get-scrap-for-approval/{userId}")]
        public async Task<IActionResult> GetScrapForApproval(Guid userId)
        {
            try
            {
                var historyScraps = await _scrapService.GetScrapForApproval(userId);
                return Ok(historyScraps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("action-approval")]
        public async Task<IActionResult> ActionApproval([FromBody] ApprovalActionDto request)
        {
            try
            {
                await _scrapService.ActionApproval(request);
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
