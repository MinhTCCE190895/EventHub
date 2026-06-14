# Luật Tự động Tham chiếu Agent Skills & Workflows

## 1. Định nghĩa và Bản đồ Tài nguyên
Agent phải luôn tự động tham chiếu đến các tài nguyên nằm trong thư mục `.agents/` khi nhận được các tác vụ tương ứng:

### A. Thư mục `.agents/skills/` (Kỹ năng & Mẫu code cấu trúc)
- **`scaffolding-templates.md`**: Chứa mẫu chuẩn cho Razor Page, Controller, Service, Repository, DTO, Mapper, Validator.
- **`bogus-data-seeder.md`**: Chứa mẫu và thư viện để seed dữ liệu giả (Bogus) phục vụ testing.
- **`devtools-inspector.md`**: Quy trình sử dụng chrome-devtools-mcp để debug giao diện và logic client-side.
- **`github-ci-fixer.md`**: Các bước xử lý lỗi build, test và cấu hình Github Actions CI.
- **`memory-compression.md`**: Cách tóm tắt, dọn dẹp context khi bộ nhớ hội thoại quá tải.

### B. Thư mục `.agents/workflows/` (Quy trình thực thi đa bước)
- **`scaffold-module.md`**: Quy trình 8 bước chuẩn để dựng mới một module (từ DB, Entity đến FE/API).
- **`migration-flow.md`**: Quy trình chuẩn khi cập nhật Database Schema, tạo và áp dụng Entity Framework Migrations.
- **`setup-identity-flow.md`**: Quy trình thiết lập ASP.NET Core Identity, Roles và Authorization.
- **`external-api-flow.md`**: Quy trình tích hợp các API bên ngoài (Weather, Map, Payment).
- **`vibe-auto-heal.md`**: Quy trình tự phát hiện lỗi, đọc log và tự động sửa lỗi (Auto-heal).

---

## 2. Quy tắc Trigger (Khi nào bắt buộc phải nạp file)
Trước khi viết code hoặc thực hiện tác vụ, Agent phải kiểm tra từ khóa trong yêu cầu của User để tự động gọi `view_file` nạp tài liệu tương ứng:

| Tác vụ / Từ khóa yêu cầu | File cần nạp (`view_file`) |
| :--- | :--- |
| Tạo mới Page, Model, Service, Repo, Mapper, DTO, CRUD | `.agents/skills/scaffolding-templates.md` và `.agents/workflows/scaffold-module.md` |
| Add Migration, Update DB, Thay đổi Entity, Schema DB | `.agents/workflows/migration-flow.md` |
| Seed data, Fake data, dữ liệu mẫu, Bogus | `.agents/skills/bogus-data-seeder.md` |
| Login, Register, Phân quyền, Role, Identity, Authorization | `.agents/workflows/setup-identity-flow.md` |
| Lỗi build, Lỗi CI, Github Actions, Workflow build | `.agents/skills/github-ci-fixer.md` |
| Tích hợp API Weather, Map, Payment, Third-party | `.agents/workflows/external-api-flow.md` |
| Test giao diện, Debug CSS/JS trên Browser, DevTools | `.agents/skills/devtools-inspector.md` |

---

## 3. Quy trình Thực thi của Agent
1. **Quét yêu cầu (Scan intent)**: Xác định từ khóa trùng khớp với bảng Trigger ở mục 2.
2. **Đọc tài liệu (Load context)**: Gọi `view_file` để đọc nội dung file skill/workflow được chỉ định trước khi thực hiện bất kỳ thao tác thay đổi code nào.
3. **Tuân thủ thiết kế (Compliance)**: Triển khai đúng các bước trong workflow và sử dụng đúng template trong skill đã nạp.
