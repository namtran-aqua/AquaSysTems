using Microsoft.AspNetCore.Mvc;
using AquaSolution.Server.Services.ImgsService;
using Microsoft.EntityFrameworkCore;
using AquaSolution.Data.Connection;
namespace AquaSolution.Server.Controllers.IGMManagement
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Uncomment this if you want to require authentication
    public class UploadController : ControllerBase
    {
        private readonly AquaDbContext _context;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IImageCleanupScheduler _cleanupScheduler;

        public UploadController(AquaDbContext context, IGoogleDriveService googleDriveService, IImageCleanupScheduler cleanupScheduler)
        {
            _context = context;
            _googleDriveService = googleDriveService;
            _cleanupScheduler = cleanupScheduler;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string WorkDayId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file tải lên." });

            if (string.IsNullOrWhiteSpace(WorkDayId))
                return BadRequest(new { message = "WorkDayId không được để trống." });

            try
            {
                // Kiểm tra WorkDayId có tồn tại trong Database không
                var isValidWorkDayId = await _context.tbl_Users
                    .AnyAsync(u => u.WorkDayId == WorkDayId && !u.IsDeleted);

                if (!isValidWorkDayId)
                    return BadRequest(new { message = $"WorkDayId '{WorkDayId}' không hợp lệ hoặc đã bị xóa khỏi hệ thống." });

                // Upload ảnh qua Service
                var (url, fileId) = await _googleDriveService.UploadAsync(file, WorkDayId);

                // Lên lịch tự động xóa sau 2 ngày
                var jobId = _cleanupScheduler.ScheduleCleanup(fileId, TimeSpan.FromDays(2));
                
                return Ok(new 
                { 
                    url = url,
                    fileId = fileId,
                    jobId = jobId,
                    message = "Upload thành công! Đã lên lịch xóa tự động sau 2 ngày." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet("workday-ids")]
        public async Task<IActionResult> GetWorkDayIds()
        {
            try
            {
                var workDayIds = await _context.tbl_Users
                    .Where(u => !string.IsNullOrEmpty(u.WorkDayId) && !u.IsDeleted)
                    .Select(u => u.WorkDayId!)
                    .Distinct()
                    .ToListAsync();
                    
                return Ok(workDayIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi khi lấy danh sách WorkDayId: {ex.Message}" });
            }
        }


    }
}
