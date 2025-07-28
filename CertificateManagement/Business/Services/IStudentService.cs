using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using Microsoft.AspNetCore.Http;

namespace CertificateManagement.Business.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetStudentsByCourseAsync(int courseId);
        Task<StudentDto?> GetStudentByIdAsync(int id);
        Task<StudentDto?> GetStudentByTokenAsync(Guid token);
        Task CreateStudentAsync(StudentDto studentDto);
        Task UpdateStudentAsync(StudentDto studentDto);
        Task DeleteStudentAsync(int id);
        Task<bool> ImportStudentsFromExcelAsync(int courseId, IFormFile file);
        Task<bool> MarkStudentAsCompletedAsync(int studentId);
        Task<bool> SendCertificateEmailAsync(int studentId);
    }
}
