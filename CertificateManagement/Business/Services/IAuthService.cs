using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;

namespace CertificateManagement.Business.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(UserDto userDto);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
