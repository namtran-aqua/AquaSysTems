using AquaSolution.Server.Services.UserService;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AquaSolution.Server.Controllers.UserManagements
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHubContext<SignalrHub> _hubContext;
        public UserController(IUserService userService, IHubContext<SignalrHub> hubContext)
        {
            _userService = userService;
            _hubContext = hubContext;
        }

        [HttpGet("get-curernUser/{userId}")]
        public async Task<UserDto?> GetCurrentUser(Guid userId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(userId);
            if (currentUser == null)
                return new UserDto();

            return currentUser;
        }
        [HttpGet("get-all")]
        public async Task<List<UserDto?>> GetAll()
        {
            var data = await _userService.GetAllUser();

            return data;
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreatedAndUpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.CreatedAsync(dto);
            if (result)
                return Ok(new { message = "User created successfully" });

            return StatusCode(500, new { message = "Failed to create user" });
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            if (result)
                return Ok(new { success = true, message = "Xóa thành công" });

            return NotFound(new { success = false, message = "User không tồn tại" });
        }
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CreatedAndUpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateAsync(dto);

            if (result)
                return Ok(new { success = true, message = "Cập nhật thành công" });

            return BadRequest(new { success = false, message = "Cập nhật thất bại" });
        }

        [HttpPut("update-avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] AvataDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ChangeAvataAsync(dto);

            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReloadMenu");
                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            return BadRequest(new { success = false, message = "Cập nhật thất bại" });
        }
        [HttpGet("get-contributer")]
        public async Task<List<UserContributerDto?>> GetContributer()
        {
            var data = await _userService.GetContributer();

            return data;
        }
    }
}
