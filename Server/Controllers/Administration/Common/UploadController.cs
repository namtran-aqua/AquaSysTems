using AquaSolution.Server.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Controllers.Administration.Common
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<SignalrHub> _hubContext;
        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine("wwwroot/uploads/avatars", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/uploads/avatars/{fileName}";
            return Ok(url);
        }
        [HttpDelete("delete-avatar")]
        public IActionResult DeleteAvatar([FromQuery] string avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl))
                return BadRequest("Avatar URL is required.");

            var relativePath = avatarUrl.Replace("/uploads/", "uploads/");
            var physicalPath = Path.Combine(_env.WebRootPath, relativePath);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                return Ok(new { message = "Avatar deleted successfully." });
            }

            return NotFound("Avatar file not found.");
        }
    }

}
