using System.Net;
using System.Net.Mail;
using AccountAPI.Helpers;
using Microsoft.Extensions.Options;

namespace AccountAPI.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly PasswordResetSettings _resetSettings;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IOptions<PasswordResetSettings> resetSettings)
        {
            _emailSettings = emailSettings.Value;
            _resetSettings = resetSettings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var resetUrl = $"{_resetSettings.ClientBaseUrl.TrimEnd('/')}/Account/ResetPassword?token={Uri.EscapeDataString(resetToken)}";

            var subject = "Reset Your Password - Ecoka Handicraft";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Password Reset Request</h2>
    <p>You have requested to reset your password. Click the link below to set a new password:</p>
    <p><a href='{resetUrl}' style='color: #007bff;'>{resetUrl}</a></p>
    <p>This link will expire in {_resetSettings.TokenExpiryMinutes} minutes.</p>
    <p>If you did not request this, please ignore this email.</p>
    <br/>
    <p>Ecoka Handicraft System</p>
</body>
</html>";

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        public async Task SendPasswordResetConfirmationEmailAsync(string toEmail)
        {
            var subject = "Your Password Has Been Reset - Ecoka Handicraft";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Password Reset Confirmation</h2>
    <p>Your password has been successfully reset.</p>
    <p>If you did not reset your password, please contact us immediately.</p>
    <br/>
    <p>Ecoka Handicraft System</p>
</body>
</html>";

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        /// <summary>
        /// Sent when the forgot-password email was not found in the database.
        /// Invites the user to register with a direct link to the registration page.
        /// </summary>
        public async Task SendUnregisteredEmailAsync(string toEmail, string registerUrl)
        {
            var subject = "Email Chua Dang Ki - Ecoka Handicraft";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Xin chao,</h2>
    <p>Email nay chua duoc dang ki tai he thong <strong>Ecoka Handicraft</strong>.</p>
    <p>Neu ban muon tao tai khoan de mua sam san pham thu cong, vui long dang ki qua lien ket ben duoi:</p>
    <p><a href='{registerUrl}' style='display:inline-block;background:#667eea;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:600;'>Dang Ki Tai Khoan</a></p>
    <p>Neu ban khong yeu cau dang ki, vui long bo qua email nay.</p>
    <br/>
    <p>Ecoka Handicraft System</p>
</body>
</html>";

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
