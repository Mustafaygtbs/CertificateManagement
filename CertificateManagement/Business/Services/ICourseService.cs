using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace CertificateManagement.Business.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(CourseDto courseDto);
        Task UpdateCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(int id);
        Task<bool> CompleteCourseAsync(int courseId);
        Task<string> UploadCertificateTemplateAsync(int courseId, IFormFile file);
    }
}
