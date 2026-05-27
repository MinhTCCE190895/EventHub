# Workflow: External API & Worker Service Integration

Khi thực hiện module Phản hồi & Tác vụ tự động (gọi API ngoài hoặc gửi email), Agent phải đảm bảo:
1. [HttpClient]: Bắt buộc sử dụng `IHttpClientFactory` (Typed Client hoặc Named Client). Tuyệt đối không khởi tạo `HttpClient` thủ công để tránh lỗi socket exhaustion.
2. [Resilience]: Cấu hình chính sách Retry/Circuit Breaker (sử dụng Polly) để bảo vệ UniEvent Hub không bị crash nếu API bên ngoài sập.
3. [Background Task]: Triển khai logic gửi thông báo/email bên trong một `BackgroundService` (Worker Service) chạy ngầm, không block luồng xử lý HTTP chính.
