using AquaSolution.Server.Services.ImgsService;
using AquaSolution.Shared.Imgs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaSolution.Server.Controllers.Imgs
{
    [ApiController]
    [Route("api/[controller]")]
    public class FolderPermissionController : ControllerBase
    {
        private readonly IFolderPermissionService _permissionService;

        public FolderPermissionController(IFolderPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("get-by-workday/{workDayId}")]
        public async Task<IActionResult> GetPermissions(string workDayId)
        {
            var result = await _permissionService.GetPermissionsByWorkDayIdAsync(workDayId);
            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var result = await _permissionService.GetAllPermissionsAsync();
            return Ok(result);
        }

        [HttpPost("update/{workDayId}")]
        public async Task<IActionResult> UpdatePermissions(string workDayId, [FromBody] List<FolderPermissionDto> permissions)
        {
            var result = await _permissionService.UpdatePermissionsAsync(workDayId, permissions);
            if (result)
                return Ok("Updated successfully");
            
            return BadRequest("Update failed");
        }
        [HttpGet("get-by-folder/{folderId}")]
        public async Task<IActionResult> GetPermissionsByFolder(string folderId)
        {
            var result = await _permissionService.GetPermissionsByFolderIdAsync(folderId);
            return Ok(result);
        }

        [HttpPost("update-by-folder/{folderId}")]
        public async Task<IActionResult> UpdatePermissionsByFolder(string folderId, [FromQuery] string folderName, [FromBody] List<string> workDayIds)
        {
            var result = await _permissionService.UpdatePermissionsByFolderAsync(folderId, folderName, workDayIds);
            if (result)
                return Ok("Updated successfully");
            
            return BadRequest("Update failed");
        }
    }
}
