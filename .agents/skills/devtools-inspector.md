# Skill: DevTools Inspector (F12 Inspection)

---
name: devtools-inspector
description: Tri thức và kỹ năng sử dụng Chrome DevTools MCP để theo dõi Network, phân tích DOM/CSS và đọc Console Log trên trình duyệt.
---

## 1. Theo dõi Mạng (Network Monitor)
- Lắng nghe và giám sát các kết nối SignalR của Blazor Dashboard.
- Nếu xảy ra lỗi kết nối (ví dụ: HTTP Status Code 400/500) hoặc kết nối WebSocket bị ngắt:
  - Tự động bắt gói tin lỗi và trích xuất payload phản hồi (response payload).
  - Phân tích nguyên nhân (như thiếu JWT token, cấu hình sai CORS, hoặc hub endpoint không khớp).

## 2. Phân tích Style giao diện (DOM Styling)
- Phục vụ việc kiểm tra và tinh chỉnh giao diện **Kinetic Glass** (giao diện kính mờ khí động học).
- Soi cây DOM của trang web để phát hiện các thuộc tính CSS như:
  - `backdrop-filter`
  - `z-index`
  - `opacity`
- Tìm kiếm các xung đột CSS hoặc thuộc tính bị ghi đè (overridden rules) giữa thư viện MudBlazor và CSS custom mặc định của ứng dụng, từ đó đề xuất giải pháp CSS sửa đổi chính xác.

## 3. Lắng nghe Console Log (JS Console Exceptions)
- Bắt tất cả các lỗi runtime, Unhandled Exceptions hoặc cảnh báo (warnings) được bắn ra từ Blazor WebAssembly hoặc các cuộc gọi JS Interop trên bảng điều khiển DevTools.
- Trích xuất stack trace lỗi của Javascript/C# chạy ở client-side để chẩn đoán.
