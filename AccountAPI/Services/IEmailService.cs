namespace AccountAPI.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendPasswordResetConfirmationEmailAsync(string toEmail);
    }
}
