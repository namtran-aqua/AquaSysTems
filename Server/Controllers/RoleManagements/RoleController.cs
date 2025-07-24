using AquaSolution.Server.Services.RoleService;
using AquaSolution.Shared.Roles;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.UserManagements
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách role", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }
        [HttpPost("{userId}/Update-user-role")]
        public async Task<IActionResult> UpdateUserRoles(Guid userId, [FromBody] List<UpdateUserRoleDto>? roles)
        {
            var success = await _roleService.UpdateUserRolesAsync(userId, roles);
            if (success)
            {
                return Ok(new { message = "Update successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update" });
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] HandleRoleDto handleRoleDto)
        {
            if (handleRoleDto == null || string.IsNullOrWhiteSpace(handleRoleDto.RoleName))
                return BadRequest("Invalid role data.");

            var result = await _roleService.CreatedRole(handleRoleDto);
            if (result)
                return Ok(new { success = true, message = "Role created successfully." });
            else
                return StatusCode(500, "Failed to create role.");
        }
        [HttpDelete("delete-role/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Role not found or already deleted." });
            }

            return Ok(new { message = "Role deleted successfully." });
        }
    }
}
