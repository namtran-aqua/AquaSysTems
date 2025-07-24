using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Common
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
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
            var url = $"/uploads/avatars/{fileName}";
            return Ok(url);
        }
    }

}
