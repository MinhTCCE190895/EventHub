# Tài liệu Thiết kế Cơ sở Dữ liệu (Database Schema Design)

Tài liệu này chi tiết cấu trúc bảng và các mối quan hệ giữa **9 thực thể cốt lõi** trong cơ sở dữ liệu của **UniEvent Hub**.

---

## 1. Sơ đồ Quan hệ Thực thể (ERD Diagram)

```mermaid
erDiagram
    Role ||--o{ Account : "has"
    Account ||--o{ Registration : "makes"
    Account ||--o{ Feedback : "writes"
    Category ||--o{ Event : "classifies"
    Location ||--o{ Event : "hosts"
    Event ||--o{ Registration : "receives"
    Event ||--o{ Feedback : "gets"
    Event ||--o{ EventTag : "tagged with"
    Tag ||--o{ EventTag : "belongs to"

    Role {
        int Id PK
        string Name
    }
    Account {
        int Id PK
        string Email
        string PasswordHash
        int RoleId FK
    }
    Category {
        int Id PK
        string Name
    }
    Location {
        int Id PK
        string Name
        string Address
        int Capacity
    }
    Event {
        int Id PK
        string Title
        string Description
        DateTime StartTime
        DateTime EndTime
        int Capacity
        int CategoryId FK
        int LocationId FK
    }
    Tag {
        int Id PK
        string Name
    }
    EventTag {
        int EventId PK, FK
        int TagId PK, FK
    }
    Registration {
        int Id PK
        int AccountId FK
        int EventId FK
        DateTime RegistrationDate
        string Status
    }
    Feedback {
        int Id PK
        int AccountId FK
        int EventId FK
        int Rating
        string Comment
        DateTime CreatedDate
    }
```

---

## 2. Quy chuẩn Fluent API EF Core 8.0

### 2.1. Cấu hình Many-to-Many cho EventTag
Thực thể trung gian `EventTag` được cấu hình tường minh để cho phép lưu trữ thêm các thuộc tính mở rộng nếu cần thiết:
```csharp
builder.Entity<Event>()
    .HasMany(e => e.Tags)
    .WithMany(t => t.Events)
    .UsingEntity<EventTag>(
        l => l.HasOne<Tag>().WithMany().HasForeignKey(et => et.TagId),
        r => r.HasOne<Event>().WithMany().HasForeignKey(et => et.EventId)
    );
```

### 2.2. Indexes & Performance Optimizations
- **Unique Indexes:** Cấu hình unique index cho `Email` trong bảng `Account` và `Name` trong các bảng cấu hình như `Category`, `Tag`.
- **Foreign Key Cascade Delete:** Thiết lập `OnDelete(DeleteBehavior.Restrict)` cho các khóa ngoại liên quan đến `Event` và `Account` để tránh việc xóa dây chuyền mất mát dữ liệu quan trọng ngoài ý muốn.
