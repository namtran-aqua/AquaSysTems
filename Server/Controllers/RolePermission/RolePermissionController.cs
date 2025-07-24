using AquaSolution.Server.Services.RolePermissionService;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Controllers.PermissionService
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public RolePermissionController(IRolePermissionService rolePermissionService, IHubContext<SignalrHub> hubContext)
        {
            _rolePermissionService = rolePermissionService;
            _hubContext = hubContext;

        }

        [HttpPost("update/{roleId}")]
        public async Task<IActionResult> UpdateRolePermission(Guid roleId,[FromBody] List<Guid> input)
        {
            if (input == null || roleId== Guid.Empty)
                return BadRequest("Invalid input");

            var result = await _rolePermissionService.UpdateRolePermission(roleId, input);
            await _hubContext.Clients.All.SendAsync("ReloadMenu");
            return Ok(new { affectedRows = result });
        }
    }
}
