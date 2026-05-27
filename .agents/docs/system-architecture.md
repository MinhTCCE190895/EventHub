# Tài liệu Kiến trúc Hệ thống (System Architecture Documentation)

Chào mừng đến với tài liệu kỹ thuật cốt lõi của **UniEvent Hub**. Tài liệu này được biên soạn bởi Senior .NET 9 Architect nhằm hướng dẫn cấu trúc dự án và quy chuẩn thiết kế.

---

## 1. Cấu trúc Dự án (3-Tier Layered Architecture Layout)

Hệ thống áp dụng chuẩn cấu trúc **3-Tier Layered Architecture** (BLL, DAL, PL) được tổ chức thông qua cấu hình trong file `EventHub.sln`:

```text
UniEventHub/
│
├── BLL/                                   # Business Logic Layer (Lớp xử lý nghiệp vụ)
│   └── Services/                          # Các service cung cấp logic nghiệp vụ chính
│
├── DAL/                                   # Data Access Layer (Lớp truy xuất dữ liệu)
│   ├── Models/                            # Chứa 9 thực thể cốt lõi của hệ thống
│   ├── Data/                              # Cấu hình DbContext & Fluent API cho EF Core
│   ├── Repositories/                      # Lớp Repository (Repository Pattern) thực thi truy xuất dữ liệu
│   └── Migrations/                        # File migrations của EF Core
│
└── PL/                                    # Presentation Layer (Lớp giao diện người dùng)
    └── [Web Project]/                     # Dự án web chính (MVC Identity, Razor Pages Location/Event, Blazor Dashboard/Analytics)
```

---

## 2. Chi tiết 9 Thực thể & Thuộc tính Dự kiến

### 2.1. Phân hệ Identity & User
- **Role:** Định nghĩa quyền hạn trong hệ thống (Admin, Organizer, Participant).
- **Account:** Thông tin tài khoản người dùng, liên kết với ASP.NET Core Identity.

### 2.2. Phân hệ Quản lý Sự kiện & Địa điểm
- **Category:** Thể loại sự kiện (ví dụ: Workshop, Seminar, Webinar).
- **Location:** Địa điểm tổ chức (tên địa điểm, địa chỉ, sức chứa tối đa - Capacity).
- **Event:** Chi tiết sự kiện (tiêu đề, mô tả, thời gian bắt đầu/kết thúc, giới hạn sức chứa, ID Category, ID Location).
- **Tag:** Các từ khóa phân loại sự kiện.
- **EventTag:** Bảng trung gian Many-to-Many liên kết giữa `Event` và `Tag`.

### 2.3. Phân hệ Tương tác & Đăng ký
- **Registration:** Đăng ký tham gia sự kiện của Account (ngày đăng ký, trạng thái, số lượng vé).
- **Feedback:** Đánh giá từ người tham gia (số sao, nhận xét, ngày gửi).

---

## 3. Quy tắc Đặt tên & Coding Conventions

- **Ngôn ngữ:** Tên lớp, thuộc tính, cơ sở dữ liệu sử dụng **tiếng Anh**. Comment code sử dụng tiếng Anh hoặc tiếng Việt tùy thuộc vào yêu cầu của module.
- **Async/Await:** Mọi phương thức truy xuất Database hoặc I/O bound phải có hậu tố `Async` (ví dụ: `GetEventByIdAsync`).
- **Clean Code:** Tuân thủ 100% nguyên tắc SOLID và Clean Code của Uncle Bob.
