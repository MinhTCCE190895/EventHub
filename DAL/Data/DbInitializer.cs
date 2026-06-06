using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Thực hiện apply các migration còn thiếu
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var adminId = Guid.NewGuid();
            await context.Users.AddAsync(new User
            {
                Id = adminId,
                FullName = "System Administrator",
                Role = "Admin",
                Email = "admin@unieventhub.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            await context.SaveChangesAsync();

            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category { Name = "Hội thảo chuyên đề", Description = "Các hội thảo về chuyên môn" },
                    new Category { Name = "Văn hóa - Nghệ thuật", Description = "Sự kiện âm nhạc, triển lãm" },
                    new Category { Name = "Thể thao", Description = "Các giải đấu, hội thao" }
                );
            }

            if (!await context.Tags.AnyAsync())
            {
                await context.Tags.AddRangeAsync(
                    new Tag { Name = "IT", Description = "Công nghệ thông tin" },
                    new Tag { Name = "SoftSkills", Description = "Kỹ năng mềm" },
                    new Tag { Name = "Music", Description = "Âm nhạc" },
                    new Tag { Name = "Sports", Description = "Thể thao" },
                    new Tag { Name = "Startup", Description = "Khởi nghiệp" }
                );
            }

            if (!await context.Venues.AnyAsync())
            {
                await context.Venues.AddAsync(new Venue
                {
                    Name = "Hội trường A",
                    Address = "Cơ sở chính",
                    MaxCapacity = 500,
                    Description = "Hội trường lớn nhất dành cho các sự kiện trọng điểm"
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
