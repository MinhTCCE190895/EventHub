# II. Release Package & User Guides

## 1. Deliverable Package

| No. | Deliverable Item | Description | Version |
|---|---|---|---|
| 1 | Project Schedule/Tracking | Bảng Jira/Trello theo dõi tiến độ và các milestone | v1.0 |
| 2 | Project Backlog | Danh sách user stories và yêu cầu chức năng (FE-01 đến FE-15) | v1.0 |
| 3 | Source Codes | Mã nguồn hệ thống EventHub (.NET 9, C# 13, MVC/Razor Pages, Blazor) | v1.0 |
| 4 | Database Script(s) | EF Core Migrations và kịch bản Seed Data khởi tạo | v1.0 |
| 5 | Final Report Document | Tài liệu báo cáo tổng kết, kiến trúc và quyết định kỹ thuật | v1.0 |
| 6 | Test Cases Document | Danh sách kịch bản kiểm thử (Unit test và Manual test) | v1.0 |
| 7 | Defects List | Danh sách các lỗi đã ghi nhận và trạng thái xử lý | v1.0 |
| 8 | Issues List | Danh sách các vấn đề kỹ thuật (Technical Debt/Impediments) | v1.0 |
| 9 | Slide | Bài thuyết trình tổng kết dự án | v1.0 |

## 2. Installation Guides

### 2.1 System Requirements

**Hardware:**
- CPU: Dual-core 2.0 GHz trở lên
- RAM: Tối thiểu 4GB (Khuyến nghị 8GB)
- Storage: Trống tối thiểu 500MB 

**Software:**
- OS: Windows 10/11, macOS, Linux
- Runtime: .NET 9.0 SDK
- Database: SQL Server 2022 hoặc PostgreSQL 15+
- Message/Cache: Redis Server
- Browser: Phiên bản mới nhất của Chrome, Firefox, Edge, Safari

### 2.2 Installation Instruction

1. **Clone mã nguồn:**
   ```bash
   git clone <repository_url>
   cd EventHub
   ```

2. **Cấu hình Database:**
   - Đảm bảo SQL Server / PostgreSQL đang hoạt động.
   - Cập nhật chuỗi kết nối `ConnectionStrings` trong file `appsettings.json` và `appsettings.Development.json` thuộc project MVC/API.

3. **Chạy EF Core Migrations:**
   ```bash
   dotnet ef database update --project DAL --startup-project MVC
   ```

4. **Khởi chạy Redis:**
   - Đảm bảo Redis Server đang chạy ở cổng mặc định `6379`.

5. **Build và Run hệ thống:**
   ```bash
   dotnet build
   dotnet run --project MVC
   ```
   Hệ thống sẽ hoạt động tại `http://localhost:5000` hoặc `https://localhost:5001`.

## 3. User Manual

### 3.1 Overview

**UniEvent Hub** là nền tảng quản lý sự kiện và đặt vé trực tuyến. Hệ thống hỗ trợ đa vai trò với các nhóm chức năng cốt lõi:
- **Người dùng (Attendees):** Tìm kiếm sự kiện (FE-03), đặt vé trực tiếp (FE-04), theo dõi lịch sử và tương tác Live Q&A (FE-15).
- **Ban tổ chức (Organizers):** Quản lý sự kiện (FE-02), giới hạn số lượng theo địa điểm (FE-05), theo dõi Live Dashboard (FE-10).
- **Quản trị viên (Admins):** Quản lý RBAC (FE-01) và Admin Control Panel (FE-09).

### 3.2 Workflow 1: Quản lý sự kiện (Organizer)

**Mục đích:**
Cho phép Organizer tạo sự kiện mới, thiết lập giới hạn chỗ ngồi và phát hành sự kiện lên hệ thống.

**Hướng dẫn chi tiết:**
1. **Đăng nhập:** Truy cập hệ thống với tài khoản Organizer.
2. **Mở Dashboard:** Chọn mục **Event Management** trên thanh điều hướng.
3. **Tạo sự kiện:** Nhấn **Create New Event**.
4. **Nhập thông tin:**
   - Điền Tên sự kiện, Mô tả, Thời gian.
   - Chọn Địa điểm (Venue) (hệ thống tự động kiểm tra sức chứa theo FE-05).
   - Thêm danh mục (Category) và Tags.
5. **Phát hành:** Nhấn **Submit**. Sự kiện chuyển sang trạng thái "Published".

### 3.3 Workflow 2: Tìm kiếm và Đặt vé (Attendee)

**Mục đích:**
Cho phép người tham dự tìm kiếm sự kiện phù hợp và đặt vé theo thời gian thực với xử lý chống trùng lặp dữ liệu (Concurrency).

**Hướng dẫn chi tiết:**
1. **Tìm sự kiện:** Sử dụng thanh tìm kiếm và bộ lọc trên trang chủ (FE-03) để tìm sự kiện theo tên, thẻ hoặc thời gian.
2. **Xem chi tiết:** Nhấn vào thẻ sự kiện để xem thông tin chi tiết và số lượng vé còn trống.
3. **Tiến hành đặt vé:**
   - Chọn số lượng vé cần mua.
   - Nhấn **Book Now**.
   - *Lưu ý:* Hệ thống áp dụng cơ chế Live Ticket Booking (FE-04), giao dịch có thể bị từ chối nếu vé vừa được người khác mua (thông báo lỗi DbUpdateConcurrencyException sẽ được xử lý an toàn).
4. **Xác nhận:** Kiểm tra email để nhận thông báo xác nhận và nhắc nhở tự động (FE-06).
