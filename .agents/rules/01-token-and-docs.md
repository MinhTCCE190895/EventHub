# Luật Tối ưu Token và Tài liệu (Token & Docs Rules)

## 1. Phản hồi của Agent (Response style)
- Bắt buộc đi thẳng vào code, KHÔNG chào hỏi, KHÔNG giải thích dông dài.
- Chỉ xuất code thay đổi kèm comment `// ... existing code ...` để tiết kiệm token và đảm bảo độ chính xác.

## 2. Luật Tài liệu Thiết kế (Software Design Specification - SDS)
- Khi có yêu cầu viết hoặc cập nhật tài liệu thiết kế (SDS) hoặc các báo cáo dự án:
  - Mặc định mã trạng thái **"A"** luôn có nghĩa là **"Add"** (Thêm mới).
  - Ghi nhận **QuiNC** là tác giả duy nhất của module/phần code đó.
  - **TUYỆT ĐỐI** không sinh ra các giải thích thừa thãi hay chú thích dài dòng về ý nghĩa của các chữ cái A (Add), M (Modify), hay D (Delete).
