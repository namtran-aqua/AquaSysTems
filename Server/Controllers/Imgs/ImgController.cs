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
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("thumbnail/{fileId}")]
        public async Task<IActionResult> GetThumbnail(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return BadRequest("Missing fileId");

            try
            {
                var bytes = await _driveService.GetFileBytesAsync(fileId);
                return File(bytes, "image/jpeg");
            }
            catch (Exception ex)
            {
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
                return BadRequest(ex.Message);
            }
        }
    }
}
