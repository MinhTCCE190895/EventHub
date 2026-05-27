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
- **Adaptive Style (Thích ứng phong cách):**
  - Tạm thời Agent chỉ được phép dựng giao diện thô (wireframe/layout cơ bản) sạch sẽ, nguyên bản. Khi nhận được lệnh thiết kế cụ thể từ người dùng (ví dụ: Kinetic Glass, Neumorphism, Minimalist, Corporate...), Agent phải linh hoạt chuyển đổi CSS/SCSS theo đúng phong cách đó.
- **Luật "Good Taste" (Thẩm mỹ & Gu thiết kế):**
  - Dù áp dụng phong cách nào, BẮT BUỘC tuân thủ các nguyên tắc thiết kế tinh tế:
    1. Không rập khuôn: Tránh sử dụng các thông số CSS mặc định thô cứng của framework. Hãy tinh chỉnh radius, shadow, transition để tạo cảm giác "premium" (cao cấp). Đặc biệt khi thiết kế giao diện **Kinetic Glass** (kính mờ khí động học) cho Blazor, không dùng các thông số CSS mặc định, phải tinh chỉnh hiệu ứng mờ Cyberpunk tinh tế, glow borders mảnh, dark theme sâu qua `backdrop-filter`.
    2. Bố cục không gian: Thiết lập điểm nhấn thị giác (visual hierarchy) rõ rệt. Sử dụng khoảng trắng (whitespace) và padding/margin phóng khoáng để giao diện "thở" được.
    3. Chống rác (Anti-slop): Tránh lạm dụng quá nhiều màu sắc, viền hoặc hiệu ứng nhấp nháy làm quá tải thị giác người dùng. Các hiệu ứng (nếu có) phải mượt mà và phục vụ luồng UX.