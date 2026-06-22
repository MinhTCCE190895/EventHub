# BÁO CÁO: CÁC ĐIỂM BẤT HỢP LÝ VỀ LOGIC & ĐIỀU HƯỚNG HỆ THỐNG - UNIEVENT HUB

Tài liệu này tổng hợp toàn bộ các điểm bất hợp lý về mặt luồng hoạt động (logic flows) và thiết kế điều hướng (navigation) giữa các phân hệ của dự án **EventHub** (MVC Identity, Razor Pages Explore & CRUD, Blazor Server Live Systems) cùng sự phân chia nhiệm vụ xử lý cho từng thành viên.

---

## 📋 PHÂN CHIA TRÁCH NHIỆM CHO THÀNH VIÊN (TASK DIVISION)

| Thành viên | Nhiệm vụ đảm nhận | Các điểm bất hợp lý cần xử lý |
|---|---|---|
| 👑 **LongNH** | Identity, Auth & Admin | **Mục 4** (Chuyển hướng chưa đăng nhập Blazor về MVC), **Mục 7** (Quy hoạch Đăng xuất về một mối), **Mục 5** (Kết nối Admin Tools MVC & Razor Pages). |
| 👨‍💻 **MinhTC** | Event CRUD & Requests | **Mục 5** (Hỗ trợ LongNH liên kết các trang quản trị Razor Pages CRUD sang MVC Admin Panel). |
| ⚙️ **QuiNC** | Explore & Weather Widget | **Mục 1** (Xóa trùng lặp trang danh sách Blazor `/events`), **Mục 2** (Sửa Sidebar Blazor chỉ hướng về Explore của Razor Pages). |
| ⚡ **Khôi** | Live Booking & Dashboard | **Mục 6** (Bổ sung nút điều hướng thoát "điểm cụt" sau đặt vé thành công), **Mục 8** (Nhận SignalR tự động cập nhật Dashboard khi sự kiện được duyệt). |
| 🕰️ **TriLT** | Emails & Feedback Analytics | **Mục 8** (Tích hợp kích hoạt gửi tin SignalR broadcast từ Razor Pages/BLL khi phê duyệt sự kiện). |

---

## 1. ⚙️ QuiNC — Explore & Weather Widget

### 1.1. Trùng Lặp Trang Danh Sách Sự Kiện (Explore / Browse Duplication)
* **Mô tả hiện trạng**: Trang `Explore` chính nằm trên Razor Pages (`5129`), nhưng Blazor (`5210`) cũng có trang `/events` hiển thị danh sách đơn giản.
* **Giải pháp**: Xóa danh sách trùng lặp ở `Home.razor` Blazor, cấu hình tự động chuyển hướng về `http://localhost:5129/`.

### 1.2. Luồng Đặt Vé Bị Đứt Gãy (Sidebar Redirect Link)
* **Mô tả hiện trạng**: Link **"Đặt vé sự kiện (Live)"** ở Sidebar Blazor trỏ về `/events` của Blazor thay vì Explore chính của Razor Pages.
* **Giải pháp**: Đổi link Sidebar Blazor sang `http://localhost:5129/`.

---

## 2. 👑 LongNH & 👨‍💻 MinhTC — Identity, Auth & Admin Panel

### 2.1. Đồng bộ Bảo mật khi chưa Đăng nhập (Unauthorized Redirect)
* **Mô tả hiện trạng**: Blazor khi chưa đăng nhập gõ thẳng URL không tự đá về MVC Login cổng `5259`.
* **Giải pháp**: Cấu hình cơ chế chuyển hướng Authentication trong Blazor trỏ về MVC.

### 2.2. Quy hoạch một cổng Đăng xuất duy nhất (SSO Logout)
* **Mô tả hiện trạng**: Razor Pages tự định nghĩa Logout cục bộ, Blazor gọi MVC Logout.
* **Giải pháp**: Hướng tất cả các nút đăng xuất của hệ thống về cổng duy nhất `/Account/Logout` của MVC.

### 2.3. Phân mảnh Giao diện Quản trị (Admin Panel Split)
* **Mô tả hiện trạng**: Admin quản lý User ở MVC, quản lý sự kiện/danh mục ở Razor Pages nhưng không có link chuyển đổi chéo trên Sidebar.
* **Giải pháp**: Bổ sung liên kết chéo trên Sidebar của 2 phân hệ giúp Admin chuyển đổi mượt mà.

---

## 3. ⚡ Khôi & 🕰️ TriLT — Real-time & SignalR Hubs

### 3.1. Điểm cụt sau khi Đặt vé thành công (Post-Booking Dead-End)
* **Mô tả hiện trạng**: Đặt vé hoặc feedback xong không có nút điều hướng tiếp theo.
* **Giải pháp** (Khôi): Thêm nút "Quay lại chi tiết sự kiện" (trỏ về Razor Pages `Detail`) hoặc "Tiếp tục khám phá" tại màn hình thông báo của Blazor.

### 3.2. Không đồng bộ thời gian thực khi Phê duyệt sự kiện (Event Approval Sync)
* **Mô tả hiện trạng**: Admin duyệt sự kiện ở Razor Pages nhưng Live Dashboard của Blazor không tự cập nhật nếu không F5.
* **Giải pháp**:
  * **TriLT**: Gửi SignalR broadcast khi duyệt sự kiện từ BLL/Razor Pages.
  * **Khôi**: Blazor Dashboard lắng nghe sự kiện để tự động nạp sự kiện mới vào danh sách hiển thị.
