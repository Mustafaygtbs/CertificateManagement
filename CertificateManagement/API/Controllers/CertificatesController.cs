using System;
using System.Threading.Tasks;
using CertificateManagement.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CertificateManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IFileService _fileService;
        
        public CertificatesController(IStudentService studentService, IFileService fileService)
        {
            _studentService = studentService;
            _fileService = fileService;
        }
        
        [HttpGet("{token}")]
        public async Task<ActionResult> GetCertificate(Guid token)
        {
            try
            {
                var student = await _studentService.GetStudentByTokenAsync(token);
                
                if (student == null || !student.HasCompletedCourse || string.IsNullOrEmpty(student.CertificateUrl))
                    return NotFound("Certificate not found or not available");
                
                var certificateData = await _fileService.GetFileAsync(student.CertificateUrl);
                
                return File(certificateData, "application/pdf", $"Certificate_{student.FirstName}_{student.LastName}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving certificate: {ex.Message}");
            }
        }
        
        [HttpGet("verify/{token}")]
        public async Task<ActionResult> VerifyCertificate(Guid token)
        {
            try
            {
                var student = await _studentService.GetStudentByTokenAsync(token);
                
                if (student == null || !student.HasCompletedCourse)
                    return NotFound(new { valid = false, message = "Certificate not found or not valid" });
                
                return Ok(new
                {
                    valid = true,
                    studentName = $"{student.FirstName} {student.LastName}",
                    courseName = student.CourseName,
                    issueDate = DateTime.Now.ToShortDateString() // Gerçek uygulamada sertifikanın oluşturulma tarihi kullanılmalı
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error verifying certificate: {ex.Message}");
            }
        }
    }
}
