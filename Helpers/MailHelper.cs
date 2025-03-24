using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace NettruyenRemake.Helpers
{
    public class MailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Send(string from, string to, string subject, string body)
        {
            try
            {
                var host = _configuration["Gmail:Host"];
                var port = int.Parse(_configuration["Gmail:Port"]);
                var username = _configuration["Gmail:Username"];
                var enableSsl = bool.Parse(_configuration["Gmail:EnableSsl"]);

                // Lấy password từ biến môi trường thay vì appsettings
                var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

                var smtpClient = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(username, password)
                };

                var mailMessage = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body
                };

                smtpClient.Send(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
