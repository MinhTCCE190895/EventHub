# Skill: Memory Compression & Research-First

---
name: memory-compression
description: Quản lý context thông qua tóm tắt (compression) các phần việc đã hoàn thiện và quy tắc Research-first để tự tra cứu trước khi đề xuất API .NET 8.
---

## 1. Luật Quản lý Context (Context & Long-Term Memory Management)
- Trong các phiên làm việc dài liên quan đến **9 thực thể** của UniEvent Hub:
  - Agent phải tự động theo dõi mức độ đầy của bộ nhớ context.
  - Khi nhận thấy dung lượng context quá tải hoặc phiên hội thoại trở nên quá dài:
    - Tự động kích hoạt luồng tóm tắt (compress) trạng thái của các module đã hoàn thiện (ví dụ: MVC Identity) vào file tài liệu snapshot: `.agents/docs/memory-snapshots.md`.
    - Sau khi ghi snapshot, Agent chỉ giữ lại trong bộ nhớ hội thoại hiện tại các luồng kiến trúc cốt lõi và các tác vụ đang thực thi dở dang để tránh hiện tượng ảo giác (hallucination) hoặc suy giảm hiệu suất xử lý.

## 2. Tư duy "Research-First" (ECC-inspired)
- Khi đối mặt với các hàm API, thư viện mới của **.NET 8** hoặc **EF Core 8.0**:
  - Agent **bắt buộc** phải ưu tiên sử dụng công cụ tìm kiếm web (web search) để tra cứu tài liệu chính thức từ Microsoft Learn hoặc các nguồn uy tín trước.
  - Tuyệt đối không tự đoán mò (hallucinate) tên hàm, cú pháp hoặc các thuộc tính mới của API.
  - Đảm bảo code đề xuất khớp 100% với đặc tả kỹ thuật của nền tảng .NET 8 ổn định nhất.

