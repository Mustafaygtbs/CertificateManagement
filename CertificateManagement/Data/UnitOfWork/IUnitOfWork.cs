using System;
using System.Threading.Tasks;
using CertificateManagement.Data.Repositories;
using CertificateManagement.Domain.Entities;

namespace CertificateManagement.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IStudentRepository Students { get; }
        IRepository<User> Users { get; }
        
        Task<int> CompleteAsync();
    }
}
