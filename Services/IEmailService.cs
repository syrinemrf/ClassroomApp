namespace ClassroomApp.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an HTML email. Silently logs and returns if SMTP is not configured.
        /// </summary>
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
