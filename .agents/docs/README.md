# Bản đồ Tri thức & Liên kết Tài liệu (Knowledge Map Index)

Trang này kết nối tất cả các tài liệu kỹ thuật, kiến trúc và quy chuẩn phát triển dành cho dự án **UniEvent Hub**. AI Agents hãy sử dụng các đường link trực tiếp (`file:///`) dưới đây để truy cập nhanh các tài nguyên liên quan, tránh việc scan thư mục gây tốn token.

---

## 1. TÀI LIỆU KIẾN TRÚC & LUỒNG CẤU TRÚC (ARCHITECTURE & FLOWS)
- **[Kiến trúc hệ thống](system-architecture.md)**: Chi tiết cấu trúc 3 lớp (BLL, DAL, PL), danh sách 9 thực thể cốt lõi và quy chuẩn lập trình C# 12 / .NET 8.
- **[Sơ đồ Cơ sở Dữ liệu](database-schema.md)**: Chi tiết cấu trúc database, Fluent API cho EF Core và quan hệ thực thể.
- **[Luồng Explore Search & Filter (FE-03)](fe03-flow.md)**: Giải thích 6 bước từ Form Submit, PageModel, BLL query, Mapping DTO đến View render.

## 2. HỆ THỐNG THIẾT KẾ & STYLE GUIDE (DESIGN SYSTEM)
- **[Style Guide toàn diện cho Dự án](../../SYSTEM_DESIGN_STYLEGUIDE.md)**: Hướng dẫn chi tiết CSS variables, cấu trúc layout DOM, component `card-minimal`, trạng thái `is-past`, quy tắc tương phản (contrast accessibility) và quy tắc tùy biến linh hoạt.
- **[File CSS Toàn cục (site.css)](../../RazorPages/wwwroot/css/site.css)**: Nơi chứa toàn bộ định nghĩa các biến màu Royal Blue & Amber và CSS classes dùng chung.

## 3. TIẾN TRÌNH & TRẠNG THÁI DỰ ÁN (MODULE SNAPSHOTS)
- **[Nhật ký Trạng thái Module & Commit Snapshots](memory-snapshots.md)**: Nhật ký tóm tắt bối cảnh các phân hệ đã hoàn thành của QuiNC và Agent để kế thừa trạng thái làm việc tiếp theo.

## 4. HỆ THỐNG LUẬT PHÁT TRIỂN (DEVELOPMENT RULES)
- **[Luật Tối ưu Token & Software Design (01-token-and-docs)](../rules/01-token-and-docs.md)**.
- **[Luật Thiết kế UI/UX & Good Taste (05-ui-ux-design)](../rules/05-ui-ux-design.md)**.
- **[Luật UI Styleguide AI-Agent (07-system-design-styleguide)](../rules/07-system-design-styleguide.md)**.
- **[Luật Coding Style của QuiNC (quinc-coding-style)](../rules/quinc-coding-style.md)**.


