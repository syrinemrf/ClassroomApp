using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ClassroomApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass)
                || smtpUser.Contains("YOUR_EMAIL") || smtpPass.Contains("YOUR_APP_PASSWORD"))
            {
                _logger.LogWarning("Email not sent to {Email}: SMTP credentials are not configured. Set Email:SmtpUser and Email:SmtpPass in appsettings or .env file.", toEmail);
                return;
            }

            var smtpPort = int.TryParse(_config["Email:SmtpPort"], out var port) ? port : 587;
            var fromEmail = _config["Email:FromEmail"] ?? smtpUser;
            var fromName = _config["Email:FromName"] ?? "ClassroomApp";

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            }
        }
    }
}
