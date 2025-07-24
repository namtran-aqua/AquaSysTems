using AquaService.Shared.AuthModels;
using AquaSolution.Server.Services.UserService;
using AquaSolution.Shared.AuthModels;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        if (result == null)
            return Unauthorized();
        return Ok(result);
    }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePassRequest request)
    {
        try
        {
            var success = await _userService.ChangePasswordAsync(request);
            if (success)
                return Ok(new { message = "Password changed successfully." });
            return BadRequest(new { message = "Password change failed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
