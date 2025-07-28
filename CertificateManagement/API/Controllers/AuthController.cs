using System;
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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                
                if (response == null)
                    return Unauthorized(new { message = "Invalid email or password" });
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error during login: {ex.Message}");
            }
        }
        
        [HttpPost("register")]
        //[Authorize(Roles = "Admin")] // Sadece admin kullanıcı ekleyebilir
        public async Task<ActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(userDto);
                
                if (!result)
                    return BadRequest(new { message = "User with this email already exists" });
                
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error during registration: {ex.Message}");
            }
        }
        
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                // User ID'sini claim'den al
                if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
                    return BadRequest(new { message = "Invalid user ID" });
                
                var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                
                if (!result)
                    return BadRequest(new { message = "Current password is incorrect" });
                
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error changing password: {ex.Message}");
            }
        }
    }
    
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
