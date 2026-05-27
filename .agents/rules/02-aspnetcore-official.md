# ASP.NET Core Design Rules

Tri thức được trích xuất từ các chuẩn mực thiết kế chính chủ của Microsoft và cộng đồng .NET Core (Blazor, MVC, Razor Pages).

## 1. Blazor Components & Render Modes
- **Lựa chọn Render Mode:**
  - Bắt đầu với **Static SSR** khi trang chủ yếu hiển thị dữ liệu read-only và cần thời gian tải trang ban đầu cực nhanh.
  - Sử dụng **Interactive Server** (SignalR) cho các component cần tương tác thời gian thực hoặc tương tác phong phú mà không muốn tải toàn bộ runtime WebAssembly xuống client.
  - Sử dụng **Interactive WebAssembly** cho các tác vụ cần chạy hoàn toàn ngoại tuyến hoặc yêu cầu tính toán nặng tại phía client.
  - Chỉ trộn lẫn các render mode khi có sự phân chia logic rõ ràng và được phê duyệt.
- **Thiết kế Component:**
  - Giữ các component nhỏ gọn, có thể tái sử dụng và tập trung vào một nhiệm vụ duy nhất (Single Responsibility).
  - Tách biệt UI presentation và logic xử lý dữ liệu.

## 2. MVC & Razor Pages Architecture
- **MVC (Identity & Security):**
  - Controller chỉ đóng vai trò điều phối HTTP (HTTP orchestration), nhận request, gọi service và trả về view/response.
  - Tuyệt đối không viết logic nghiệp vụ (business logic) trực tiếp trong Controller.
  - Sử dụng Filters (Action, Exception, Resource filters) để giải quyết các mối quan tâm chéo (cross-cutting concerns).
- **Razor Pages (Location & Event Modules):**
  - Sử dụng PageModel để quản lý trạng thái và liên kết dữ liệu (data binding) cho trang.
  - Đảm bảo các handler methods (`OnGetAsync`, `OnPostAsync`) phản ánh chính xác hành động HTTP.
  - Tránh viết logic nghiệp vụ phức tạp trực tiếp trong PageModel; chuyển tiếp chúng qua Service/Repository.

## 3. Dependency Injection (DI) & Configuration
- Luôn sử dụng DI để tiêm các service phụ thuộc thông qua Constructor Injection. Tránh sử dụng Service Locator pattern (`IServiceProvider.GetService`).
- Phân nhóm đăng ký dịch vụ trong `Program.cs` thành các Extension Methods sạch sẽ (ví dụ: `builder.Services.AddInfrastructureServices(...)`).
- Sử dụng Options Pattern (`IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`) để cấu hình có kiểu dữ liệu mạnh (strongly-typed settings).
