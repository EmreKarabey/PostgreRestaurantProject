using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Email;
using Microsoft.Extensions.Configuration;
namespace Infrastructure.Email
{
    public class SmtpEmailSender:IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var host = _configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host ayarı appsettings.json'da bulunamadı");

            var portString = _configuration["Smtp:Port"];

            int port = string.IsNullOrEmpty(portString) ? 587 : int.Parse(portString);

            var Ssl = _configuration["Smtp:EnableSsl"];

            bool enableSsl = string.IsNullOrEmpty(portString) ? true : bool.Parse(Ssl);

            var User = _configuration["Smtp:User"] ?? throw new InvalidOperationException("Smtp:User ayarı appsettings.json'da bulunamadı");

            var Pass = _configuration["Smtp:Pass"] ?? throw new InvalidOperationException("Smtp:Pass ayarı appsettings.json'da bulunamadı");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(User, Pass),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage(from: User!, to: email, subject, htmlMessage);

            mail.IsBodyHtml = true;

            await client.SendMailAsync(mail);
        }
    }
}
