using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace SalesForecastingDashboard.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("brcksdagi@gmail.com", "ejmy udht suzi gbbp"),
                    EnableSsl = true
                };

                var mail = new MailMessage("brcksdagi@gmail.com", to, subject, body);
                mail.IsBodyHtml = true;

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Daily report email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email sending failed: {ex.Message}");
            }
        }
    }
}
