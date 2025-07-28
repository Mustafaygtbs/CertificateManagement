using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Business.Helpers;
using CertificateManagement.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CertificateManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        
        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByCourse(int courseId)
        {
            try
            {
                var students = await _studentService.GetStudentsByCourseAsync(courseId);
                return Ok(students);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data: {ex.Message}");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                
                if (student == null)
                    return NotFound($"Student with ID {id} not found");
                
                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> CreateStudent([FromBody] StudentDto studentDto)
        {
            try
            {
                if (studentDto == null)
                    return BadRequest("Student data is null");
                
                await _studentService.CreateStudentAsync(studentDto);
                
                return CreatedAtAction(nameof(GetStudent), 
                    new { id = studentDto.Id }, 
                    studentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error creating student: {ex.Message}");
            }
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStudent(int id, [FromBody] StudentDto studentDto)
        {
            try
            {
                if (studentDto == null)
                    return BadRequest("Student data is null");
                
                if (id != studentDto.Id)
                    return BadRequest("Student ID mismatch");
                
                await _studentService.UpdateStudentAsync(studentDto);
                
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error updating student: {ex.Message}");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            try
            {
                await _studentService.DeleteStudentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error deleting student: {ex.Message}");
            }
        }
        
        [HttpPost("course/{courseId}/import")]
        public async Task<ActionResult> ImportStudentsFromExcel(int courseId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");
                
                var result = await _studentService.ImportStudentsFromExcelAsync(courseId, file);
                
                if (!result)
                    return BadRequest("Failed to import students");
                
                return Ok(new { message = "Students imported successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error importing students: {ex.Message}");
            }
        }
        
        [HttpGet("course/{courseId}/export")]
        public async Task<ActionResult> ExportStudentsToExcel(int courseId)
        {
            try
            {
                var students = await _studentService.GetStudentsByCourseAsync(courseId);
                var fileBytes = ExcelHelper.ExportStudentsToExcel(students);
                
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Students_{courseId}_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error exporting students: {ex.Message}");
            }
        }
        
        [HttpPost("{id}/complete")]
        public async Task<ActionResult> MarkStudentAsCompleted(int id)
        {
            try
            {
                var result = await _studentService.MarkStudentAsCompletedAsync(id);
                
                if (!result)
                    return NotFound($"Student with ID {id} not found");
                
                return Ok(new { message = "Student marked as completed" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error marking student as completed: {ex.Message}");
            }
        }
        
        [HttpPost("{id}/send-certificate")]
        public async Task<ActionResult> SendCertificateEmail(int id)
        {
            try
            {
                var result = await _studentService.SendCertificateEmailAsync(id);
                
                if (!result)
                    return NotFound($"Student with ID {id} not found or not completed the course");
                
                return Ok(new { message = "Certificate email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error sending certificate email: {ex.Message}");
            }
        }
    }
}
