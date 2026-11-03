using AquaSolution.Server.Services.ImgsService;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AquaSolution.Server.Controllers.Imgs
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImgController : ControllerBase
    {
        private readonly IGoogleDriveService _driveService;
        private readonly IFolderPermissionService _permissionService;

        public ImgController(IGoogleDriveService driveService, IFolderPermissionService permissionService)
        {
            _driveService = driveService;
            _permissionService = permissionService;
        }

        [HttpGet("get-all-img")]
        public async Task<IActionResult> GetAllImg([FromQuery] string? workDayId, [FromQuery] bool isAdmin = false)
        {
            try
            {
                List<string> allowedFolders = new List<string>();

                if (!isAdmin && !string.IsNullOrWhiteSpace(workDayId))
                {
                    var perms = await _permissionService.GetPermissionsByWorkDayIdAsync(workDayId);
                    allowedFolders = perms.Select(p => p.FolderName).ToList();
                }
                
                var list = await _driveService.GetAllImagesAsync(isAdmin ? null : allowedFolders);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteImage([FromQuery] string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return BadRequest("Missing fileId");

            try
            {
                var result = await _driveService.DeleteFileAsync(fileId);

                if (!result)
                    return NotFound("Image not found or already deleted");

                return Ok("Deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("thumbnail/{fileId}")]
        [HttpGet("view/{fileId}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetThumbnail(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return BadRequest("Missing fileId");

            try
            {
                var bytes = await _driveService.GetFileBytesAsync(fileId);
                if (bytes == null || bytes.Length == 0)
                    return NotFound("Image not found or empty");
                var meta = await _driveService.GetFileMetadataAsync(fileId);
                Response.Headers["Cache-Control"] = "public, max-age=86400";
                var mimeType = !string.IsNullOrWhiteSpace(meta?.MimeType) ? meta.Value.MimeType : "image/jpeg";
                return File(bytes, mimeType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadImage(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return BadRequest("Missing fileId");

            try
            {
                var meta = await _driveService.GetFileMetadataAsync(fileId);
                var bytes = await _driveService.GetFileBytesAsync(fileId);
                if (bytes == null || bytes.Length == 0)
                    return NotFound("Image not found or empty");

                var fileName = !string.IsNullOrWhiteSpace(meta?.Name) ? meta.Value.Name : $"{fileId}.jpg";
                var mimeType = !string.IsNullOrWhiteSpace(meta?.MimeType) ? meta.Value.MimeType : "application/octet-stream";

                return File(bytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-folders")]
        public async Task<IActionResult> GetFolders()
        {
            try
            {
                var folders = await _driveService.GetFoldersAsync();
                return Ok(folders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImgController] GetAllImg Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
    }
}
