using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CertificateManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CertificateManagement.Data.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Course?> GetCourseWithStudentsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        
        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.EndDate >= DateTime.Today)
                .ToListAsync();
        }
    }
}
