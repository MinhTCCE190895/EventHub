using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BCrypt.Net;
using System.Threading;

namespace DAL.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Kiểm tra nhanh xem có cần chạy Migration không trước khi dùng Mutex để tránh block luồng khởi chạy
        bool needsMigration = false;
        try
        {
            var pending = await context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                needsMigration = true;
            }
        }
        catch (Exception)
        {
            // Database hoặc bảng __EFMigrationsHistory chưa tồn tại
            needsMigration = true;
        }

        if (needsMigration)
        {
            using var mutex = new Mutex(false, "UniEventHubDbMigrationMutex");
            try
            {
                var hasHandle = mutex.WaitOne(TimeSpan.FromSeconds(30));
                if (hasHandle)
                {
                    await context.Database.MigrateAsync();
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
            catch (AbandonedMutexException)
            {
                await context.Database.MigrateAsync();
            }
            finally
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch (Exception) { }
            }
        }


        // Cập nhật địa chỉ đầy đủ có tỉnh thành cho các Venue đã tồn tại từ trước để đồng bộ tính năng thời tiết
        var existingA = await context.Venues.FirstOrDefaultAsync(v => v.Name == "Hội trường A");
        if (existingA != null && !existingA.Address.Contains("TP. Hồ Chí Minh"))
        {
            existingA.Address = "Cơ sở 1 - 123 Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh";
            context.Venues.Update(existingA);
        }
        var existingB = await context.Venues.FirstOrDefaultAsync(v => v.Name == "Hội trường B");
        if (existingB != null && !existingB.Address.Contains("TP. Hồ Chí Minh"))
        {
            existingB.Address = "Cơ sở 2 - 456 Võ Văn Ngân, Thủ Đức, TP. Hồ Chí Minh";
            context.Venues.Update(existingB);
        }
        await context.SaveChangesAsync();

        var studentId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        if (await context.Users.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            bool hasChanges = false;

            var seedEmails = new[] { "admin@unieventhub.com", "organizer@unieventhub.com", "khoi.student@fpt.edu.vn" };
            var seedUsers = users.Where(u => seedEmails.Contains(u.Email)).ToList();

            // Chỉ reset password cho các tài khoản seed mặc định thành 123456 nếu chưa được hash
            foreach (var u in seedUsers)
            {
                // Nếu password hash chưa được hash bằng BCrypt (không bắt đầu bằng $2) thì mới hash lại
                if (string.IsNullOrEmpty(u.PasswordHash) || !u.PasswordHash.StartsWith("$2"))
                {
                    u.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    hasChanges = true;
                }
            }

            // Đảm bảo luôn có ít nhất 1 tài khoản Student để test
            if (!users.Any(u => u.Id == studentId))
            {
                context.Users.Add(new User
                {
                    Id = studentId,
                    FullName = "Khoi Sinh Vien",
                    Role = "Student",
                    Email = "khoi.student@fpt.edu.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                hasChanges = true;
            }

            if (hasChanges)
            {
                context.Users.UpdateRange(users);
                await context.SaveChangesAsync();
            }
        }
        else
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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = organizerId,
                    FullName = "Nguyen Van Organizer",
                    Role = "Organizer",
                    Email = "organizer@unieventhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = studentId,
                    FullName = "Khoi Sinh Vien",
                    Role = "Student",
                    Email = "khoi.student@fpt.edu.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }

        using (var seedMutex = new Mutex(false, "UniEventHubDbSeedMutex"))
        {
            try
            {
                bool hasHandle = false;
                try
                {
                    hasHandle = seedMutex.WaitOne(TimeSpan.FromSeconds(30));
                }
                catch (AbandonedMutexException)
                {
                    hasHandle = true; // Mutex bị tiến trình cũ bỏ rơi, ta chiếm quyền điều khiển để seed tiếp
                }

                if (hasHandle)
                {
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
        var venueA = new Venue { Name = "Hội trường A", Address = "Cơ sở 1 - 123 Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh", MaxCapacity = 500 };
        var venueB = new Venue { Name = "Hội trường B", Address = "Cơ sở 2 - 456 Võ Văn Ngân, Thủ Đức, TP. Hồ Chí Minh", MaxCapacity = 200 };
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
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Học máy và Ứng dụng AI",
                Description = "Giới thiệu các mô hình học máy cơ bản và ứng dụng thực tiễn trong công nghiệp.",
                BannerUrl = "https://placehold.co/600x300/0284c7/white?text=Machine+Learning",
                StartTime = now.AddDays(5),
                EndTime = now.AddDays(5).AddHours(3),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Kỹ năng làm việc nhóm hiệu quả",
                Description = "Chia sẻ kỹ năng phối hợp, giao tiếp và giải quyết xung đột trong nhóm làm việc.",
                BannerUrl = "https://placehold.co/600x300/f59e0b/white?text=Teamwork+Skills",
                StartTime = now.AddDays(8),
                EndTime = now.AddDays(8).AddHours(2),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Giải bóng rổ sinh viên tranh cúp 2025",
                Description = "Giải đấu bóng rổ kịch tính quy tụ các đội tuyển xuất sắc từ các khoa.",
                BannerUrl = "https://placehold.co/600x300/e11d48/white?text=Basketball+Cup",
                StartTime = now.AddDays(12),
                EndTime = now.AddDays(12).AddHours(4),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Lập trình Web với React và Next.js",
                Description = "Tìm hiểu kỹ thuật xây dựng ứng dụng Web hiện đại và tối ưu hóa SEO.",
                BannerUrl = "https://placehold.co/600x300/0891b2/white?text=React+NextJS",
                StartTime = now.AddDays(15),
                EndTime = now.AddDays(15).AddHours(3),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Triển lãm tranh sinh viên sắc màu hội họa",
                Description = "Nơi trưng bày các tác phẩm nghệ thuật sáng tạo của các bạn sinh viên tài năng.",
                BannerUrl = "https://placehold.co/600x300/8b5cf6/white?text=Art+Exhibition",
                StartTime = now.AddDays(18),
                EndTime = now.AddDays(18).AddHours(5),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Workshop thiết kế UI/UX cơ bản",
                Description = "Học cách nghiên cứu người dùng, vẽ wireframe và thiết kế giao diện chuẩn chỉ.",
                BannerUrl = "https://placehold.co/600x300/ec4899/white?text=UIUX+Design",
                StartTime = now.AddDays(20),
                EndTime = now.AddDays(20).AddHours(3),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueA.Id,
                Title = "Ngày hội tuyển dụng Job Fair 2025",
                Description = "Cơ hội ứng tuyển trực tiếp vào hơn 50 doanh nghiệp công nghệ lớn nhỏ.",
                BannerUrl = "https://placehold.co/600x300/10b981/white?text=Job+Fair",
                StartTime = now.AddDays(25),
                EndTime = now.AddDays(25).AddHours(8),
                Status = "Published",
                CreatedAt = now
            },
            new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = orgId,
                VenueId = venueB.Id,
                Title = "Chuyên đề An toàn thông tin và Cyber Security",
                Description = "Cập nhật các mối đe dọa bảo mật mới nhất và cách phòng vệ hệ thống.",
                BannerUrl = "https://placehold.co/600x300/ef4444/white?text=Cyber+Security",
                StartTime = now.AddDays(30),
                EndTime = now.AddDays(30).AddHours(4),
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
            new EventCategory { EventId = events[4].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[5].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[6].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[7].Id, CategoryId = catSport.Id },
            new EventCategory { EventId = events[8].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[9].Id, CategoryId = catArt.Id },
            new EventCategory { EventId = events[10].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[11].Id, CategoryId = catIT.Id },
            new EventCategory { EventId = events[12].Id, CategoryId = catIT.Id }
        );

        // --- EventTags ---
        await context.EventTags.AddRangeAsync(
            new EventTag { EventId = events[0].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[0].Id, TagId = tagSkill.Id },
            new EventTag { EventId = events[1].Id, TagId = tagMusic.Id },
            new EventTag { EventId = events[2].Id, TagId = tagSport.Id },
            new EventTag { EventId = events[3].Id, TagId = tagStartup.Id },
            new EventTag { EventId = events[3].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[4].Id, TagId = tagSkill.Id },
            new EventTag { EventId = events[5].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[6].Id, TagId = tagSkill.Id },
            new EventTag { EventId = events[7].Id, TagId = tagSport.Id },
            new EventTag { EventId = events[8].Id, TagId = tagIT.Id },
            new EventTag { EventId = events[9].Id, TagId = tagMusic.Id },
            new EventTag { EventId = events[10].Id, TagId = tagSkill.Id },
            new EventTag { EventId = events[11].Id, TagId = tagStartup.Id },
            new EventTag { EventId = events[12].Id, TagId = tagIT.Id }
        );

        await context.SaveChangesAsync();
                }
            }
            finally
            {
                try
                {
                    seedMutex.ReleaseMutex();
                }
                catch (Exception) { }
            }
        }
    }
}