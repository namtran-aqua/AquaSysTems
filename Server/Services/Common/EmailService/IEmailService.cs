using System.Threading.Tasks;

namespace AquaSolution.Server.Services.Common.EmailService
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetOtpAsync(string email, string workDayId, string otp);
    }
}
