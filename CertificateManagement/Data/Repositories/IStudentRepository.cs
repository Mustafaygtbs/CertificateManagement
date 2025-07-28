using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CertificateManagement.Domain.Entities;

namespace CertificateManagement.Data.Repositories
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId);
        Task<Student?> GetStudentByTokenAsync(Guid token);
    }
}
