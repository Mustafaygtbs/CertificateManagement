using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CertificateManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        
        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data from the database: {ex.Message}");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                
                if (course == null)
                    return NotFound($"Course with ID {id} not found");
                
                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data from the database: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> CreateCourse([FromBody] CourseDto courseDto)
        {
            try
            {
                if (courseDto == null)
                    return BadRequest("Course data is null");
                
                var createdCourse = await _courseService.CreateCourseAsync(courseDto);
                
                return CreatedAtAction(nameof(GetCourse), 
                    new { id = createdCourse.Id }, 
                    createdCourse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error creating course: {ex.Message}");
            }
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCourse(int id, [FromBody] CourseDto courseDto)
        {
            try
            {
                if (courseDto == null)
                    return BadRequest("Course data is null");
                
                if (id != courseDto.Id)
                    return BadRequest("Course ID mismatch");
                
                await _courseService.UpdateCourseAsync(courseDto);
                
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error updating course: {ex.Message}");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            try
            {
                await _courseService.DeleteCourseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error deleting course: {ex.Message}");
            }
        }
        
        [HttpPost("{id}/template")]
        public async Task<ActionResult> UploadCertificateTemplate(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");
                
                var templateUrl = await _courseService.UploadCertificateTemplateAsync(id, file);
                
                return Ok(new { templateUrl });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error uploading template: {ex.Message}");
            }
        }
        
        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteCourse(int id)
        {
            try
            {
                var result = await _courseService.CompleteCourseAsync(id);
                
                if (!result)
                    return NotFound($"Course with ID {id} not found");
                
                return Ok(new { message = "Course completed and certificates sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error completing course: {ex.Message}");
            }
        }
    }
}
