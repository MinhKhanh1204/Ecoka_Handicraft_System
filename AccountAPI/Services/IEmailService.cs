namespace AccountAPI.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendPasswordResetConfirmationEmailAsync(string toEmail);
        /// <summary>
        /// Sent when the submitted email is not found in the database during forgot-password.
        /// Includes a link to the registration page.
        /// </summary>
        Task SendUnregisteredEmailAsync(string toEmail, string registerUrl);
    }
}
