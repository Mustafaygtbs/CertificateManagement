using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Domain.Entities;

namespace CertificateManagement.Data.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetCourseWithStudentsAsync(int id);
        Task<IEnumerable<Course>> GetActiveCoursesAsync();
    }
}
