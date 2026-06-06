# UniEvent Hub Workspace Launchpad

Hệ thống workspace đã được cập nhật để sẵn sàng cho quá trình phát triển dự án **UniEvent Hub** theo đúng tài liệu đặc tả hệ thống mới nhất.

---

## 1. Tổng Quan Kiến Trúc & Công Nghệ
- **Tên dự án:** UniEvent Hub (Hệ thống Quản lý và Khám phá Sự kiện dành cho Sinh viên và Ban tổ chức cấp trường Đại học)
- **Kiến trúc:** 3-Layer Architecture (Data Access Layer - DAL, Business Logic Layer - BLL, Presentation Layer - PL)
- **Design Patterns:** Dependency Injection (DI), Repository Pattern, Unit of Work
- **Công nghệ lõi:**
  - **Backend:** ASP.NET Core 8.0, Entity Framework Core (Code-First)
  - **Frontend:** ASP.NET Core MVC, Razor Pages, Blazor Web App
  - **Real-time:** SignalR, WebSockets
  - **Chạy ngầm & Tính toán:** Background Worker Service, TPL (Task Parallel Library), PLINQ
  - **Database:** SQL Server / Oracle Database

---

## 2. Bản Đồ Phân Hệ Tính Năng (System Features)

Hệ thống được chia thành 5 phân hệ tương ứng với các tính năng nghiệp vụ:

| Mã tính năng | Phân hệ (Module) | Tên tính năng | Mô tả nghiệp vụ |
|:---|:---|:---|:---|
| **FE-01** | **Identity & Administration** | Identity & Auth | Đăng ký, đăng nhập, phân quyền (Role-based: Admin, Organizer, Student). |
| **FE-09** | **Identity & Administration** | Admin Control Panel | Quản trị người dùng (khóa/mở khóa) và xem dashboard tổng quan số liệu. |
| **FE-02** | **Event Management** | Event CRUD | Tạo, sửa, xóa, quản lý trạng thái sự kiện theo Category và Tags. |
| **FE-05** | **Event Management** | Venue Management | Quản lý địa điểm tổ chức, giới hạn sức chứa để chặn lố vé. |
| **FE-13** | **Event Management** | Event Requests | Cổng tiếp nhận và duyệt đề xuất ý tưởng sự kiện từ sinh viên. |
| **FE-03** | **Student Experience** | Search & Filter Events | Tìm kiếm và lọc đa điều kiện (multi-tag, thời gian, danh mục). |
| **FE-08** | **Student Experience** | Weather API Integration | Tích hợp API thời tiết thực tế tại địa điểm tổ chức (có Caching). |
| **FE-11** | **Student Experience** | Bookmark Action | Lưu sự kiện yêu thích (Wishlist) và quản lý danh sách cá nhân. |
| **FE-04** | **Real-time Operations** | Live Ticket Booking | Đặt vé trực tuyến sử dụng Optimistic Concurrency chống race conditions. |
| **FE-10** | **Real-time Operations** | Live Dashboard | Theo dõi biểu đồ tốc độ đặt vé thời gian thực sử dụng SignalR. |
| **FE-15** | **Real-time Operations** | Live Q&A Hub | Bình luận, hỏi đáp trực tiếp thời gian thực dưới bài viết sự kiện. |
| **FE-06** | **Post-Event & Automation** | Automated Email Reminders | Background Worker tự động quét DB gửi email nhắc nhở tham gia. |
| **FE-07** | **Post-Event & Automation** | Feedback Submission | Hệ thống gửi đánh giá sau sự kiện (chỉ cho sinh viên đã có vé), hỗ trợ đánh giá đa tiêu chí (chấm điểm độc lập Diễn giả, Hậu cần...) sử dụng tính toán song song (PLINQ). |
| **FE-12** | **Post-Event & Automation** | Follows System | Theo dõi (Follow) ban tổ chức để nhận cập nhật ưu tiên. |


---

## 3. Thiết Kế Cơ Sở Dữ Liệu (Database Schema)

Dưới đây là sơ đồ quan hệ thực thể giữa 15 bảng dữ liệu được phân nhóm theo Module:

```mermaid
erDiagram
    Users ||--o{ Events : "organizes"
    Users ||--o{ EventRequests : "submits"
    Users ||--o{ Bookmarks : "bookmarks"
    Users ||--o{ Bookings : "books"
    Users ||--o{ EventComments : "writes"
    Users ||--o{ Follows : "follower/followee"
    
    Venues ||--o{ Events : "hosts"
    
    Events ||--o{ EventCategories : "has"
    Categories ||--o{ EventCategories : "categorizes"
    
    Events ||--o{ EventTags : "has"
    Tags ||--o{ EventTags : "tags"
    
    Events ||--o{ Bookmarks : "bookmarked_in"
    Events ||--o{ Bookings : "has_bookings"
    Events ||--o{ EventComments : "has_comments"
    Events ||--o{ EventReminders : "has_reminder"
    
    Bookings ||--oI Feedbacks : "evaluated_by"
    Feedbacks ||--o{ FeedbackDetails : "has_details"
```

### Chi tiết các bảng dữ liệu:

#### Phân hệ Identity & Admin
1. **Users**
   - `Id` (Guid, PK)
   - `FullName` (nvarchar(100), Required)
   - `StudentCode` (varchar(20), Nullable)
   - `Role` (varchar(20), Required) — *Admin/Organizer/Student*
   - `IsActive` (bit, Default: true)
   - `Email` (varchar, Required)
   - `AvatarUrl` (varchar, Nullable)
   - `CreatedAt` (datetime2, Required)

#### Phân hệ Core Organizer
2. **Venues**
   - `Id` (int, PK)
   - `Name` (nvarchar(100), Required)
   - `MaxCapacity` (int, Required)
   - `Address` (nvarchar(200), Required)
   - `Description` (nvarchar(500), Nullable)

3. **Events**
   - `Id` (Guid, PK)
   - `OrganizerId` (Guid, FK to Users)
   - `VenueId` (int, FK to Venues)
   - `Title` (nvarchar(200), Required)
   - `Description` (nvarchar(max), Required)
   - `BannerUrl` (varchar, Required)
   - `StartTime` (datetime2, Required)
   - `EndTime` (datetime2, Required)
   - `Status` (varchar(20), Required) — *Draft/Published/Completed*
   - `CreatedAt` (datetime2, Required)
   - `RowVersion` (byte[], ConcurrencyToken)

4. **Categories**
   - `Id` (int, PK)
   - `Name` (nvarchar(50), Required)
   - `Description` (nvarchar(200), Nullable)

5. **EventCategories**
   - `EventId` (Guid, PK, FK to Events)
   - `CategoryId` (int, PK, FK to Categories)

6. **Tags**
   - `Id` (int, PK)
   - `Name` (nvarchar(50), Required)
   - `Description` (nvarchar(200), Nullable)

7. **EventTags**
   - `EventId` (Guid, PK, FK to Events)
   - `TagId` (int, PK, FK to Tags)

8. **EventRequests**
   - `Id` (int, PK)
   - `StudentId` (Guid, FK to Users)
   - `Topic` (nvarchar(200), Required)
   - `Description` (nvarchar(max), Required)
   - `Status` (varchar(20), Default: Pending)
   - `SubmittedAt` (datetime2, Required)
   - `ResponseMessage` (nvarchar(500), Nullable)

#### Phân hệ Student UI
9. **Bookmarks**
   - `StudentId` (Guid, PK, FK to Users)
   - `EventId` (Guid, PK, FK to Events)
   - `SavedAt` (datetime2, Required)
   - `Notes` (nvarchar(200), Nullable)

#### Phân hệ RealTime Booking
10. **Bookings**
    - `Id` (Guid, PK)
    - `EventId` (Guid, FK to Events)
    - `StudentId` (Guid, FK to Users)
    - `TicketCode` (varchar(50), Required) — *Mã vé QR*
    - `BookingTime` (datetime2, Required)
    - `Status` (varchar(20), Required)
    - `IsCheckedIn` (bit, Default: false)

11. **EventComments**
    - `Id` (Guid, PK)
    - `EventId` (Guid, FK to Events)
    - `UserId` (Guid, FK to Users)
    - `ParentCommentId` (Guid, Nullable, FK to self) — *Hỗ trợ Reply*
    - `Content` (nvarchar(500), Required)
    - `CreatedAt` (datetime2, Required)

#### Phân hệ Background & Post-Event
12. **EventReminders**
    - `EventId` (Guid, PK, FK to Events)
    - `ScheduledTime` (datetime2, Required)
    - `IsEmailSent` (bit, Default: false)
    - `SentAt` (datetime2, Nullable)

13. **Follows**
    - `FollowerId` (Guid, PK, FK to Users)
    - `FolloweeId` (Guid, PK, FK to Users)
    - `FollowedAt` (datetime2, Required)

14. **Feedbacks**
    - `Id` (Guid, PK)
    - `BookingId` (Guid, FK to Bookings)
    - `GeneralComment` (nvarchar(500), Nullable)
    - `SubmittedAt` (datetime2, Required)

15. **FeedbackDetails**
    - `Id` (int, PK)
    - `FeedbackId` (Guid, FK to Feedbacks)
    - `Criteria` (varchar(50), Required)
    - `Score` (int, Required) — *Từ 1 đến 5*
