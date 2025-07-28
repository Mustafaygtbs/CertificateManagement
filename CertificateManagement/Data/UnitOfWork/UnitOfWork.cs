using System;
using System.Threading.Tasks;
using CertificateManagement.Data.Repositories;
using CertificateManagement.Domain.Entities;

namespace CertificateManagement.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ICourseRepository Courses { get; private set; }
        public IStudentRepository Students { get; private set; }
        public IRepository<User> Users { get; private set; }
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(_context);
            Students = new StudentRepository(_context);
            Users = new Repository<User>(_context);
        }
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
