# Luật Thiết kế Giao diện UI/UX (UI/UX Design Rules)

## 1. Đồng nhất UI giữa các Phân hệ (UI Consistency)
- **Phân hệ Blazor (Registration Dashboard & Feedback Analytics):**
  - Sử dụng thư viện UI **MudBlazor** làm nền tảng cho việc phát triển component, dashboard, và các biểu đồ phân tích.
- **Phân hệ MVC (Identity) & Razor Pages (Location & Event):**
  - Sử dụng framework CSS mặc định kèm theo các template mẫu của ASP.NET Core (thông thường là Bootstrap được tích hợp sẵn).

## 2. Quy chuẩn Thiết kế Wireframe & Giới hạn CSS (Design System Guidelines)
- **Tình trạng Thiết kế:** Phong cách thiết kế UI/UX cuối cùng của dự án **CHƯA ĐƯỢC CHỐT**.
- **Nguyên tắc Dựng UI Giai đoạn Đầu:**
  - Agent chỉ được phép xây dựng giao diện ở mức **wireframe** hoặc layout cơ bản thô sơ.
  - Giữ mã nguồn UI/HTML/CSS sạch sẽ, nguyên bản, gọn gàng và dễ mở rộng.
  - **TUYỆT ĐỐI không** tự ý thêm các hiệu ứng CSS phức tạp, hoạt ảnh (animations) hoặc các custom style kỳ lạ cho đến khi nhận được hướng dẫn thiết kế chính thức từ User.
- **Luật "Good Taste" (Thẩm mỹ & Gu thiết kế):**
  - Khi bắt đầu có lệnh thiết kế giao diện **Kinetic Glass** (kính mờ khí động học) cho Blazor, Agent không được sử dụng các thông số CSS rập khuôn, thô cứng.
  - Đảm bảo thiết lập điểm nhấn thị giác (visual hierarchy) rõ nét, sử dụng khoảng trắng (whitespace) phóng khoáng, hợp lý.
  - Áp dụng các hiệu ứng Cyberpunk tinh tế (glowing borders mảnh, dark background sâu, frosted glass blur mượt mà qua `backdrop-filter`) để tạo cảm giác hiện đại và cao cấp, tránh làm rác hoặc quá tải giao diện.
