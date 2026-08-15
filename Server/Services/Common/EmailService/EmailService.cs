using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.Common.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        // Reusing the logic app url from SendEmailRequestClinic or similar
        private readonly string _flowUrl = "https://prod-15.southeastasia.logic.azure.com:443/workflows/2dcf2a5ca6a74e7ca0565bc92a03c5b1/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wVvZUqoCZW7gvxjgfiLGLJHdZDndFg9oeOKjGxquIKM";

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetOtpAsync(string email, string workDayId, string otp)
        {
            try
            {
                var subject = "ITSM - Password Reset Verification Code";
                var bodyEmail = new StringBuilder();
                bodyEmail.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>");
                bodyEmail.AppendLine("<h2 style='color: #1677ff; margin-bottom: 20px;'>ITSM Account Verification</h2>");
                bodyEmail.AppendLine("<p>We received a request to reset the password for your ITSM account.</p>");
                bodyEmail.AppendLine("<p>Please use the following 6-digit verification code (OTP) to complete your request:</p>");
                bodyEmail.AppendLine($"<div style='background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1677ff; border-radius: 4px; margin: 20px 0;'>{otp}</div>");
                bodyEmail.AppendLine("<p style='color: #ff4d4f; font-weight: bold;'>Note: This verification code is only valid for 5 minutes and can be used only once.</p>");
                bodyEmail.AppendLine("<p>If you did not request this, please ignore this email or contact support if you have security concerns.</p>");
                bodyEmail.AppendLine("<hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;' />");
                bodyEmail.AppendLine("<p style='font-size: 12px; color: #8c8c8c;'>This is an automated message from the ITSM System. Please do not reply directly to this email.</p>");
                bodyEmail.AppendLine("</div>");

                var payload = new
                {
                    RequestId = 0,
                    Status = "No Status",
                    To = email,
                    Subject = subject,
                    Body = bodyEmail.ToString()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

                using var client = new HttpClient();
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_flowUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"OTP Email sent successfully to {email}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send email. Status: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending OTP email.");
                return false;
            }
        }
    }
}
