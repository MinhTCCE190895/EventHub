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

        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            // --- Users ---
            var adminId = Guid.NewGuid();
            var organizerId = Guid.NewGuid();

            await context.Users.AddRangeAsync(
                new User
                {
                    Id = adminId,
                    FullName = "System Administrator",
                    Role = "Admin",
                    Email = "admin@unieventhub.com",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = organizerId,
                    FullName = "Nguyen Van Organizer",
                    Role = "Organizer",
                    Email = "organizer@unieventhub.com",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }

        // Đảm bảo có 1 user sinh viên để test Đặt vé
        var studentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!await context.Users.AnyAsync(u => u.Id == studentId))
        {
            await context.Users.AddAsync(new User
            {
                Id = studentId,
                FullName = "Khoi Sinh Vien",
                Role = "Student",
                Email = "khoi.student@fpt.edu.vn",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (await context.Events.AnyAsync())
            return;

        var currentOrganizer = await context.Users.FirstOrDefaultAsync(u => u.Role == "Organizer");
        var orgId = currentOrganizer?.Id ?? Guid.NewGuid();

        // --- Categories ---
        var catIT = new Category { Name = "Hội thảo chuyên đề", Description = "Các hội thảo về chuyên môn" };
        var catArt = new Category { Name = "Văn hóa - Nghệ thuật", Description = "Sự kiện âm nhạc, triển lãm" };
        var catSport = new Category { Name = "Thể thao", Description = "Các giải đấu, hội thao" };
        await context.Categories.AddRangeAsync(catIT, catArt, catSport);

        // --- Tags ---
        var tagIT = new Tag { Name = "IT", Description = "Công nghệ thông tin" };
        var tagSkill = new Tag { Name = "SoftSkills", Description = "Kỹ năng mềm" };
        var tagMusic = new Tag { Name = "Music", Description = "Âm nhạc" };
        var tagSport = new Tag { Name = "Sports", Description = "Thể thao" };
        var tagStartup = new Tag { Name = "Startup", Description = "Khởi nghiệp" };
        await context.Tags.AddRangeAsync(tagIT, tagSkill, tagMusic, tagSport, tagStartup);

        // --- Venues ---
        var venueA = new Venue { Name = "Hội trường A", Address = "Cơ sở 1 - 123 Nguyễn Văn Cừ", MaxCapacity = 500 };
        var venueB = new Venue { Name = "Hội trường B", Address = "Cơ sở 2 - 456 Võ Văn Ngân", MaxCapacity = 200 };
        await context.Venues.AddRangeAsync(venueA, venueB);

        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // --- Events (đủ loại để test filter) ---
        var events = new List<Event>
        {
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Workshop .NET Core nâng cao",
                Description = "Hội thảo chuyên sâu về ASP.NET Core, EF Core và các best practices trong lập trình .NET hiện đại.",
                BannerUrl = "https://placehold.co/600x300/0d6efd/white?text=.NET+Workshop",
                StartTime = now.AddDays(3),
                EndTime = now.AddDays(3).AddHours(4),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Đêm nhạc acoustic sinh viên",
                Description = "Chương trình âm nhạc do chính sinh viên biểu diễn, không gian ấm cúng và thân thiện.",
                BannerUrl = "https://placehold.co/600x300/198754/white?text=Acoustic+Night",
                StartTime = now.AddDays(7),
                EndTime = now.AddDays(7).AddHours(3),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Giải bóng đá sinh viên 2025",
                Description = "Giải đấu bóng đá thường niên dành cho sinh viên toàn trường, tranh cúp vô địch.",
                BannerUrl = "https://placehold.co/600x300/dc3545/white?text=Football+2025",
                StartTime = now.AddDays(14),
                EndTime = now.AddDays(14).AddHours(6),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Seminar Startup & Khởi nghiệp",
                Description = "Gặp gỡ và chia sẻ kinh nghiệm khởi nghiệp cùng các founder trẻ trong và ngoài trường.",
                BannerUrl = "https://placehold.co/600x300/fd7e14/white?text=Startup+Seminar",
                StartTime = now.AddDays(-2),
                EndTime = now.AddDays(-2).AddHours(3),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Kỹ năng phỏng vấn xin việc",
                Description = "Workshop thực hành kỹ năng mềm: CV, phỏng vấn, và cách tìm kiếm việc làm sau tốt nghiệp.",
                BannerUrl = "https://placehold.co/600x300/6f42c1/white?text=Interview+Skills",
                StartTime = now.AddHours(-1),
                EndTime = now.AddHours(2),
                Status = "Published",
                CreatedAt = now
            },
            // Event ở trạng thái Draft — không được hiển thị trên Explore
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Sự kiện chưa duyệt (Draft)",
                Description = "Event này ở trạng thái Draft, không được hiển thị.",
                BannerUrl = "",
                StartTime = now.AddDays(5),
                EndTime = now.AddDays(5).AddHours(2),
                Status = "Draft",
                CreatedAt = now
            }
        };

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        // --- EventCategories ---
        await context.EventCategories.AddRangeAsync(
            new EventCategory { EventId = events[0].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[1].Id, CategoryId = catArt.Id },
            new EventCategory { EventId = events[2].Id, CategoryId = catSport.Id },
            new EventCategory { EventId = events[3].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[4].Id, CategoryId = catIT.Id }
        );

        // --- EventTags ---
        await context.EventTags.AddRangeAsync(
            new EventTag { EventId = events[0].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[0].Id, TagId = tagSkill.Id },
            new EventTag { EventId = events[1].Id, TagId = tagMusic.Id },
            new EventTag { EventId = events[2].Id, TagId = tagSport.Id },
            new EventTag { EventId = events[3].Id, TagId = tagStartup.Id },
            new EventTag { EventId = events[3].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[4].Id, TagId = tagSkill.Id }
        );

        await context.SaveChangesAsync();
    }
}
