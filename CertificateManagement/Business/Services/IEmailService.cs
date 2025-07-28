using System.Threading.Tasks;

namespace CertificateManagement.Business.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendCertificateEmailAsync(string to, string studentName, string certificateLink);
    }
}
