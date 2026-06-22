# BÁO CÁO TOÀN DIỆN: CÁC KỊCH BẢN LỖI LOGIC & EDGE CASES - UNIEVENT HUB

Tài liệu này tổng hợp toàn bộ các lỗ hổng logic, lỗi đồng bộ giữa các phân hệ (MVC, RazorPages, Blazor Server) và phân chia trách nhiệm xử lý cho từng thành viên dự án dựa trên chức năng đảm nhiệm.

---

## 📋 PHÂN CHIA TRÁCH NHIỆM CHO THÀNH VIÊN (TASK DIVISION)

| Thành viên | Nhiệm vụ đảm nhận | Các Edge Cases cần xử lý |
|---|---|---|
| 👑 **LongNH** | Identity, Auth & Admin (`FE-01`, `FE-09`) | Lệch phiên SSO, OnValidatePrincipal (Force Logout), cấu hình Domain Cookie. |
| 👨‍💻 **MinhTC** | Event CRUD, Venues, Approvals (`FE-02`, `FE-05`, `FE-13`) | Over-posting khi duyệt sự kiện, bắt FK Exception khi xóa thực thể, chuẩn hóa Venue Campus Address, chống trùng lặp Tag/Category. |
| ⚙️ **QuiNC** | Explore, Search, Weather (`FE-03`, `FE-08`, `FE-11`) | NormalizeLocation crash thời tiết, Cache Stampede, AJAX Debounce tìm kiếm, Client-side Clock Sync khi đếm ngược. |
| ⚡ **Khôi** | Live Booking, Dashboards, Q&A (`FE-04`, `FE-10`, `FE-15`) | Race Condition đặt vé (RowVersion), đặt nhiều tab lách luật, Blazor Disconnect Spinner, Rate Limiting phía Server-side của SignalR. |
| 👑 **TriLT** | Emails, Feedbacks, Analytics (`FE-06`, `FE-07`, `FE-12`) | Gửi trùng email nhắc nhở (Multi-instance), lỗi tràn bộ nhớ (Out of Memory) PLINQ Feedback. |

---

## 1. 👑 LongNH — Identity, Auth & Admin

### 1.1. Vấn đề Force Logout chậm (OnValidatePrincipal)
* **Mô tả**: Khi Admin khóa tài khoản (`IsActive = false`), người dùng vẫn hoạt động do cookie `.EventHub.Auth` chưa hết hạn.
* **Giải pháp**: Viết `CookieAuthenticationEvents.OnValidatePrincipal` kiểm tra trạng thái hoạt động trong DB mỗi 5 phút.

### 1.2. Vấn đề SSO Cookie trên môi trường thực tế (Subdomain Support)
* **Mô tả**: Cookie không tự chia sẻ giữa các subdomain (ví dụ: `auth.unievent.edu.vn` và `booking.unievent.edu.vn`).
* **Giải pháp**: Thiết lập `options.Cookie.Domain = ".unievent.edu.vn"`.

---

## 2. 👨‍💻 MinhTC — Event CRUD & Venue Limits

### 2.1. Ngăn chặn Over-posting khi duyệt ý tưởng (`FE-13`)
* **Mô tả**: Người dùng sửa HTML gửi thêm trường `IsApproved = true` tự duyệt sự kiện.
* **Giải pháp**: Sử dụng `EventUpdateDTO` loại bỏ các trường hệ thống, không bind trực tiếp Model vào Entity Database.

### 2.2. Lỗi Crash hệ thống khi xóa Venue/Category đang được sử dụng (`FE-05`)
* **Mô tả**: Ràng buộc khóa ngoại DB trả về Exception màn hình vàng ASP.NET.
* **Giải pháp**: Bọc lệnh xóa trong khối `try-catch` tại PageModel, hiển thị thông báo lỗi thân thiện thay vì crash.

### 2.3. Trùng lặp dữ liệu do khoảng trắng và ký tự (Tag/Category Duplication)
* **Mô tả**: Tạo các tag giống nhau nhưng thừa khoảng trắng ở cuối (Ví dụ: `"Công Nghệ "` và `"Công Nghệ"`).
* **Giải pháp**: Gọi `.Trim()` và `.ToLower()` trước khi lưu, thêm Unique Index vào DB.

---

## 3. ⚙️ QuiNC — Search & Weather Widget

### 3.1. Lỗi phân tích vị trí thời tiết (`FE-08`)
* **Mô tả**: Địa chỉ Venue không có hậu tố `, [Campus Name]` khiến hàm `NormalizeLocation` cắt chuỗi bị lỗi index vỡ trang chi tiết.
* **Giải pháp**: Thêm try-catch và kiểm tra định dạng địa chỉ, gán giá trị mặc định nếu không khớp.

### 3.2. Chống nghẽn API thời tiết (Cache Stampede)
* **Mô tả**: Khi cache hết hạn, hàng trăm request đồng thời gọi tới `wttr.in/` gây nghẽn luồng.
* **Giải pháp**: Sử dụng cơ chế khóa Semaphore/Lock khi cache bị miss (Double-checked locking).

### 3.3. Spam nút tìm kiếm (AJAX Search Debounce)
* **Mô tả**: Người dùng spam click nút lọc khiến hàng chục request AJAX chạy ngầm song song.
* **Giải pháp**: Thêm hàm Debounce trì hoãn request tìm kiếm 300ms trong JS Explore.

---

## 4. ⚡ Khôi — Real-time Master (Blazor & SignalR)

### 4.1. Race Condition đặt vé cuối cùng (Overbooking - `FE-04`)
* **Mô tả**: Hai sinh viên cùng lúc đặt 1 vé cuối cùng, DB ghi nhận thành công cả hai làm vượt quá sức chứa.
* **Giải pháp**: Bắt lỗi `DbUpdateConcurrencyException` thông qua cấu hình `[Timestamp] RowVersion` trong Entity `Event`.

### 4.2. Đặt vé lách luật bằng Multi-tab
* **Mô tả**: Sinh viên mở 2 tab đặt 2 ghế khác nhau cùng lúc để bỏ qua giới hạn "mỗi người tối đa 1 vé".
* **Giải pháp**: Thực hiện check số lượng vé hiện tại của User trong Transaction có khóa (Serializable hoặc isolation thích hợp).

### 4.3. Treo giao diện khi mất kết nối mạng
* **Mô tả**: Đứt kết nối WebSocket giữa chừng làm vòng xoay loading quay mãi mãi.
* **Giải pháp**: Bổ sung cơ chế Timeout và lắng nghe sự kiện ngắt kết nối Blazor để hiển thị thông báo kết nối lại.

### 4.4. Rate Limiting trên SignalR Hub (`FE-15`)
* **Mô tả**: Kẻ xấu spam trực tiếp gói tin WebSocket để đẩy comment rác.
* **Giải pháp**: Tích hợp kiểm tra giới hạn tần suất gửi (Rate Limiting) trên Server-side của Hub.

---

## 5. 🕰️ TriLT — Background & Analytics

### 5.1. Gửi trùng Email nhắc nhở (`FE-06`)
* **Mô tả**: Chạy nhiều instance app/IIS worker khiến mỗi instance tự quét DB gửi trùng lặp mail cho 1 sinh viên.
* **Giải pháp**: Sử dụng Row-locking hoặc Transaction cập nhật trạng thái `ReminderSent` trước khi gửi.

### 5.2. Tràn bộ nhớ RAM máy chủ do PLINQ (`FE-07`)
* **Mô tả**: Gọi `.AsParallel()` trên RAM cho hàng trăm ngàn bản ghi feedback thay vì tính toán trực tiếp bằng SQL Aggregates ở DB.
* **Giải pháp**: Chuyển đổi công thức tính trung bình điểm thành câu lệnh LINQ SQL `GroupBy` để Database xử lý.
