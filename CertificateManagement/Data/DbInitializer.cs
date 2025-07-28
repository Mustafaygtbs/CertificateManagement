using CertificateManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CertificateManagement.Data
{

    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Veritabanını oluştur
            context.Database.EnsureCreated();

            // Kullanıcı yoksa admin kullanıcı oluştur
            if (!context.Users.Any())
            {
                // Şifre oluşturma
                string password = "Admin123!";
                string hash = CreatePasswordHash(password);
                Console.WriteLine($"Admin kullanıcısı oluşturuluyor. Şifre: {password}, Hash: {hash.Substring(0, 20)}...");

                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = hash,
                    Role = "Admin"
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Örnek eğitim ve öğrenci verisi ekle
            if (!context.Courses.Any())
            {
                var course = new Course
                {
                    Name = "ASP.NET Core Eğitimi",
                    Description = "ASP.NET Core ile web uygulamaları geliştirme",
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    IsCompleted = false,
                    CertificateTemplateUrl = ""
                };

                context.Courses.Add(course);
                context.SaveChanges();

                // Örnek öğrenciler
                var students = new[]
                {
               new Student
               {
                   FirstName = "Ali",
                   LastName = "Yılmaz",
                   Email = "mustafaekstra06@gmail.com",
                   PhoneNumber = "5551112233",
                   HasCompletedCourse = false,
                   CourseId = course.Id,
                   CertificateAccessToken = Guid.NewGuid()
               },
               new Student
               {
                   FirstName = "Ayşe",
                   LastName = "Demir",
                   Email = "mustafaekstra06@gmail.com",
                   PhoneNumber = "5552223344",
                   HasCompletedCourse = true,
                   CourseId = course.Id,
                   CertificateAccessToken = Guid.NewGuid()
               },
               new Student
               {
                   FirstName = "Mehmet",
                   LastName = "Kaya",
                   Email = "mustafaekstra06@gmail.com",
                   PhoneNumber = "5553334455",
                   HasCompletedCourse = false,
                   CourseId = course.Id,
                   CertificateAccessToken = Guid.NewGuid()
               }
           };

                context.Students.AddRange(students);
                context.SaveChanges();
            }
        }

        private static string CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = hmac.Key; // 64 byte
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                var hashBytes = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, hashBytes, 0, salt.Length);
                Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

                return Convert.ToBase64String(hashBytes);
            }
        }


    }


}
