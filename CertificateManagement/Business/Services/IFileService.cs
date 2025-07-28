using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CertificateManagement.Business.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task<byte[]> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task<string> GenerateCertificateAsync(string templatePath, Dictionary<string, string> replacements);
    }
}
