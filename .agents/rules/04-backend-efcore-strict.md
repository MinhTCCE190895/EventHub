# Luật Backend & EF Core Nghiêm Ngặt (Backend & EF Core Rules)

## 1. Hiệu năng & Tối ưu hóa Database (Database & EF Core Performance)
- **Truy vấn Read-Only:**
  - Mọi truy vấn chỉ đọc (Read-only), đặc biệt là các dữ liệu phục vụ cho Dashboard, báo cáo và phân tích, bắt buộc phải đính kèm phương thức `.AsNoTracking()` hoặc `.AsNoTrackingWithIdentityResolution()`.
- **EF Core 8.0 Features:**
  - Khuyến khích tận dụng các tính năng tối ưu hóa của EF Core cho .NET 8 (như hỗ trợ truy vấn kiểu dữ liệu JSON tốt hơn bằng các hàm SQL gốc).
  - Sử dụng phương thức `Database.SqlQuery<T>()` cho các câu lệnh SQL thô trả về kiểu dữ liệu unmapped không thuộc DbContext.


## 2. Đồng bộ & Xử lý Đa luồng (Asynchronous & Parallel Programming)
- **Tương tác Cơ sở Dữ liệu:**
  - Tất cả các tác vụ I/O bound (tương tác database, ghi file, gọi API bên ngoài) bắt buộc phải sử dụng lập trình bất đồng bộ với cú pháp `async` / `await` (ví dụ: `ToListAsync()`, `SaveChangesAsync()`).
  - Tránh chặn luồng đồng bộ bằng `.Result` hoặc `.Wait()`.
- **Xử lý Dữ liệu Lớn (Feedback Analytics):**
  - Khi xử lý và phân tích phản hồi (Feedback) quy mô lớn trong bộ nhớ (CPU-bound), ưu tiên sử dụng **Parallel LINQ (PLINQ)** thông qua `.AsParallel()` để tối ưu hóa việc phân phối công việc lên các nhân CPU.

## 3. SignalR & Background Service
- **SignalR Capacity Hub:**
  - Các sự kiện làm thay đổi sức chứa của sự kiện (Event Capacity) như đăng ký mới (Registration), hủy đăng ký, hoặc thay đổi giới hạn chỗ ngồi bắt buộc phải được truyền thông báo thời gian thực đến client thông qua SignalR Hub.
- **Worker Service (Background Jobs):**
  - Các tác vụ chạy ngầm như gửi email xác nhận đăng ký, email khảo sát ý kiến sau sự kiện, hoặc đồng bộ hóa dữ liệu định kỳ phải được cấu hình và chạy trong các lớp kế thừa từ `BackgroundService` (Worker Service).
