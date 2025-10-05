using System.Net.Mail;
using System.Net;
using CreditCalculatorApi.Services.Interfaces;
using System.Net.Mime;

namespace CreditCalculatorApi.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient(_config["Email:SmtpServer"])
                {
                    Port = int.Parse(_config["Email:Port"]),
                    Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                    EnableSsl = true,
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_config["Email:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                message.To.Add(toEmail);

                await smtpClient.SendMailAsync(message);

               
                _logger.LogInformation("E-posta gönderildi: Alıcı={To}, Konu={Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "E-posta gönderilemedi: Alıcı={To}, Konu={Subject}", toEmail, subject);
                throw;
            }
        }
        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] pdfBytes, string fileName)
        {
            try
            {
                var smtpClient = new SmtpClient(_config["Email:SmtpServer"])
                {
                    Port = int.Parse(_config["Email:Port"]),
                    Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                    EnableSsl = true,
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_config["Email:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                message.To.Add(toEmail);

                //  PDF ek dosya olarak ekleniyor
                using (var stream = new MemoryStream(pdfBytes))
                {
                    var attachment = new Attachment(stream, fileName, MediaTypeNames.Application.Pdf);
                    message.Attachments.Add(attachment);

                    await smtpClient.SendMailAsync(message);
                }

                _logger.LogInformation("PDF ekli e-posta gönderildi: Alıcı={To}, Konu={Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF ekli e-posta gönderilemedi: Alıcı={To}, Konu={Subject}", toEmail, subject);
                throw;
            }
        }
    }
}
