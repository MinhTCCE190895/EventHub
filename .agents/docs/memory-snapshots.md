# UniEvent Hub - Memory Snapshots & Project Status

This file tracks the current state of **UniEvent Hub**. All AI agents **must** read and update this file before and after working to maintain project alignment and traceability.

```mermaid
graph TD
    classDef completed fill:#dbeafe,stroke:#2563eb,stroke-width:2px,color:#1e3a8a;
    classDef pending fill:#fef3c7,stroke:#d97706,stroke-width:2px,color:#78350f;

    RP[RazorPages Project]:::completed
    MVC[MVC Project]:::completed
    BZ[Blazor Project]:::pending

    RP --> RP_Explore["Explore, Bookmarks, Weather"]:::completed
    MVC --> MVC_Auth["Identity & Authorization"]:::completed
    BZ --> BZ_Dash["Booking & Dashboard"]:::completed
    BZ --> BZ_Feed["Feedback Analytics"]:::pending
```

---

## 1. COMPLETED MODULES

### 1.1. Architecture & Global Config
- **Environment**: Upgraded 100% to **.NET 8** and **C# 12**.
- **Shared Authentication**: SSO implemented via shared Cookie `.EventHub.Auth` using EF Core Data Protection (`Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`) across MVC, RazorPages, and Blazor.

### 1.2. Razor Pages (Explore, Bookmarks & Event CRUD)
- **Explore & Search (`Pages/Index.cshtml` / `FE-03`)**: Form search (.search-card-minimal) with time filters (All, Upcoming, Ongoing, Past). Dynamic LED status dots, seats capacity urgency warnings, grayscale for past events, skeleton loader, and AJAX pagination.
- **Bookmarks (`FE-11`)**: Floating bookmark button (`.btn-bookmark-floating`) on events card (disabled for current sprint).
- **Weather Widget (`FE-08`)**: Integrated wttr.in weather lookup with 30-minute `IMemoryCache`. `NormalizeLocation` automatically extracts the campus city name from Venue address.
- **Event CRUD (`FE-02`)**: Complete Event CRUD. Uses DTOs (`EventCreateDTO`, `EventUpdateDTO`) to prevent over-posting. Categories and Tags assigned via checkboxes (Many-to-Many). Serviced via `IEventRepository.BuildSearchQuery()` with eager loading to prevent N+1 queries.
- **My Feedbacks (`/Events/MyFeedbacks`)**: Read-only dashboard for students to review submitted feedbacks.

### 1.3. Blazor Server (Live Systems)
- **Live Ticket Booking (`FE-04`)**: Real-time seat reservation. Integrated with `AuthenticationStateProvider` to retrieve authentic User IDs from the Identity cookie. Features premium Glassmorphism and real-time seat progress bar.
- **Live Dashboard (`FE-10`)**: Displays dynamic real-time seats progress connected via SignalR `ReceiveTicketUpdate`. Features SVG Area Trend Charts, pulse-green status badges, and a live activity log.
- **Feedback Analytics (`FE-07`)**: multi-criteria feedback statistics dashboard using PLINQ `.AsParallel()` on BLL. Includes detailed feedback list pop-up modal.

---

## 2. PENDING MODULES
- **Blazor (Feedback Analytics - Detail Reports)**: Further custom analytics reports if requested.

---

## 3. WORK LOG & TRACEABILITY

### 3.1. Detailed Changes Log

- **2026-06-23 (Antigravity)**:
  - **LongNH - Auth & SSO Fixes (`FE-01`)**: Triển khai `CookieAuthenticationEvents.OnValidatePrincipal` kiểm tra trạng thái hoạt động trong DB ở cả 3 project (MVC, RazorPages, Blazor) nhằm Force Logout tức thì khi Admin khóa tài khoản (`IsActive = false`). Đồng thời bổ sung `options.Cookie.Domain = ".unievent.edu.vn"` để chia sẻ cookie giữa các subdomain, xử lý triệt để kịch bản lỗi SSO.
  - **Thời tiết sự kiện & Dự báo (Event Weather Forecast - FE-08)**: Bổ sung method `GetWeatherForecastAsync` vào `WeatherService` để hỗ trợ dự báo thời tiết cho ngày diễn ra sự kiện. Thực hiện kiểm tra ngày diễn ra sự kiện theo giờ Việt Nam (UTC+7): nếu sự kiện nằm trong khoảng 3 ngày (hôm nay, ngày mai, ngày kia) sẽ truy vấn dữ liệu dự báo ngày từ API `wttr.in`; nếu ngoài 3 ngày sẽ hiển thị khung chờ mờ nhẹ (Glassmorphism skeleton) với thông điệp *"Dự báo thời tiết sẽ khả dụng trước ngày diễn ra sự kiện 3 ngày"* (Phương án A). Tích hợp hiển thị Widget dự báo này vào trang chi tiết sự kiện `Detail.cshtml`. Đồng thời, thiết kế lại toàn bộ trang `Detail.cshtml` sang dạng báo cáo học thuật tối giản (Split-Screen Report với `.whitepaper-sheet` và `.glass-action-panel`), loại bỏ các box card đơn lẻ, icon ở tiêu đề phần, và đồng bộ hiển thị danh mục/thẻ dạng văn bản báo cáo khoa học.
  - **MinhTC - Tách nghiệp vụ Duyệt Sự Kiện (Admin Approve)**: Bổ sung logic duyệt độc lập `ChangeEventStatusAsync` trong `EventService`, tạo luồng POST API chuyên biệt `?handler=ChangeStatus` trong giao diện List Event `Index.cshtml`. Giới hạn truy cập (RBAC) với `if (!User.IsInRole("Admin"))` để chặn Organizer tự duyệt. Tích hợp trực tiếp các nút Duyệt/Hủy vào Data Grid dành riêng cho role Admin, ngăn chặn triệt để lỗi Over-posting trạng thái từ Form Edit cũ.

  - **MinhTC - Fix Logic Anomalies (FE-05, FE-13)**: Khắc phục các lỗi logic cho phân hệ Event CRUD: Xóa trường `Status` khỏi tính năng Edit Event (Chống Over-posting), xử lý `DbUpdateException` khi xóa `Venue`, `Category`, `Tag` đang được sử dụng (thông báo lỗi thay vì crash), cập nhật `CategoryService` và `TagService` tự động `.Trim()` và kiểm tra trùng lặp tên. Thêm logic xác thực sức chứa (Capacity Limits) khi Cập nhật Địa điểm (Venue) và Cập nhật Sự kiện (Event) để đảm bảo Sức chứa mới không được nhỏ hơn số lượng đã đăng ký hiện tại, hiển thị lỗi qua `ModelState`.
- **2026-06-22 (Antigravity)**:
  - **Tài liệu hóa Edge Cases & Phân chia lỗi logic**: Biên soạn tài liệu phân tích chi tiết các kịch bản lỗi logic, UX edge cases và phân chia cụ thể cho các thành viên trong nhóm phục vụ giai đoạn kiểm thử và hoàn thiện.
  - **QuiNC - Fix Weather & AJAX Search Edge Cases (`FE-08`, `FE-03`)**: Sửa lỗi `NormalizeLocation` tránh crash khi đầu vào địa chỉ thiếu `, [Campus Name]`; giải quyết triệt để Cache Stampede bằng `SemaphoreSlim` (Double-checked locking pattern) trong `WeatherService.cs`; và tích hợp `AbortController` hủy các AJAX request tìm kiếm thừa khi người dùng spam click nhanh trên trang Explore (`Index.cshtml`).
  - **QuiNC - Tối ưu luồng điều hướng Blazor (`FE-03`)**: Tối ưu hóa trải nghiệm Student bằng cách chuyển hướng tự động trang `/events` bên Blazor (`Home.razor`) và cập nhật liên kết Sidebar (`NavMenu.razor`) trỏ trực tiếp về trang Explore chính của Razor Pages (`http://localhost:5129/`) để tránh trùng lặp giao diện xem danh sách sự kiện.
  - **QuiNC - Explore AutoMapper Conversion (`FE-03`)**: Chuyển đổi thành công phần map dữ liệu thủ công (`.Select` tay) trong BLL `SearchService.cs` sang sử dụng **AutoMapper** tự động. Cập nhật cấu hình map tương ứng trong `EventProfile.cs` (gồm lấy VenueName, OrganizerName, danh sách TagNames từ bảng liên kết, số lượng vé đã đặt `BookedCount` và sức chứa tối đa `MaxCapacity`).
  - **Đồng bộ múi giờ Việt Nam (UTC+7)**: Cấu hình AutoMapper trong `EventProfile.cs` tự động cộng thêm 7 tiếng khi chuyển đổi từ Entity lên DTO (`EventDTO`, `EventCardDTO`, `EventUpdateDTO`), và tự động trừ đi 7 tiếng khi map từ DTO tạo mới/cập nhật xuống Database để dữ liệu lưu trữ vẫn chuẩn UTC nhưng giao diện hiển thị đúng giờ Việt Nam.
  - **Cấu hình Connection String**: Chuyển đổi chuỗi kết nối `"EventHub"` trong cả 3 dự án (`MVC`, `RazorPages`, `Blazor`) từ LocalDB/Server cũ sang Server SQL Developer local mặc định (`Server=.`) theo yêu cầu của anh QuiNC để chạy mượt mà trên máy của anh.
  - **Tối ưu hóa launchBrowser**: Cập nhật file `launchSettings.json` của 3 dự án, tắt tự động mở trình duyệt ở RazorPages và Blazor (đặt thành `false`), chỉ để `true` ở dự án MVC để khi khởi động chỉ mở duy nhất tab đăng nhập của MVC, hạn chế rác tab trình duyệt.
  - **Đồng nhất Giao diện & Layout**: Loại bỏ các thẻ bao bọc `.app-container` và `.app-content` dư thừa trong `Home.razor` và `BookingComponent.razor` để giao diện Blazor tích hợp đồng nhất với thanh điều hướng (sidebar) toàn hệ thống giống như bên RazorPages/MVC.
- **2026-06-21 (Antigravity)**:
  - **TriLT - Feedback (`FE-07`) & Email Worker (`FE-06`)**:
    - Created detailed feedback modal in Blazor `FeedbackAnalyticsComponent.razor`.
    - Created student's read-only feedback history `MyFeedbacks` page in Razor Pages.
    - Optimized feedback average calculations using PLINQ `.AsParallel()` in `FeedbackAnalyticsService`.
    - Upgraded `EventReminderService` (BLL) to dispatch emails concurrently using TPL `Parallel.ForEachAsync`.
    - Fixed LINQ translation issue in `EventReminderRepository` (DAL) by utilizing SQL-translatable time filter. Added `IsCanReminder` tracking flag in DB.
  - **MinhTC - Event CRUD (`FE-02`)**:
    - Removed CRUD Organizer (deleted folder `/Pages/Organizers`, services, and DTOs).
    - Built Razor Pages Event CRUD with dynamic checkboxes for Category & Tag updates (Many-to-Many). Protected queries using `EventCreateDTO` and `EventUpdateDTO`.
  - **Access Control & Logouts**:
    - Unified Sidebar Layout across MVC, RazorPages, and Blazor to restrict Student role permissions.
    - Created local RazorPages `Logout.cshtml` page to support local cookie invalidation.

- **2026-06-20 (Antigravity / MinhTC)**:
  - Reconfigured Blazor routing: Live Dashboard to `/` (default) and event bookings to `/events`.
  - **MinhTC - CRUD Category, Tag, Venue (`FE-05`)**:
    - Created CRUD Razor Pages and DTOs (`CategoryDTO`, `TagDTO`, `VenueDTO`).
    - Appended chosen Campus to Venue Address suffix in `VenueCreateDTO`/`VenueUpdateDTO` for weather lookup.

- **2026-06-17 (Antigravity)**:
  - Resolved Blazor Server WebSocket disconnection issue by downgrading `Microsoft.AspNetCore.SignalR.Client` from 9.0 to 8.0.* to match SDK .NET 8.
  - Added test automation script `test.js` using Puppeteer.
  - Designed premium Dark Mode Deep Tech layout for Live Dashboard, featuring SVG Area Chart.
  - Rewrote `BookingComponent.razor` using Cascading Authentication State for real claims.

- **2026-06-17 (quinc)**:
  - Removed Bookmark UI and deleted `BookmarkService.cs` DI configurations to match backlog.

- **2026-06-16 (Antigravity / LongNH)**:
  - **LongNH - Identity & SSO (`FE-01`)**:
    - Configured Cookie Authentication, AccountController, and BCrypt password hashing.
    - Set up shared EF Core Data Protection keys in BLL/MVC to enable SSO.
  - Integrated wttr.in weather API with 30-min `IMemoryCache` for Weather Widget.

- **2026-06-14 (Antigravity / MinhTC / QuiNC)**:
  - Setup core PRN222 architectural rules (3-Layer structure, connection safety, async/await).
  - **MinhTC**: Created Organizer CRUD Razor Pages.
  - **QuiNC**: Added status dots and urgencies to Explore list UI (`FE-03`).

### 3.2. Team Branch Status
- **`feature/trilt-email-worker`**: Local development for email background worker.
- **`feature/trilt-feedback`**: Local development for feedback reports.
- **`feature/trilt-follows`**: Local development for event follows.

---

## 4. CROSS-MODULE CONTRACTS

### 4.1. Venue Address & Weather Widget (MinhTC - FE-02/FE-05)
- Forms creating/editing Venues **must** provide a Campus selection dropdown. The selected value must be appended to the end of `Venue.Address` as `, [Campus Name]` to allow `FE-08 (Weather Widget)` to correctly parse and fetch weather data.

### 4.2. Navigation Flow & Authorization Routing (Global SSO)
- **SSO Authentication Cookie**: MVC, RazorPages, and Blazor share the cookie `.EventHub.Auth` using EF Core Data Protection.
- **Post-Login Redirection**: Following successful authentication, all roles (including `Student`) must be redirected to the Razor Pages application (`http://localhost:5129/`) to preserve a unified entry point.
- **Single Tab Navigation Flow**: Avoid opening Blazor modules (Dashboard or Ticket Booking) in new browser tabs. All links navigating between Razor Pages and Blazor must load on the same tab (no `target="_blank"`), utilizing relative paths or direct port mappings to prevent session breakages.
- **Role-based Sidebar Visibility**: Sidebar links for CRUD management must be strictly hidden from `Student` users and authorized only for `Admin` and `Organizer` roles across all three presentation apps.

