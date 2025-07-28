using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace CertificateManagement.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        
        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(configuration["Email:SmtpPort"]);
            _smtpUsername = configuration["Email:SmtpUsername"];
            _smtpPassword = configuration["Email:SmtpPassword"];
            _senderEmail = configuration["Email:SenderEmail"];
            _senderName = configuration["Email:SenderName"];
        }
        
        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            
            var bodyBuilder = new BodyBuilder();
            if (isHtml)
                bodyBuilder.HtmlBody = body;
            else
                bodyBuilder.TextBody = body;
            
            message.Body = bodyBuilder.ToMessageBody();
            
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        
        public async Task SendCertificateEmailAsync(string to, string studentName, string certificateLink)
        {
            var subject = "Eğitim Sertifikanız Hazır!";
            var body = $@"
                <html>
                <body>
                    <h2>Tebrikler, {studentName}!</h2>
                    <p>Eğitimi başarıyla tamamladınız ve sertifikanız hazır.</p>
                    <p>Sertifikanızı görüntülemek ve indirmek için aşağıdaki bağlantıya tıklayın:</p>
                    <p><a href='{certificateLink}'>Sertifikanızı Görüntüleyin</a></p>
                    <p>Teşekkür ederiz.</p>
                </body>
                </html>
            ";
            
            await SendEmailAsync(to, subject, body);
        }
    }
}
