---
trigger: always_on
---

# Luật Tối ưu Token và Tài liệu (Token & Docs Rules)

## 1. Phản hồi của Agent (Response style) & Luật "Stop Slop"
- Bắt buộc đi thẳng vào code, KHÔNG chào hỏi, KHÔNG giải thích dông dài.
- Chỉ xuất code thay đổi kèm comment `// ... existing code ...` để tiết kiệm token và đảm bảo độ chính xác.
- **Luật "Stop Slop":** Bắt buộc mọi tài liệu SDS, giải thích và comment code phải sử dụng ngôn ngữ kỹ thuật cô đọng, sắc bén. CẤM TUYỆT ĐỐI các từ ngữ sáo rỗng, sến súa, rập khuôn của AI (ví dụ: *delve, tapestry, testament, beacon, moreover, in summary, hành trình, bức tranh toàn cảnh*,...). Tập trung vào cấu trúc dữ liệu, thuật toán và giải pháp thực tế.


## 2. Luật Tài liệu Thiết kế (Software Design Specification - SDS)
- Khi có yêu cầu viết hoặc cập nhật tài liệu thiết kế (SDS) hoặc các báo cáo dự án:
  - Mặc định mã trạng thái **"A"** luôn có nghĩa là **"Add"** (Thêm mới).
  - Ghi nhận **QuiNC** là tác giả duy nhất của module/phần code đó.
  - **TUYỆT ĐỐI** không sinh ra các giải thích thừa thãi hay chú thích dài dòng về ý nghĩa của các chữ cái A (Add), M (Modify), hay D (Delete).

## 3. Luật Thực thi Memory Snapshot & Work Log
- **Trước khi làm việc**: Agent bắt buộc phải đọc file `.agents/docs/memory-snapshots.md` để nắm rõ tiến trình và bối cảnh các module đã hoàn thiện.
- **Sau khi hoàn thành tác vụ lớn / Khi context hội thoại quá tải**:
  - Bắt buộc phải cập nhật trạng thái phân hệ và ghi nhận nhật ký làm việc (ngày tháng, tên agent, công việc ngắn gọn) vào phần **3. NHẬT KÝ THAY ĐỔI CỦA AGENT** trong file `.agents/docs/memory-snapshots.md`.
  - File `.agents/docs/memory-snapshots.md` phải được force add (`git add -f`) và commit/push lên repository cùng với code của tính năng đó để đồng bộ cho toàn bộ các thành viên khác và các Agent AI tiếp theo.

## 4. Luật Sử dụng Codegraph để Phân tích Mã nguồn
- Khi cần phân tích luồng code, tìm kiếm cấu trúc class, kiểm tra các lớp gọi (Callers) / lớp bị gọi (Callees) hoặc phân tích tầm ảnh hưởng của thay đổi (Impact Analysis), Agent bắt buộc phải ưu tiên sử dụng các MCP tools của `codegraph` (ví dụ: `codegraph_search`, `codegraph_callers`, `codegraph_callees`, `codegraph_impact`).
- Hạn chế sử dụng grep text đơn giản đối với các tác vụ liên quan đến phân tích cấu trúc Roslyn để đảm bảo độ chính xác tuyệt đối của cấu trúc Clean Architecture trong dự án.
- **Hướng dẫn Cài đặt & Cấu hình Codegraph MCP (Nếu chưa có):**
  - Đảm bảo trong file `.agents/mcp-config.json` có cấu hình block `"codegraph"` chạy bằng npx với package `codegraph-mcp`.
  - Nếu Agent hoặc IDE báo thiếu tool, lập trình viên/Agent cần cài đặt hoặc khởi chạy thủ công thông qua CLI bằng lệnh:
    ```bash
    npx -y codegraph-mcp
    ```
  - Cấu hình MCP server trong Settings của IDE Client (Cursor/VSCode/Windsurf) trỏ đến file `.agents/mcp-config.json` để tự động tích hợp.

## 5. Luật Xác định Danh tính & Vai trò Thành viên (Member Identity & Persona)
- **Bắt đầu Hội thoại mới (Conversation Startup)**: 
  - Trong lượt phản hồi đầu tiên của một cuộc hội thoại mới, Agent **bắt buộc** phải hỏi người dùng xem họ là thành viên nào trong dự án UniEvent Hub (Ví dụ: *"Bạn là QuiNC, LongNH, MinhTC, Khôi hay TriLT?"*) trừ khi thông tin này đã được người dùng chủ động khai báo từ trước.
- **Phong cách Xưng hô & Lập trình theo Persona**:
  - **Nếu là QuiNC (quinc-fptu)**:
    - Xưng hô: "anh QuiNC" hoặc "anh".
    - Phân hệ hỗ trợ: Search & Filter (`FE-03`), Weather Widget (`FE-08`), Bookmark (`FE-11`).
    - Coding Style: Code đơn giản dạng intern/junior, LINQ method syntax, viết comment giải thích lý do bằng tiếng Việt, commit ngắn gọn thực tế, không viết unit test.
  - **Nếu là LongNH (Nhóm trưởng)**:
    - Xưng hô: "anh Long" hoặc "Trưởng nhóm Long".
    - Phân hệ hỗ trợ: MVC Identity & Authorization (`FE-01`), Admin Control Panel (`FE-09`).
    - Focus: Quản lý RBAC, Unit of Work, DbContext Setup, bảo mật MVC Views.
  - **Nếu là MinhTC**:
    - Xưng hô: "anh Minh".
    - Phân hệ hỗ trợ: Event CRUD (`FE-02`), Venue Limits (`FE-05`), Event Requests (`FE-13`).
    - Focus: Razor Pages PageModel, AutoMapper, xử lý DB Exceptions, chống Over-posting qua `[BindProperty]`.
  - **Nếu là Khôi**:
    - Xưng hô: "anh Khôi".
    - Phân hệ hỗ trợ: Live Ticket Booking (`FE-04`), Live Dashboard (`FE-10`), Live Q&A Hub (`FE-15`).
    - Focus: Blazor Components, SignalR Hubs, Concurrency Exception (`DbUpdateConcurrencyException`), Rate Limiting.
  - **Nếu là TriLT**:
    - Xưng hô: "anh TriLT" hoặc "anh Trí".
    - Phân hệ hỗ trợ: Email Reminders (`FE-06`), Feedback + Metrics (`FE-07`), Follows System (`FE-12`).
    - Focus: BackgroundService, `Parallel.ForEachAsync`, PLINQ (`.AsParallel()`), tối ưu truy vấn đếm.

## 6. Luật Quản lý File Cấu hình (appsettings.json & ConnectionString)
- **Cấm tự ý chỉnh sửa:** Agent tuyệt đối **không được tự ý thay đổi** chuỗi kết nối (`ConnectionStrings`) hoặc các cấu hình SMTP/Email trong file `appsettings.json` của bất kỳ dự án nào (`Blazor`, `MVC`, `RazorPages`) trừ khi được người dùng yêu cầu trực tiếp.
- **Xử lý cấu hình local:** Trường hợp cần thay đổi chuỗi kết nối để chạy thử dưới local (ví dụ: chuyển từ LocalDB sang SQL Server cục bộ), Agent phải hướng dẫn người dùng chạy lệnh khóa file hoặc tự chạy lệnh sau để tránh việc commit đè cấu hình cá nhân lên repository:
  ```bash
  git update-index --assume-unchanged Blazor/appsettings.json MVC/appsettings.json RazorPages/appsettings.json
  ```






