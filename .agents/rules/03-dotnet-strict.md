# Luật C# 13, .NET 9 & Repository Pattern Nghiêm Ngặt (Strict Rules)

## 1. Luật C# 13 & .NET 9
- **Tính năng mới của C# 13:**
  - Khuyến khích sử dụng bộ chỉ mục ngầm trong object initializers (Implicit indexer access).
  - Sử dụng từ khóa `params` kết hợp với `ReadOnlySpan<T>` hoặc `IEnumerable<T>` để tối ưu hóa bộ nhớ và hiệu suất.
  - Sử dụng `Lock` object mới thay thế cho `object` truyền thống khi đồng bộ hóa đa luồng (`lock (new Lock())`).
- **Đặc tả .NET 9:**
  - Tối ưu hóa việc dùng `System.Text.Json` với các cải tiến về schema serialization.
  - Sử dụng các tính năng LINQ mới như `.CountBy()` và `.AggregateBy()` thay thế cho `.GroupBy().Select()`.

## 2. Chuẩn mực Repository Pattern (Tri-Architecture)
- **Tách biệt BLL và DAL:**
  - Các Service tại lớp **BLL (Business Logic Layer)** chỉ tương tác với database thông qua các Interface của Repository được tiêm vào (Dependency Injection).
  - Tầng **DAL (Data Access Layer)** chứa các định nghĩa thực thể (`DAL/Models`), cấu hình database (`DAL/Data`), và các thực thi repository (`DAL/Repositories`) che giấu các chi tiết cụ thể của EF Core `DbContext`.
- **Specification/Query Pattern:**
  - Đóng gói toàn bộ các truy vấn LINQ phức tạp, sắp xếp, phân trang và Eager Loading (`.Include()`) vào các cấu trúc Specification hoặc truy vấn chuyên biệt trong tầng DAL.
  - Hạn chế tối đa việc viết trực tiếp LINQ query tự do (ad-hoc) trong lớp Service (BLL) hoặc Controller (PL).

## 3. Cấu hình EF Core Many-to-Many cho EventTag
- Hệ thống có thực thể cầu nối `EventTag` liên kết `Event` và `Tag`.
- Trong EF Core 9.0, cấu hình mối quan hệ Many-to-Many trong Fluent API sử dụng `UsingEntity` để định nghĩa rõ ràng thực thể trung gian `EventTag`:
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
