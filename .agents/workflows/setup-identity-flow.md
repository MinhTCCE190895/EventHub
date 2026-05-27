# Workflow: Setup ASP.NET Core Identity (Module Định danh)

Khi được yêu cầu khởi tạo phân hệ Account/Role bằng Identity, Agent phải tuân thủ 4 bước:
1. [Entity Mapping]: Kế thừa `IdentityUser` cho thực thể `Account` và `IdentityRole` cho thực thể `Role`. Không tự ý tạo bảng User riêng ngoài hệ thống Identity.
2. [DbContext]: Kế thừa `IdentityDbContext` và cấu hình schema bằng Fluent API.
3. [Middleware]: Đăng ký dịch vụ Identity và cấu hình Authentication/Authorization pipeline trong `Program.cs`.
4. [UI Scaffold]: Dựng các view Đăng nhập/Đăng ký cơ bản bằng kiến trúc MVC, tách biệt rõ ràng Controller và View.
