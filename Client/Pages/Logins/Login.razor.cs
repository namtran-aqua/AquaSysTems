using AquaService.Shared.AuthModels;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Logins;

public enum LoginScreenState
{
    Login,
    ForgotPassword,
    VerifyOtp,
    ResetPassword
}

public partial class Login
{
    [Inject] private HttpClient Http { get; set; }
    [Inject] private CustomAuthenticationStateProvider AuthProvider { get; set; }
    [Inject] private NavigationManager Nav { get; set; }
    [Inject] private ISessionStorageService _sessionStorage { get; set; }
    
    private string username { get; set; }
    private string password { get; set; }
    private bool showPassword = false;
    private bool showNewPassword = false;
    private bool showConfirmPassword = false;

    private LoginScreenState currentScreenState = LoginScreenState.Login;
    private string resetEmail { get; set; }
    private string otpCode { get; set; }
    private string resetToken { get; set; }
    private string newPassword { get; set; }
    private string confirmPassword { get; set; }

    private async Task DoLogin()
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(username))
        {
            await Message.Error("username cannot be blank");
            return;
        }
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(password))
        {
            await Message.Error("password cannot be blank");
            return;
        }

        var response = await Http.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            UserName = username,
            Password = password
        });
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await Message.Error("Incorrect Account or Password, Please Check Again !");
            return;
        }
        if (!response.IsSuccessStatusCode)
        {
            await Message.Error($"Server error: {(int)response.StatusCode}");
            return;
        }
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        if (content == null || !content.ContainsKey("token"))
        {
            await Message.Error("Invalid response from server.");
            return;
        }
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(content["token"]);
        var claims = token.Claims.ToList();

        await _sessionStorage.SetItemAsync("authToken", content["token"]);
        AuthProvider.MarkUserAsAuthenticated(username, claims);

        var baseUri = Nav.BaseUri.TrimEnd('/');
        Nav.NavigateTo($"{baseUri}/");
    }

    private void TogglePassword()
    {
        showPassword = !showPassword;
    }

    private void ToggleNewPassword()
    {
        showNewPassword = !showNewPassword;
    }

    private void ToggleConfirmPassword()
    {
        showConfirmPassword = !showConfirmPassword;
    }

    private async Task HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (currentScreenState == LoginScreenState.Login)
                await DoLogin();
            else if (currentScreenState == LoginScreenState.ForgotPassword)
                await SendOtp();
            else if (currentScreenState == LoginScreenState.VerifyOtp)
                await VerifyOtp();
            else if (currentScreenState == LoginScreenState.ResetPassword)
                await ResetPassword();
        }
    }

    private void GoToForgotPassword()
    {
        currentScreenState = LoginScreenState.ForgotPassword;
        resetEmail = string.Empty;
        otpCode = string.Empty;
        newPassword = string.Empty;
        confirmPassword = string.Empty;
    }

    private void GoToLogin()
    {
        currentScreenState = LoginScreenState.Login;
        password = string.Empty;
    }

    private async Task SendOtp()
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(resetEmail))
        {
            await Message.Error("WorkDay ID and Email cannot be blank");
            return;
        }

        var response = await Http.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            WorkDayId = username,
            Email = resetEmail
        });

        if (response.IsSuccessStatusCode)
        {
            await Message.Success("If the information is correct, an OTP has been sent to the registered email.");
            currentScreenState = LoginScreenState.VerifyOtp;
        }
        else
        {
            await Message.Success("If the information is correct, an OTP has been sent to the registered email.");
            currentScreenState = LoginScreenState.VerifyOtp;
        }
    }

    private async Task VerifyOtp()
    {
        if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != 6)
        {
            await Message.Error("Please enter a valid 6-digit OTP code");
            return;
        }

        var response = await Http.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpRequest
        {
            WorkDayId = username,
            Otp = otpCode
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (content != null && content.ContainsKey("resetToken"))
            {
                resetToken = content["resetToken"];
                currentScreenState = LoginScreenState.ResetPassword;
                await Message.Success("OTP verified successfully. Please set your new password.");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (errorContent != null && errorContent.ContainsKey("message"))
            {
                await Message.Error(errorContent["message"]);
            }
            else
            {
                await Message.Error("Failed to verify OTP.");
            }
        }
    }

    private async Task ResetPassword()
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await Message.Error("Passwords cannot be blank");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await Message.Error("Confirm password does not match the new password.");
            return;
        }

        var response = await Http.PostAsJsonAsync("/api/auth/reset-password-with-token", new ResetPasswordRequest
        {
            ResetToken = resetToken,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        });

        if (response.IsSuccessStatusCode)
        {
            await Message.Success("Password reset successfully. Please login with your new password.");
            currentScreenState = LoginScreenState.Login;
            password = string.Empty;
        }
        else
        {
            var errorContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (errorContent != null && errorContent.ContainsKey("message"))
            {
                await Message.Error(errorContent["message"]);
            }
            else
            {
                await Message.Error("Failed to reset password.");
            }
        }
    }
}
