using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartParcel.API.Services.Interfaces;

namespace SmartParcel.API.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Gmail SMTP settings
            _smtpHost = "smtp.gmail.com";
            _smtpPort = 587;
            _smtpUsername = configuration["Email:Username"]
                ?? throw new ArgumentNullException("Email username not configured");
            _smtpPassword = configuration["Email:Password"]
                ?? throw new ArgumentNullException("Email password not configured");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, "SmartParcel Delivery"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {email} with subject '{subject}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email} with subject '{subject}'");
                throw;
            }
        }
    }
}