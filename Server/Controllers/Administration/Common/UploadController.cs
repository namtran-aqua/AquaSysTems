using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Administration.Common
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Local file endpoints removed to avoid conflict.

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars", fileName);
            
            var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var url = $"{baseUrl}/uploads/avatars/{fileName}";
            return Ok(url);
        }

        [HttpDelete("delete-avatar")]
        public IActionResult DeleteAvatar([FromQuery] string avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl))
                return BadRequest("Avatar URL is required.");

            avatarUrl = Uri.UnescapeDataString(avatarUrl);
            var appPath = HttpContext.Request.PathBase.HasValue
                ? HttpContext.Request.PathBase.Value
                : string.Empty;

            if (!string.IsNullOrEmpty(appPath) && avatarUrl.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
            {
                avatarUrl = avatarUrl.Substring(appPath.Length);
            }
            if (avatarUrl.StartsWith("/"))
                avatarUrl = avatarUrl.Substring(1);


            var physicalPath = Path.Combine(_env.WebRootPath, avatarUrl);
            var fileName = Path.GetFileName(physicalPath);
            if (string.Equals(fileName, "default.jpg", StringComparison.OrdinalIgnoreCase))
            {
                return NoContent();
            }
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                return Ok(new { message = "Avatar deleted successfully." });
            }

            return NotFound("Avatar file not found.");
        }

    }

}
