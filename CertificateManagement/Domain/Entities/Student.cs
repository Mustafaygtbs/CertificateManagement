using System;

namespace CertificateManagement.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool HasCompletedCourse { get; set; }
        public string CertificateUrl { get; set; } = string.Empty;
        public Guid CertificateAccessToken { get; set; } = Guid.NewGuid();
        
        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
