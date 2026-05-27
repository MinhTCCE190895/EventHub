# Skill: Bogus Data Seeder

---
name: bogus-data-seeder
description: Tự động cài đặt Bogus và viết mã sinh dữ liệu giả (Seed/Mock Data) cho 9 thực thể cốt lõi trong UniEvent Hub.
---

## 1. Tổng quan (Overview)
Kích hoạt tự động khi User yêu cầu **"Mock data"** hoặc **"Seed data"** để phục vụ việc kiểm thử giao diện và hiệu năng.

## 2. Quy trình Cài đặt & Cấu hình (Setup Workflow)

### Bước 1: Tự động cài đặt NuGet Bogus
Khi có lệnh yêu cầu, Agent tự động chạy lệnh sau để thêm thư viện Bogus vào dự án DAL:
```bash
dotnet add <DALProjectPath> package Bogus
```

### Bước 2: Tạo lớp DataSeeder
Viết mã nguồn seeding nằm trong tầng `DAL/Data` hoặc một class helper chuyên biệt, cấu hình quy tắc sinh dữ liệu ngẫu nhiên cho **9 thực thể**.

## 3. Quy tắc Sinh Dữ liệu mẫu (Bogus Rules)

```csharp
using Bogus;
using UniEventHub.DAL.Models;

public static class DataSeeder
{
    public static void SeedAll(ModelBuilder modelBuilder)
    {
        // 1. Seed Roles
        var roles = new[] {
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Organizer" },
            new Role { Id = 3, Name = "Participant" }
        };
        modelBuilder.Entity<Role>().HasData(roles);

        // 2. Seed Accounts
        var accountId = 1;
        var accountFaker = new Faker<Account>()
            .RuleFor(a => a.Id, f => accountId++)
            .RuleFor(a => a.Email, f => f.Internet.Email())
            .RuleFor(a => a.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Password123"))
            .RuleFor(a => a.RoleId, f => f.PickRandom(1, 2, 3));
        var accounts = accountFaker.Generate(20);
        modelBuilder.Entity<Account>().HasData(accounts);

        // 3. Seed Categories
        var categories = new[] {
            new Category { Id = 1, Name = "Workshop" },
            new Category { Id = 2, Name = "Seminar" },
            new Category { Id = 3, Name = "Conference" }
        };
        modelBuilder.Entity<Category>().HasData(categories);

        // 4. Seed Locations
        var locationId = 1;
        var locationFaker = new Faker<Location>()
            .RuleFor(l => l.Id, f => locationId++)
            .RuleFor(l => l.Name, f => f.Company.CompanyName() + " Hall")
            .RuleFor(l => l.Address, f => f.Address.FullAddress())
            .RuleFor(l => l.Capacity, f => f.Random.Number(50, 500));
        var locations = locationFaker.Generate(5);
        modelBuilder.Entity<Location>().HasData(locations);

        // 5. Seed Events
        var eventId = 1;
        var eventFaker = new Faker<Event>()
            .RuleFor(e => e.Id, f => eventId++)
            .RuleFor(e => e.Title, f => f.Lorem.Sentence(3))
            .RuleFor(e => e.Description, f => f.Lorem.Paragraph())
            .RuleFor(e => e.StartTime, f => f.Date.Soon(10))
            .RuleFor(e => e.EndTime, (f, e) => e.StartTime.AddHours(f.Random.Number(2, 6)))
            .RuleFor(e => e.Capacity, f => f.Random.Number(20, 100))
            .RuleFor(e => e.CategoryId, f => f.PickRandom(1, 2, 3))
            .RuleFor(e => e.LocationId, f => f.PickRandom(1, 2, 3, 4, 5));
        var events = eventFaker.Generate(15);
        modelBuilder.Entity<Event>().HasData(events);

        // 6. Seed Tags
        var tags = new[] {
            new Tag { Id = 1, Name = "Tech" },
            new Tag { Id = 2, Name = "AI" },
            new Tag { Id = 3, Name = "Business" }
        };
        modelBuilder.Entity<Tag>().HasData(tags);

        // 7. Seed EventTag (Many-to-Many Bridge)
        var eventTags = new List<EventTag>
        {
            new EventTag { EventId = 1, TagId = 1 },
            new EventTag { EventId = 1, TagId = 2 },
            new EventTag { EventId = 2, TagId = 3 }
        };
        modelBuilder.Entity<EventTag>().HasData(eventTags);

        // 8. Seed Registrations
        var regId = 1;
        var regFaker = new Faker<Registration>()
            .RuleFor(r => r.Id, f => regId++)
            .RuleFor(r => r.AccountId, f => f.PickRandom(accounts).Id)
            .RuleFor(r => r.EventId, f => f.PickRandom(events).Id)
            .RuleFor(r => r.RegistrationDate, f => f.Date.Past(1))
            .RuleFor(r => r.Status, f => f.PickRandom("Pending", "Confirmed", "Cancelled"));
        var registrations = regFaker.Generate(30);
        modelBuilder.Entity<Registration>().HasData(registrations);

        // 9. Seed Feedbacks
        var feedbackId = 1;
        var feedbackFaker = new Faker<Feedback>()
            .RuleFor(f => f.Id, f => feedbackId++)
            .RuleFor(f => f.AccountId, f => f.PickRandom(accounts).Id)
            .RuleFor(f => f.EventId, f => f.PickRandom(events).Id)
            .RuleFor(f => f.Rating, f => f.Random.Number(1, 5))
            .RuleFor(f => f.Comment, f => f.Lorem.Sentence())
            .RuleFor(f => f.CreatedDate, f => f.Date.Past(1));
        var feedbacks = feedbackFaker.Generate(25);
        modelBuilder.Entity<Feedback>().HasData(feedbacks);
    }
}
```
