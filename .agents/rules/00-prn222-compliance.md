# 00. Tiêu chuẩn Kỹ thuật Cốt lõi (Mức độ ưu tiên cao nhất)

Bất kỳ đoạn code nào được sinh ra BẮT BUỘC phải thỏa mãn các tiêu chuẩn kỹ thuật PRN222 sau:

## 1. Phân chia Trách nhiệm 3-Layer (Architecture Constraints)
- **Tầng PL (Presentation Layer - MVC/Razor Pages/Blazor):** Tuyệt đối không tiêm (inject) `DbContext` hoặc gọi trực tiếp các `Repository` để xử lý nghiệp vụ/lấy dữ liệu. Mọi tương tác dữ liệu phải thông qua các `Service` ở tầng BLL.
- **Tầng BLL (Business Logic Layer):** Chứa toàn bộ logic xử lý, kiểm tra ràng buộc và điều phối nghiệp vụ.
- **Tầng DAL (Data Access Layer):** Chỉ chứa các `Entity`, `DbContext` và lớp `Repository` phục vụ truy vấn thô.

## 2. Quản lý Connection String & Cấu hình (Configuration & Security)
- Tuyệt đối không hardcode chuỗi kết nối (Connection String) hoặc các khóa bí mật trong mã nguồn hoặc lớp `DbContext`.
- Bắt buộc khai báo cấu hình trong file `appsettings.json` (hoặc `appsettings.Development.json` khi chạy local) và đọc thông qua cơ chế `IConfiguration` hoặc Options Pattern (`IOptions`).

## 3. Kiểm soát Tính nhất quán Dữ liệu (Transactions & UoW)
- Không gọi `SaveChanges` hoặc `SaveChangesAsync` vô tội vạ sau mỗi câu lệnh đơn lẻ. Mọi thao tác ghi/cập nhật dữ liệu liên quan đến nhiều bảng/nhiều bản ghi trong cùng một yêu cầu phải được thực thi qua một `Unit of Work` hoặc bọc trong `IDbContextTransaction` để đảm bảo tính nhất quán (Atomicity).
- Bắt buộc xử lý lỗi chi tiết khi thực thi database (ví dụ: `DbUpdateException`, `DbUpdateConcurrencyException`) và rollback transaction nếu xảy ra lỗi.

## 4. Lập trình Bất đồng bộ Triệt để (Asynchronous Programming)
- Mọi thao tác truy xuất Database (EF Core), I/O bound hoặc gọi API ngoại vi phải sử dụng các API bất đồng bộ (ví dụ: `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`).
- Tuyệt đối không sử dụng `.Result`, `.GetAwaiter().GetResult()` hoặc `.Wait()` trên các luồng đồng bộ để tránh gây Thread Starvation và Deadlock.

## 5. Các Tiêu chuẩn Khác
- **Dependency Injection (DI):** Không dùng từ khóa `new` để khởi tạo các service hoặc repository. Mọi dependency phải được inject qua constructor.
- **Model Validation:** Bắt buộc áp dụng Model Validation (DataAnnotations) chặt chẽ cho mọi DTO/ViewModel ở PL và kiểm tra `ModelState.IsValid` (hoặc `EditContext` đối với Blazor) trước khi xử lý.
- **Exception Handling:** Không viết khối catch rỗng (`catch { }`). Mọi lỗi phải được log đầy đủ thông qua `ILogger`.
- **Bảo mật OWASP:**
  - Form gửi dữ liệu (MVC/Razor Pages) bắt buộc có `[ValidateAntiForgeryToken]`.
  - SignalR Hub phải cấu hình Authorization hợp lệ.
  - Tuyệt đối không dùng nối chuỗi SQL trong DbContext. Chỉ sử dụng LINQ hoặc parameterized queries để chống SQL Injection.
