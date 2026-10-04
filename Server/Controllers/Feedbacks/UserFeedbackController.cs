using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using AquaSolution.Server.Services.Feedbacks;
using AquaSolution.Shared.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Feedbacks
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFeedbackController : ControllerBase
    {
        private readonly IUserFeedbackService _feedbackService;
        private readonly IWebHostEnvironment _env;

        public UserFeedbackController(
            IUserFeedbackService feedbackService,
            IWebHostEnvironment env)
        {
            _feedbackService = feedbackService;
            _env = env;
        }

        /// <summary>
        /// Public endpoint to get list of departments for dropdown
        /// </summary>
        [AllowAnonymous]
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var depts = await _feedbackService.GetDepartmentListAsync();
            return Ok(depts);
        }

        /// <summary>
        /// Public endpoint to get list of factories for dropdown
        /// </summary>
        [AllowAnonymous]
        [HttpGet("factories")]
        public async Task<IActionResult> GetFactories()
        {
            var factories = await _feedbackService.GetFactoryListAsync();
            return Ok(factories);
        }

        /// <summary>
        /// Public endpoint to submit feedback with multiple files (No login required)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("submit")]
        [RequestSizeLimit(262144000)] // 250MB max total request size
        [RequestFormLimits(MultipartBodyLengthLimit = 262144000)]
        public async Task<IActionResult> SubmitFeedback(
            [FromForm] string fullName,
            [FromForm] int? age,
            [FromForm] string? factory,
            [FromForm] string department,
            [FromForm] string? phoneNumber,
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] List<IFormFile>? files)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return BadRequest("Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(department))
                return BadRequest("Phòng ban không được để trống.");

            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Tiêu đề không được để trống.");

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Nội dung không được để trống.");

            var attachments = new List<UserFeedbackAttachmentCreateDto>();

            if (files != null && files.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "feedbacks");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var originalName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid():N}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var physicalPath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativeUrl = $"/uploads/feedbacks/{uniqueFileName}";

                    attachments.Add(new UserFeedbackAttachmentCreateDto
                    {
                        FileName = uniqueFileName,
                        OriginalFileName = originalName,
                        FilePath = relativeUrl,
                        FileType = extension,
                        FileSize = file.Length
                    });
                }
            }

            var createDto = new UserFeedbackCreateDto
            {
                FullName = fullName,
                Age = age,
                Factory = factory ?? string.Empty,
                Department = department,
                PhoneNumber = phoneNumber,
                Title = title,
                Content = content,
                Attachments = attachments
            };

            var result = await _feedbackService.SubmitFeedbackAsync(createDto);
            return Ok(new { success = true, data = result, message = "Gửi ý kiến thành công!" });
        }

        /// <summary>
        /// Admin endpoint: Get paged feedbacks with filters
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetPaged([FromQuery] UserFeedbackFilterDto filter)
        {
            var result = await _feedbackService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Get feedback by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _feedbackService.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy ý kiến.");
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Get dashboard stats
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _feedbackService.GetDashboardStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Update status and admin note
        /// </summary>
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UserFeedbackStatusUpdateDto dto)
        {
            var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value 
                                  ?? User.FindFirst("name")?.Value 
                                  ?? User.Identity?.Name 
                                  ?? "Admin";

            var success = await _feedbackService.UpdateStatusAsync(dto, currentUserName);
            if (!success) return BadRequest("Cập nhật trạng thái thất bại.");
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công." });
        }

        /// <summary>
        /// Admin endpoint: Delete feedback
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feedbackService.DeleteAsync(id);
            if (!success) return NotFound("Không tìm thấy ý kiến cần xóa.");
            return Ok(new { success = true, message = "Xóa ý kiến thành công." });
        }
    }
}
