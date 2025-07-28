using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CertificateManagement.Business.DTOs;
using CertificateManagement.Data.UnitOfWork;
using CertificateManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CertificateManagement.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        
        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email)).FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine($"Kullanıcı bulunamadı: {loginDto.Email}");
                return null;
            }

            Console.WriteLine($"Kullanıcı bulundu: {user.Email}, PasswordHash: {user.PasswordHash.Substring(0, 20)}...");

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                Console.WriteLine($"Şifre doğrulanamadı: {loginDto.Password}");
                return null;
            }

            var token = GenerateJwtToken(user);
            Console.WriteLine($"Token oluşturuldu: {token.Substring(0, 20)}...");

            return new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }




        public async Task<bool> RegisterAsync(UserDto userDto)
        {
            // Email kontrolü
            var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == userDto.Email)).FirstOrDefault();
            if (existingUser != null)
                return false;
                
            var passwordHash = CreatePasswordHash(userDto.Password);
            
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = passwordHash,
                Role = userDto.Role
            };
            
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
        
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;
                
            if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
                return false;
                
            user.PasswordHash = CreatePasswordHash(newPassword);
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Base64 formatında güvenli bir anahtar oluşturun
            var keyBytes = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var securityKey = new SymmetricSecurityKey(keyBytes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"])),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                // HMACSHA512 generates a 128 byte key by default
                var salt = hmac.Key;
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Salt ve hash'i birleştir
                var hashBytes = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, hashBytes, 0, salt.Length);
                Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);

            // HMACSHA512 key length is 128 bytes
            var salt = new byte[128];
            Array.Copy(hashBytes, 0, salt, 0, salt.Length);

            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Hash'leri karşılaştır
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hashBytes[i + salt.Length])
                        return false;
                }
            }

            return true;
        }


    }
}
