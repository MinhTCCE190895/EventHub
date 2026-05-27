# Kịch bản Scaffolding - UniEvent Hub

---
name: scaffolding-templates
description: Kịch bản hướng dẫn xây dựng và dựng khung code nhanh cho các module .NET 9, EF Core, MVC, Razor Pages, Blazor và Worker Service của UniEvent Hub.
---

## 1. Dựng khung cơ sở dữ liệu và Thực thể (Database & Entity Scaffolding)
Khi tạo mới một thực thể trong hệ thống:
1. Định nghĩa thực thể trong lớp **Core** (Entities), sử dụng các C# 13 features (ví dụ: primary constructors, required properties, init-only properties).
2. Tạo Fluent API mapping cấu hình bảng trong lớp **Infrastructure** (`DbContext`).
3. Đảm bảo cấu hình mối quan hệ khóa ngoại và indexes rõ ràng, đặc biệt cho các bảng chứa quan hệ Many-to-Many như `EventTag`.

```csharp
public class EventTag
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
```

## 2. Tạo Repository và Service (BLL/DAL Scaffolding)
1. Dựng interface `I[Entity]Repository` kế thừa từ `IRepository<T>` (Ardalis).
2. Viết các specifications kế thừa từ `Specification<T>` để đóng gói câu truy vấn LINQ:
```csharp
public class EventWithDetailsSpecification : Specification<Event>
{
    public EventWithDetailsSpecification(int eventId)
    {
        Query.Where(e => e.Id == eventId)
             .Include(e => e.Category)
             .Include(e => e.Location)
             .Include(e => e.Tags);
    }
}
```
3. Khai báo service Interface và Implementation tại lớp Business Logic.

## 3. Sinh mã giao diện (UI Scaffolding)
- **MVC (Identity):**
  - Sử dụng ASP.NET Core Identity Scaffolder để ghi đè các View đăng nhập/đăng ký nếu cần tùy biến.
  - Sử dụng Command: `dotnet aspnet-codegenerator identity`
- **Razor Pages (Location & Event):**
  - Sinh mã CRUD thô sử dụng `dotnet aspnet-codegenerator razorpage`.
  - Giữ giao diện nguyên bản Bootstrap, không thêm custom styles phức tạp.
- **Blazor (Registration Dashboard & Feedback Analytics):**
  - Dựng các trang Razor Component với layout cơ bản sử dụng MudBlazor grid (`MudGrid`, `MudItem`), bảng dữ liệu (`MudTable`) và biểu đồ (`MudChart`).
  - Sử dụng Render Mode: `@rendermode InteractiveServer` hoặc `@rendermode InteractiveWebAssembly` tùy theo yêu cầu của trang.
