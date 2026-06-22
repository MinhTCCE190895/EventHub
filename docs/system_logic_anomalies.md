# BÁO CÁO: CÁC ĐIỂM BẤT HỢP LÝ VỀ LOGIC & ĐIỀU HƯỚNG HỆ THỐNG - UNIEVENT HUB

Tài liệu này tổng hợp toàn bộ các điểm bất hợp lý về mặt luồng hoạt động (logic flows) và thiết kế điều hướng (navigation) giữa các phân hệ của dự án **EventHub** (MVC Identity, Razor Pages Explore & CRUD, Blazor Server Live Systems).

---

## 1. Trùng Lặp Trang Danh Sách Sự Kiện (Explore / Browse Duplication)
* **Mô tả hiện trạng**:
  * **Razor Pages (Cổng 5129 - `Pages/Index.cshtml`)**: Trang chủ hiển thị danh sách sự kiện đầy đủ kèm theo AJAX Search & Filter, phân trang, lọc theo thời gian (Upcoming, Ongoing, Past), lọc nhanh theo Tags và tích hợp Widget thời tiết.
  * **Blazor Server (Cổng 5210 - `Home.razor` / `/events`)**: Cũng hiển thị một danh sách sự kiện đơn giản, chỉ lấy các sự kiện sắp diễn ra mà không có bất kỳ bộ lọc hay tìm kiếm nào.
* **Điểm bất hợp lý**:
  * Phá vỡ nguyên tắc **Single Source of Truth** (Một nguồn sự thật duy nhất). Người dùng cùng vai trò Student nhưng có tới 2 trang khác nhau để xem danh sách sự kiện.
  * Sinh viên dễ bị bối rối không biết nên dùng trang nào để tìm kiếm sự kiện chuẩn xác nhất.
* **Đề xuất khắc phục**:
  * Xóa bỏ danh sách sự kiện trùng lặp ở trang `Home.razor` của Blazor.
  * Cấu hình trang `Home.razor` của Blazor tự động chuyển hướng (`Navigation.NavigateTo`) về trang Explore chính của Razor Pages (`http://localhost:5129/`).

---

## 2. Luồng Điều Hướng Đặt Vé Bị Đứt Gãy (Broken Booking Navigation Flow)
* **Mô tả hiện trạng**:
  * Sinh viên xem chi tiết sự kiện ở Razor Pages (`5129/Events/Detail/{id}`).
  * Khi click "Đăng Ký Đặt Vé Ngay", hệ thống chuyển hướng sang Blazor Booking (`5210/events/{id}/book`).
  * Trong Sidebar của Blazor, link **"Đặt vé sự kiện (Live)"** lại trỏ về `/events` (trang danh sách sự kiện Blazor trùng lặp).
* **Điểm bất hợp lý**:
  * Nếu sinh viên muốn quay lại tìm sự kiện khác, họ click **"Khám phá sự kiện"** thì bay về Razor Pages (`5129`), nhưng click **"Đặt vé sự kiện (Live)"** thì lại ra trang `/events` của Blazor (`5210`).
  * Hai hành động có mục tiêu giống nhau nhưng lại dẫn ra hai trang ở hai cổng khác nhau, làm đứt gãy luồng trải nghiệm.
* **Đề xuất khắc phục**:
  * Thay đổi liên kết **"Đặt vé sự kiện (Live)"** trên Sidebar của Blazor trỏ trực tiếp về trang Explore của Razor Pages (`http://localhost:5129/`).

---

## 3. Lệch Múi Giờ Hiển Thị Giữa Các Nền Tảng (Timezone Discrepancy)
* **Mô tả hiện trạng**:
  * **Razor Pages**: Thời gian hiển thị của sự kiện đã được cấu hình qua AutoMapper để cộng thêm 7 tiếng (sang múi giờ Việt Nam UTC+7) trước khi hiển thị.
  * **Blazor Server (`Home.razor`)**: Vẫn so sánh trực tiếp với `DateTime.UtcNow` và hiển thị chuỗi giờ gốc chưa chuyển đổi múi giờ.
* **Điểm bất hợp lý**:
  * Một sự kiện có thể đã kết thúc ở Razor Pages nhưng bên Blazor vẫn hiển thị trạng thái đang diễn ra hoặc sắp diễn ra, tạo ra sự mâu thuẫn dữ liệu nghiêm trọng.
* **Đề xuất khắc phục**:
  * Áp dụng quy chuẩn đồng bộ cộng thêm 7 tiếng cho tất cả các hiển thị thời gian trên giao diện Blazor, hoặc chuyển đổi múi giờ thống nhất từ tầng Service/BLL.

---

## 4. Thiếu Đồng Bộ Chuyển Hướng Chưa Đăng Nhập (Unauthorized Redirect)
* **Mô tả hiện trạng**:
  * **Razor Pages**: Khi chưa đăng nhập, nếu truy cập vào trang con sẽ tự động chuyển hướng về trang đăng nhập MVC (`5259/Account/Login`).
  * **Blazor Server**: Khi chưa đăng nhập, nếu người dùng gõ thẳng link (như `/feedback-analytics` hoặc `/dashboard`), Blazor chưa có cơ chế tự động chuyển hướng về MVC Login một cách thống nhất.
* **Điểm bất hợp lý**:
  * Trải nghiệm bảo mật không đồng bộ. Người dùng có thể thấy màn hình Blazor trống hoặc thông báo lỗi kỹ thuật thay vì giao diện đăng nhập thân thiện.
* **Đề xuất khắc phục**:
  * Cấu hình sự kiện chuyển hướng đăng nhập thống nhất trong tệp `ServiceExtensions.cs` của Blazor trỏ về trang Login của MVC.

---

## 5. Sự Phân Mảnh Giao Diện Quản Trị Của Admin/Organizer (Scattered Admin Tools)
* **Mô tả hiện trạng**:
  * **MVC (`cổng 5259`)**: Chứa trang **Admin Control Panel (`FE-09`)** quản lý User (khóa, kích hoạt tài khoản).
  * **Razor Pages (`cổng 5129`)**: Chứa các trang quản lý CRUD danh mục, thẻ, địa điểm và sự kiện.
* **Điểm bất hợp lý**:
  * Admin muốn thực hiện toàn bộ công việc quản trị hệ thống phải nhảy qua lại giữa 2 cổng khác nhau.
  * Giao diện Sidebar của 2 dự án chưa hề có các liên kết chuyển đổi nhanh giữa hai trang quản trị này.
* **Đề xuất khắc phục**:
  * Thêm liên kết "Quản lý tài khoản" trên Sidebar của Razor Pages trỏ về MVC Admin Panel, và ngược lại thêm các link CRUD dữ liệu trên Sidebar của MVC trỏ về Razor Pages.

---

## 6. Điểm Cụt Sau Khi Giao Dịch Thành Công (Post-Booking Dead-End)
* **Mô tả hiện trạng**:
  * Sinh viên hoàn thành đặt vé thành công hoặc gửi đánh giá phản hồi trên hệ thống Blazor.
* **Điểm bất hợp lý**:
  * Giao diện hiển thị thông báo thành công nhưng không có nút điều hướng tiếp theo (như "Quay lại trang chi tiết sự kiện" hay "Xem các sự kiện khác"). Người dùng bị rơi vào trạng thái "cụt" luồng trải nghiệm, bắt buộc tự điều hướng thủ công.
* **Đề xuất khắc phục**:
  * Bổ sung nút bấm điều hướng rõ ràng sau khi đặt vé thành công: *"Quay lại trang chi tiết"* (trỏ về Razor Pages `Detail` của sự kiện đó) hoặc *"Tiếp tục khám phá"* (trỏ về trang chủ Razor Pages).

---

## 7. Phân Tán Đầu Mối Đăng Xuất (Scattered Logout Actions)
* **Mô tả hiện trạng**:
  * **Razor Pages** có trang `/Logout` riêng, **Blazor Sidebar** nút đăng xuất lại gọi trực tiếp link của **MVC (`5259/Account/Logout`)**.
* **Điểm bất hợp lý**:
  * Việc đăng xuất xử lý cookie ở nhiều đầu mối dễ dẫn tới lệch session nếu cơ chế chia sẻ cookie qua EF Core Data Protection gặp độ trễ.
* **Đề xuất khắc phục**:
  * Quy hoạch toàn bộ hành động Đăng xuất (Logout) trên tất cả các Sidebar của Razor Pages và Blazor trỏ về một endpoint duy nhất là `/Account/Logout` của dự án MVC.

---

## 8. Đồng Bộ Thời Gian Thực Khi Phê Duyệt Sự Kiện (Event Approval Live Hub Sync)
* **Mô tả hiện trạng**:
  * Admin phê duyệt một ý tưởng sự kiện từ `Pending` sang `Approved` ở trang Razor Pages (`5129/Requests`).
* **Điểm bất hợp lý**:
  * Sự kiện được kích hoạt trong DB nhưng danh sách đặt vé trực tuyến của Blazor và Live Dashboard không hề tự động cập nhật. Người dùng đang mở Dashboard bắt buộc phải tải lại trang thủ công thì mới thấy sự kiện mới xuất hiện.
* **Đề xuất khắc phục**:
  * Gửi một thông điệp SignalR broadcast (từ Razor Pages thông qua SignalR Client hoặc API) sang Blazor Hub để cập nhật danh sách sự kiện theo thời gian thực cho tất cả người dùng đang kết nối.
