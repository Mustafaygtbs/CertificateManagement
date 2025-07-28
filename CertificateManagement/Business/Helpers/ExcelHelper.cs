using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace CertificateManagement.Business.Helpers
{
    public static class ExcelHelper
    {
        public static async Task<List<Student>> ImportStudentsFromExcel(IFormFile file, int courseId)
        {
            var students = new List<Student>();
            
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return students;
                    
                    var rowCount = worksheet.Dimension.Rows;
                    
                    // İlk satır başlık olduğu için 2. satırdan başla
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var firstName = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
                        var lastName = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
                        var email = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
                        var phoneNumber = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty;
                        var hasCompletedStr = worksheet.Cells[row, 5].Value?.ToString() ?? "false";
                        
                        bool.TryParse(hasCompletedStr, out bool hasCompleted);
                        
                        // Yeni öğrenci ekle
                        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(email))
                        {
                            var student = new Student
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Email = email,
                                PhoneNumber = phoneNumber,
                                HasCompletedCourse = hasCompleted,
                                CourseId = courseId,
                                CertificateAccessToken = Guid.NewGuid()
                            };
                            
                            students.Add(student);
                        }
                    }
                    
                    return students;
                }
            }
        }
        
        public static byte[] ExportStudentsToExcel(IEnumerable<StudentDto> students)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Öğrenciler");
                
                // Başlık satırı
                worksheet.Cells[1, 1].Value = "Ad";
                worksheet.Cells[1, 2].Value = "Soyad";
                worksheet.Cells[1, 3].Value = "E-posta";
                worksheet.Cells[1, 4].Value = "Telefon";
                worksheet.Cells[1, 5].Value = "Eğitimi Tamamladı";
                worksheet.Cells[1, 6].Value = "Eğitim Adı";
                
                // Başlık stilini ayarla
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                }
                
                // Veri satırları
                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = student.FirstName;
                    worksheet.Cells[row, 2].Value = student.LastName;
                    worksheet.Cells[row, 3].Value = student.Email;
                    worksheet.Cells[row, 4].Value = student.PhoneNumber;
                    worksheet.Cells[row, 5].Value = student.HasCompletedCourse;
                    worksheet.Cells[row, 6].Value = student.CourseName;
                    
                    row++;
                }
                
                // Sütun genişliklerini ayarla
                worksheet.Cells.AutoFitColumns();
                
                return package.GetAsByteArray();
            }
        }
    }
}
