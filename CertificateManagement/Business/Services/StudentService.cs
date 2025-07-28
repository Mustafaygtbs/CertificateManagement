using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Data.UnitOfWork;
using CertificateManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

namespace CertificateManagement.Business.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly string _baseUrl;
        
        public StudentService(
            IUnitOfWork unitOfWork, 
            IEmailService emailService, 
            IFileService fileService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _fileService = fileService;
            _baseUrl = configuration["AppSettings:BaseUrl"];
        }
        
        public async Task<IEnumerable<StudentDto>> GetStudentsByCourseAsync(int courseId)
        {
            var students = await _unitOfWork.Students.GetStudentsByCourseAsync(courseId);
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
                
            return students.Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                HasCompletedCourse = s.HasCompletedCourse,
                CertificateUrl = s.CertificateUrl,
                CourseId = s.CourseId,
                CourseName = course.Name
            });
        }
        
        public async Task<StudentDto?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                return null;
                
            var course = await _unitOfWork.Courses.GetByIdAsync(student.CourseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {student.CourseId} not found");
                
            return new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                HasCompletedCourse = student.HasCompletedCourse,
                CertificateUrl = student.CertificateUrl,
                CourseId = student.CourseId,
                CourseName = course.Name
            };
        }
        
        public async Task<StudentDto?> GetStudentByTokenAsync(Guid token)
        {
            var student = await _unitOfWork.Students.GetStudentByTokenAsync(token);
            if (student == null)
                return null;
                
            var course = await _unitOfWork.Courses.GetByIdAsync(student.CourseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {student.CourseId} not found");
                
            return new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                HasCompletedCourse = student.HasCompletedCourse,
                CertificateUrl = student.CertificateUrl,
                CourseId = student.CourseId,
                CourseName = course.Name
            };
        }
        
        public async Task CreateStudentAsync(StudentDto studentDto)
        {
            var student = new Student
            {
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                Email = studentDto.Email,
                PhoneNumber = studentDto.PhoneNumber,
                HasCompletedCourse = studentDto.HasCompletedCourse,
                CertificateUrl = studentDto.CertificateUrl,
                CourseId = studentDto.CourseId,
                CertificateAccessToken = Guid.NewGuid()
            };
            
            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task UpdateStudentAsync(StudentDto studentDto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {studentDto.Id} not found");
                
            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.Email = studentDto.Email;
            student.PhoneNumber = studentDto.PhoneNumber;
            student.HasCompletedCourse = studentDto.HasCompletedCourse;
            
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {id} not found");
                
            await _unitOfWork.Students.DeleteAsync(student);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task<bool> ImportStudentsFromExcelAsync(int courseId, IFormFile file)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
            
            if (file == null || file.Length <= 0)
                throw new ArgumentException("Excel file is required");
                
            // Excel'i oku
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return false;
                    
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
                            
                            await _unitOfWork.Students.AddAsync(student);
                        }
                    }
                    
                    await _unitOfWork.CompleteAsync();
                    return true;
                }
            }
        }
        
        public async Task<bool> MarkStudentAsCompletedAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
                return false;
                
            student.HasCompletedCourse = true;
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
        
        public async Task<bool> SendCertificateEmailAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null || !student.HasCompletedCourse)
                return false;
                
            var course = await _unitOfWork.Courses.GetByIdAsync(student.CourseId);
            if (course == null)
                return false;
                
            // Sertifika yoksa oluştur
            if (string.IsNullOrEmpty(student.CertificateUrl))
            {
                var replacements = new Dictionary<string, string>
                {
                    { "{{StudentName}}", $"{student.FirstName} {student.LastName}" },
                    { "{{CourseName}}", course.Name },
                    { "{{CompletionDate}}", DateTime.Now.ToShortDateString() }
                };
                
                student.CertificateUrl = await _fileService.GenerateCertificateAsync(
                    course.CertificateTemplateUrl, 
                    replacements);
                    
                await _unitOfWork.Students.UpdateAsync(student);
                await _unitOfWork.CompleteAsync();
            }
            
            // E-posta gönder
            var certificateLink = $"{_baseUrl}/certificate/{student.CertificateAccessToken}";
            await _emailService.SendCertificateEmailAsync(
                student.Email,
                $"{student.FirstName} {student.LastName}",
                certificateLink);
                
            return true;
        }
    }
}
