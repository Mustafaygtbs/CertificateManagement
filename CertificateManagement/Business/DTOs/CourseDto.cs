using System;

namespace CertificateManagement.Business.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public string CertificateTemplateUrl { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }
}
