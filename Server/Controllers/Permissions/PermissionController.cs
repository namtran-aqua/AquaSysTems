using AquaSolution.Server.Services.PermissionService;
using AquaSolution.Server.Services.UserService;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Permissions
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("get-all-permission")]
        public async Task<ActionResult<List<GroupedPermissionDto>>> GetPermissionTree()
        {
            var result = await _permissionService.GetGroupedPermissionsByMenuViewAsync();
            return Ok(result);
        }
        [HttpGet("get-all-permission-role/{roleId}")]
        public async Task<ActionResult<List<GroupedPermissionDto>>> GetPermissionByRole(Guid roleId)
        {
            var result = await _permissionService.GetPermissionTreeByRoleId(roleId);
            return Ok(result);
        }
        [HttpGet("get-all")]
        public async Task<ActionResult<List<MenuDto>>> GetallPermission()
        {
            var result = await _permissionService.GetPermissionAll();
            return Ok(result);
        }
        [HttpPost("create-permission")]
        public async Task<IActionResult> CreatePermission([FromBody] HandlePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _permissionService.CreatedPermission(dto);
            if (result)
                return Ok(new { Success = true, Message = "Permission created successfully" });
            else
                return StatusCode(500, new { Success = false, Message = "Failed to create permission" });
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _permissionService.DeletePermission(id);
            if (result)
                return Ok(new { success = true, message = "Xóa thành công" });

            return NotFound(new { success = false, message = "permission không tồn tại" });
        }
    }
}
