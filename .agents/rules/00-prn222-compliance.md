# 00. Tiêu chuẩn Kỹ thuật Cốt lõi (Mức độ ưu tiên cao nhất)

Bất kỳ đoạn code nào được sinh ra BẮT BUỘC phải thỏa mãn các tiêu chuẩn kỹ thuật khắt khe sau:
- Dependency Injection (DI): Tuyệt đối không dùng từ khóa `new` để khởi tạo các service hoặc repository trong controller/page. Mọi dependency phải được inject qua constructor.
- MVC (Module Quản lý Định danh & Quyền): Bắt buộc phải có Model Validation (DataAnnotations) chặt chẽ cho mọi ViewModel/DTO.
- Razor Pages (Module Không gian & Sự kiện): Bắt buộc sử dụng đúng chuẩn Handler Methods (`OnGetAsync`, `OnPostAsync`). 
- Xử lý ngoại lệ (Exception Handling): Không được để try-catch rỗng. Mọi exception phải được log lại.
