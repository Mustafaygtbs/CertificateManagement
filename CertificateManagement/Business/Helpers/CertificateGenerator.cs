using System;
using System.IO;
using System.Threading.Tasks;
using CertificateManagement.Domain.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CertificateManagement.Business.Helpers
{
    public static class CertificateGenerator
    {
        public static async Task<byte[]> GenerateCertificatePdf(Student student, Course course)
        {
            // Bu örnek basit bir PDF sertifika oluşturur
            // Gerçek projede şablon üzerinden oluşturma yapılmalıdır
            
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate());
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                
                document.Open();
                
                // Yazı tipi tanımları
                var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 24, Font.BOLD);
                var headerFont = new Font(baseFont, 16, Font.BOLD);
                var normalFont = new Font(baseFont, 12);
                
                // Sertifika başlığı
                Paragraph title = new Paragraph("SERTİFİKA", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 30;
                document.Add(title);
                
                // Tarih
                Paragraph date = new Paragraph($"Tarih: {DateTime.Now.ToShortDateString()}", normalFont);
                date.Alignment = Element.ALIGN_RIGHT;
                date.SpacingAfter = 30;
                document.Add(date);
                
                // İçerik
                Paragraph content = new Paragraph();
                content.Alignment = Element.ALIGN_CENTER;
                content.Add(new Chunk($"Bu sertifika, ", normalFont));
                content.Add(new Chunk($"{student.FirstName} {student.LastName}", headerFont));
                content.Add(new Chunk(" isimli katılımcının\n\n", normalFont));
                content.Add(new Chunk($"\"{course.Name}\"", headerFont));
                content.Add(new Chunk("\n\neğitimini başarıyla tamamladığını belgelemektedir.", normalFont));
                content.SpacingAfter = 40;
                document.Add(content);
                
                // Eğitim bilgileri
                Paragraph courseInfo = new Paragraph();
                courseInfo.Alignment = Element.ALIGN_CENTER;
                courseInfo.Add(new Chunk($"Eğitim Tarihleri: {course.StartDate.ToShortDateString()} - {course.EndDate.ToShortDateString()}", normalFont));
                document.Add(courseInfo);
                
                // İmza
                Paragraph signature = new Paragraph("____________________", normalFont);
                signature.Alignment = Element.ALIGN_RIGHT;
                signature.SpacingBefore = 50;
                document.Add(signature);
                
                Paragraph signatureName = new Paragraph("Yetkili İmza", normalFont);
                signatureName.Alignment = Element.ALIGN_RIGHT;
                document.Add(signatureName);
                
                document.Close();
                
                return memoryStream.ToArray();
            }
        }
    }
}
