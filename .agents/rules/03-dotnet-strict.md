# Luật C# 12, .NET 8 & Repository Pattern Nghiêm Ngặt (Strict Rules)

## 1. Luật C# 12 & .NET 8
- **Tính năng mới của C# 12:**
  - Khuyến khích sử dụng biểu thức Collection (`[item1, item2]`) thay thế cho `new[]` hoặc `new List<T>`.
  - Sử dụng **Primary Constructors** cho Class và Struct khi khởi tạo các dịch vụ/dependency đơn giản để rút gọn code boilerplate.
  - Sử dụng directive `using` alias để đặt alias cho bất kỳ kiểu dữ liệu nào (tuple, pointer, array...).
  - Sử dụng tham số mặc định cho biểu thức Lambda (Default lambda parameters).
- **Đặc tả .NET 8:**
  - Tối ưu hóa hiệu năng serialization với `System.Text.Json` (sử dụng Source Generators nếu cần).
  - Sử dụng các API hiệu năng cao của .NET 8 như `FrozenDictionary` hoặc `FrozenSet` cho các dữ liệu cấu hình chỉ đọc.

## 2. Chuẩn mực Repository Pattern (Tri-Architecture)
- **Tách biệt BLL và DAL:**
  - Các Service tại lớp **BLL (Business Logic Layer)** chỉ tương tác với database thông qua các Interface của Repository được tiêm vào (Dependency Injection).
  - Tầng **DAL (Data Access Layer)** chứa các định nghĩa thực thể (`DAL/Models`), cấu hình database (`DAL/Data`), và các thực thi repository (`DAL/Repositories`) che giấu các chi tiết cụ thể của EF Core `DbContext`.
- **Specification/Query Pattern:**
  - Đóng gói toàn bộ các truy vấn LINQ phức tạp, sắp xếp, phân trang và Eager Loading (`.Include()`) vào các cấu trúc Specification hoặc truy vấn chuyên biệt trong tầng DAL.
  - Hạn chế tối đa việc viết trực tiếp LINQ query tự do (ad-hoc) trong lớp Service (BLL) hoặc Controller (PL).

## 3. Cấu hình EF Core Many-to-Many cho EventTag
- Hệ thống có thực thể cầu nối `EventTag` liên kết `Event` và `Tag`.
- Trong EF Core 8.0, cấu hình mối quan hệ Many-to-Many trong Fluent API sử dụng `UsingEntity` để định nghĩa rõ ràng thực thể trung gian `EventTag`:
```csharp
builder.Entity<Event>()
    .HasMany(e => e.Tags)
    .WithMany(t => t.Events)
    .UsingEntity<EventTag>(
        l => l.HasOne<Tag>().WithMany().HasForeignKey(et => et.TagId),
        r => r.HasOne<Event>().WithMany().HasForeignKey(et => et.EventId)
    );
```
- Đảm bảo thực thể `EventTag` có thể chứa thêm các trường dữ liệu tùy biến nếu cần thiết (ví dụ: ngày tạo, người gán tag).

