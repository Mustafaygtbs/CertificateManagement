using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CertificateManagement.Business.Services
{
    public class FileService : IFileService
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        
        public FileService(IConfiguration configuration)
        {
            _storageConnectionString = configuration["Azure:StorageConnectionString"];
            _storageContainerName = configuration["Azure:BlobContainerName"];
        }
        
        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            // Dosyayı Azure Blob Storage'a yükleme
            var containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            await containerClient.CreateIfNotExistsAsync();
            
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobPath = $"{folderName}/{fileName}";
            
            var blobClient = containerClient.GetBlobClient(blobPath);
            
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            
            return blobPath;
        }
        
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File {filePath} not found in the blob storage.");
            }
            
            using (var memoryStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        
        public async Task DeleteFileAsync(string filePath)
        {
            var containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            
            await blobClient.DeleteIfExistsAsync();
        }
        
        public async Task<string> GenerateCertificateAsync(string templatePath, Dictionary<string, string> replacements)
        {
            try
            {
                // Şablon dosyasını indir
                var templateBytes = await GetFileAsync(templatePath);
                
                // PDF, Word veya HTML şablonu işleme (burada basit bir PDF işleme örneği gösterilmiştir)
                // Gerçek uygulamada, uygun bir PDF kütüphanesi (örn. iTextSharp, PDFsharp) kullanılmalıdır.
                
                // Şablon üzerinde değişiklikler yap ve yeni dosya oluştur
                // Bu kısım kullanılacak şablon formatına göre değişecektir
                
                // Örnek amacıyla, şablonu değiştirmeden aynı formatta kaydediyoruz
                var certificateName = $"{Guid.NewGuid()}.pdf";
                var certificatePath = $"certificates/{certificateName}";
                
                // Değiştirilen içeriği Azure Blob Storage'a yükle
                var containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
                var blobClient = containerClient.GetBlobClient(certificatePath);
                
                using (var memoryStream = new MemoryStream(templateBytes))
                {
                    await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = "application/pdf" });
                }
                
                return certificatePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Certificate generation failed: {ex.Message}", ex);
            }
        }
    }
}
