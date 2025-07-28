using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CertificateManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CertificateManagement.Data.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId)
        {
            return await _context.Students
                .Where(s => s.CourseId == courseId)
                .ToListAsync();
        }
        
        public async Task<Student?> GetStudentByTokenAsync(Guid token)
        {
            return await _context.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.CertificateAccessToken == token);
        }
    }
}
