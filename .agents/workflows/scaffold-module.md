# Quy trình Dựng Module (Scaffold Module Workflow)

Quy trình chuẩn hóa gồm 5 bước khi triển khai một module chức năng hoặc thực thể mới trong hệ thống UniEvent Hub.

## 5 Bước Thực Hiện

### Bước 1: [Plan] Lập kế hoạch thiết kế
- Sử dụng công cụ **Sequential Thinking** để phân tích yêu cầu nghiệp vụ của module.
- Xác định cấu trúc dữ liệu, các quan hệ của thực thể, và giao diện người dùng tương ứng.
- Phác thảo thiết kế và xác nhận tính khả thi trước khi viết code.

### Bước 2: [DB] Thiết kế Cơ sở Dữ liệu & Thực thể
- Tạo các lớp thực thể (Entity) trong thư mục `DAL/Models`.
- Khai báo thuộc tính và các mối quan hệ (khóa ngoại, unique indexes, many-to-many).
- Cấu hình Mapping bằng Fluent API trong `DbContext` tại thư mục `DAL/Data`.

### Bước 3: [BLL] Xây dựng Lớp Logic & Repository
- Tạo interface và class repository chuyên biệt tại thư mục `DAL/Repositories`.
- Viết các Specification hoặc lớp truy vấn để đóng gói logic truy vấn SQL/EF Core.
- Viết các Service tại `BLL/Services` để thực thi business logic, xử lý luồng nghiệp vụ và gọi repository.

### Bước 4: [UI] Phát triển Giao diện Người dùng
- Triển khai UI tương ứng với phân hệ được giao:
  - **Identity/Account:** Viết Controller MVC kết hợp ASP.NET Core Identity.
  - **Location & Event:** Viết các Razor Pages với layout Bootstrap cơ bản.
  - **Dashboard & Analytics:** Thiết kế MudBlazor components thời gian thực với render mode thích hợp.
- Đảm bảo giữ UI sạch sẽ, tuân thủ nguyên tắc wireframe cơ bản của giai đoạn 1.

### Bước 5: [Log] Nhật ký Phát triển & Ghi chép
- Cập nhật nhật ký phát triển vào file log hệ thống hoặc tài liệu bàn giao.
- Ghi lại các quyết định kỹ thuật quan trọng và kết quả chạy thử nghiệm ban đầu.
