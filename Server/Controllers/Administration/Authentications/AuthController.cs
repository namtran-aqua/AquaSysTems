using AquaService.Shared.AuthModels;
using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Shared.AuthModels;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        if (result == null)
            return Unauthorized(); 

        return Ok(new Dictionary<string, string> {
        { "token", result.Token }
    });
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
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPassword request)
    {
        try
        {
            var success = await _userService.ResetPasswordAsync(request);
            if (success)
                return Ok(new { message = "Password reset successfully." });
            return BadRequest(new { message = "Password reset failed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        try
        {
            await _userService.SendOtpAsync(request);
            // Always return success to prevent user enumeration
            return Ok(new { message = "If the information is correct, an OTP has been sent to the registered email." });
        }
        catch (Exception)
        {
            return Ok(new { message = "If the information is correct, an OTP has been sent to the registered email." });
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        try
        {
            var resetToken = await _userService.VerifyOtpAsync(request);
            return Ok(new { ResetToken = resetToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password-with-token")]
    public async Task<IActionResult> ResetPasswordWithToken(ResetPasswordRequest request)
    {
        try
        {
            var success = await _userService.ResetPasswordWithTokenAsync(request);
            if (success)
                return Ok(new { message = "Password reset successfully." });
            return BadRequest(new { message = "Password reset failed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
