using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Data.UnitOfWork;
using CertificateManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CertificateManagement.Business.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;
        private readonly string _baseUrl;
        
        public CourseService(
            IUnitOfWork unitOfWork, 
            IFileService fileService, 
            IEmailService emailService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _emailService = emailService;
            _baseUrl = configuration["AppSettings:BaseUrl"];
        }
        
        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.Courses.GetAllAsync();
            var courseDtos = new List<CourseDto>();
            
            foreach (var course in courses)
            {
                var students = await _unitOfWork.Students.GetStudentsByCourseAsync(course.Id);
                var courseDto = new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Description = course.Description,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    IsCompleted = course.IsCompleted,
                    CertificateTemplateUrl = course.CertificateTemplateUrl,
                    StudentCount = students.Count()
                };
                
                courseDtos.Add(courseDto);
            }
            
            return courseDtos;
        }
        
        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetCourseWithStudentsAsync(id);
            if (course == null)
                return null;
                
            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                IsCompleted = course.IsCompleted,
                CertificateTemplateUrl = course.CertificateTemplateUrl,
                StudentCount = course.Students.Count
            };
        }
        
        public async Task<Course> CreateCourseAsync(CourseDto courseDto)
        {
            var course = new Course
            {
                Name = courseDto.Name,
                Description = courseDto.Description,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                IsCompleted = courseDto.IsCompleted,
                CertificateTemplateUrl = courseDto.CertificateTemplateUrl
            };
            
            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.CompleteAsync();
            
            return course;
        }
        
        public async Task UpdateCourseAsync(CourseDto courseDto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseDto.Id);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseDto.Id} not found");
                
            course.Name = courseDto.Name;
            course.Description = courseDto.Description;
            course.StartDate = courseDto.StartDate;
            course.EndDate = courseDto.EndDate;
            course.IsCompleted = courseDto.IsCompleted;
            
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {id} not found");
                
            await _unitOfWork.Courses.DeleteAsync(course);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task<bool> CompleteCourseAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetCourseWithStudentsAsync(courseId);
            if (course == null)
                return false;
                
            course.IsCompleted = true;
            await _unitOfWork.Courses.UpdateAsync(course);
            
            // Eğitimi tamamlamış öğrencilere sertifika gönder
            foreach (var student in course.Students.Where(s => s.HasCompletedCourse))
            {
                // Öğrenci için sertifika oluştur
                var replacements = new Dictionary<string, string>
                {
                    { "{{StudentName}}", $"{student.FirstName} {student.LastName}" },
                    { "{{CourseName}}", course.Name },
                    { "{{CompletionDate}}", DateTime.Now.ToShortDateString() }
                };
                
                var certificatePath = await _fileService.GenerateCertificateAsync(
                    course.CertificateTemplateUrl, 
                    replacements);
                    
                student.CertificateUrl = certificatePath;
                await _unitOfWork.Students.UpdateAsync(student);
                
                // E-posta gönder
                var certificateLink = $"{_baseUrl}/certificate/{student.CertificateAccessToken}";
                await _emailService.SendCertificateEmailAsync(
                    student.Email,
                    $"{student.FirstName} {student.LastName}",
                    certificateLink);
            }
            
            await _unitOfWork.CompleteAsync();
            return true;
        }
        
        public async Task<string> UploadCertificateTemplateAsync(int courseId, IFormFile file)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
                
            var templateUrl = await _fileService.UploadFileAsync(file, "certificate-templates");
            course.CertificateTemplateUrl = templateUrl;
            
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.CompleteAsync();
            
            return templateUrl;
        }
    }
}
