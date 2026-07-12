---
name: clean-architecture-crud-refactor
description: >-
  Tự động kiểm tra, rà soát và tái cấu trúc (Refactor) chuẩn Clean Architecture cho các phân hệ CRUD (RazorPages, MVC, Blazor Components) theo quy trình 3 bước (Quét tĩnh -> Phân cấp xử lý 3 mức -> Kiểm chứng Rollback Guard).
---

# Clean Architecture CRUD Refactorer (`clean-architecture-crud-refactor`)

## Overview
Skill chuyên dụng để duy trì tính độc lập giữa các tầng kiến trúc trong hệ thống Clean Architecture (.NET Core / C#). Khi được gọi trên bất kỳ phân hệ CRUD nào (như `Events`, `Venues`, `EventRequests`, `Users`...), Skill sẽ kết hợp quét tĩnh (`check_clean_arch.ps1`) để phát hiện vi phạm khách quan, và dùng lý luận kiến trúc của AI để xử lý theo 3 mức độ rủi ro nhằm loại bỏ triệt để logic nghiệp vụ và truy vấn CSDL khỏi tầng Presentation (`RazorPages`, `MVC`, `Blazor Components`).

## Quick Start
Khi người dùng yêu cầu rà soát hoặc refactor một module theo chuẩn Clean Architecture (ví dụ: *"Hãy rà soát CRUD Venues theo chuẩn"*, *"Kiểm tra Clean Architecture cho RazorPages/Pages/Requests"*):

1. **Bước 1: Quét tĩnh bằng script PowerShell**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agents/skills/clean-architecture-crud-refactor/scripts/check_clean_arch.ps1 -TargetDir "<Đường_dẫn_thư_mục>" -OutputFormat Both
   ```
2. **Bước 2: Đọc báo cáo quét (`.agents/docs/reports/clean-arch-scan-*.md`)**:
   - Nếu `Total Violations Found: 0`, báo cáo nhanh cho người dùng codebase đã đạt chuẩn.
   - Nếu có vi phạm, tiến hành xử lý theo **Quy tắc Phân cấp 3 Mức (Tiered Handling)** bên dưới.
3. **Bước 3: Kiểm chứng & Bảo vệ (Verify & Rollback Guard)**:
   - Thực thi `dotnet build` ngay sau mỗi thao tác tự động sửa.

---

## Bảng Phân loại Vi phạm (Rule ID Registry)

| Rule ID | Severity | Mô tả vi phạm | Vị trí thường gặp (`RazorPages`, `MVC`, `Blazor`) |
| :--- | :--- | :--- | :--- |
| `CA-ERR-01` | **Error** | Tiêm trực tiếp `AppDbContext`, `IDbContext` hoặc `DbSet<T>` vào tầng Presentation | `.cshtml.cs`, Controller, `.razor` (`@code`), `.razor.cs` |
| `CA-ERR-02` | **Error** | Gọi trực tiếp LINQ/EF Core query (`.Where(`, `.FirstOrDefaultAsync(`, `.Include(`...) trên Repository tại tầng Presentation | `.cshtml.cs`, Controller, `.razor` (`@code`), `.razor.cs` |
| `CA-ERR-03` | **Error** | Chứa logic nghiệp vụ rẽ nhánh (`if / else if` kiểm tra thời gian `StartTime < EndTime`, kiểm tra sức chứa `MaxCapacity`, trạng thái...) ngay trong UI / Controller | `.cshtml.cs`, Controller, `.cshtml`, `.razor`, `.razor.cs` |
| `CA-WARN-01` | **Warning** | Bắt và phân loại riêng biệt từng ngoại lệ nghiệp vụ (`catch (ArgumentException)`, `catch (InvalidOperationException)`) để tự gán vào form field cụ thể | `.cshtml.cs` (`OnPostAsync`), Controller |
| `CA-WARN-02` | **Warning** | Logic hiển thị ánh xạ (`switch / case`) đổi mã `Status` / `Type` sang màu sắc (`badgeClass`) hay hiển thị nằm trực tiếp bên trong View HTML hoặc cần chuẩn hóa | `.cshtml`, Component `.razor` |
| `CA-CLEAN-01` | **Notice** | Vi phạm cơ học mức độ thấp không làm thay đổi hành vi runtime: `using` thừa (`using DAL;`, `using Microsoft.EntityFrameworkCore;` trong PageModel khi không dùng DB trực tiếp), comment code chết của logic cũ, khối `catch` rỗng thừa thãi | `.cshtml.cs`, Controller, `.razor.cs` |

---

## Quy tắc Phân cấp Xử lý 3 Mức (Tiered Handling Rules)

### Cấp độ 1: Auto-fix An toàn (Safe Auto-fix) — `CA-CLEAN-01`
Áp dụng cho các vi phạm cơ học rõ ràng, rủi ro thấp và không làm thay đổi hành vi runtime (`CA-CLEAN-01`):
- Xóa các câu lệnh `using DAL;`, `using Microsoft.EntityFrameworkCore;` thừa thãi tại tầng Presentation khi PageModel/Controller không truy cập DB.
- Xóa bỏ các khối comment code chết liên quan đến logic cũ (ví dụ: `// var x = _context.Events...`).
- Xóa bỏ hoặc thu gọn các khối `catch` rỗng vô nghĩa (`catch (Exception) { }`).

> [!IMPORTANT]
> **Safety Precondition & Rollback Guard**:
> 1. Trước khi sửa, kiểm tra `git status --porcelain`. Nếu có file chưa commit khác, tạo checkpoint hoặc chỉ chỉnh sửa trên file sạch.
> 2. Thực hiện sửa đổi cơ học (`CA-CLEAN-01`).
> 3. Chạy lệnh `dotnet build` ngay lập tức.
> 4. Nếu build thất bại (`Error(s) > 0`), lập tức thực hiện `git restore <file_vừa_sửa>` để khôi phục nguyên trạng và chuyển lỗi đó lên Cấp độ 3 (Báo cáo).

---

### Cấp độ 2: Auto-fix Có Review (Review Required) — `CA-ERR-01`, `CA-ERR-02`, `CA-ERR-03`, `CA-WARN-01`, `CA-WARN-02`
Áp dụng cho các vi phạm cấu trúc và logic nghiệp vụ. **CẤM TỰ ĐỘNG APPLY LÊN REMOTE HOẶC COMMIT KHI CHƯA ĐƯỢC DUYỆT**. Agent phải lập kế hoạch hoặc diff chi tiết (`implementation_plan.md` / diff report) trình bày cho người dùng (`[Approve / Reject]`):

1. **Xử lý `CA-ERR-01` & `CA-ERR-02` (Vi phạm Data Access / LINQ ở UI)**:
   - Thay thế việc tiêm `AppDbContext` bằng `I<Module>Service` hoặc `IUnitOfWork` + Repository thông qua tầng Service.
   - Di chuyển toàn bộ truy vấn `.Where()`, `.Include()`, `.FirstOrDefaultAsync()` từ PageModel/Controller xuống các phương thức trong `BLL/Services/<Module>Service.cs` và khai báo trong `BLL/Interfaces/I<Module>Service.cs`.
2. **Xử lý `CA-ERR-03` (Vi phạm Logic Nghiệp vụ ở UI)**:
   - Đẩy toàn bộ các kiểm tra nghiệp vụ (ví dụ: `StartTime >= EndTime`, `Quantity > MaxCapacity`, kiểm tra ràng buộc khóa ngoại) vào Service (hoặc FluentValidation/IValidatableObject tại `BusinessObjects`).
   - Service sẽ throw ngoại lệ nghiệp vụ (`ArgumentException`, `InvalidOperationException`) hoặc trả về `Result<T>` khi vi phạm.
3. **Xử lý `CA-WARN-01` (Vi phạm Exception Mapping field-level)**:
   - Nếu PageModel đang cố tình map ngoại lệ `ArgumentException` vào trường form cụ thể (`ModelState.AddModelError("Input.EndTime", ex.Message)`), cần đánh giá UX trước khi chuyển đổi sang `catch (Exception ex)` chung để tránh mất ngữ cảnh lỗi trên UI.
4. **Xử lý `CA-WARN-02` (Chuẩn hóa logic hiển thị switch/case)**:
   - Di chuyển logic ánh xạ trạng thái sang màu sắc / tên tiếng Việt (`switch/case` màu badge) vào **DTO (`BusinessObjects/DTOs/<Model>DTO.cs`)** thông qua các thuộc tính computed (như `StatusDisplayName`, `StatusBadgeClass`) để tái sử dụng giữa `List.cshtml`, `Detail.cshtml`, `Edit.cshtml`, và API.
   - Trong trường hợp hiển thị đặc thù riêng biệt chỉ cho 1 trang duy nhất, đặt thuộc tính computed ngay tại `PageModel (.cshtml.cs)` hoặc `ViewModel`.

---

### Cấp độ 3: Chỉ Báo cáo (Report Only)
Áp dụng khi vi phạm thuộc các trường hợp mơ hồ, ảnh hưởng chéo đến nhiều module, hoặc khối code có comment đặc biệt (`// clean-arch-ignore` hoặc `@* clean-arch-ignore *@`):
- Liệt kê tường minh trong báo cáo quét (`.agents/docs/reports/...`).
- Giải thích lý do vì sao không tự động sửa (ví dụ: *"Lệnh `_context` trong `AdminDebug.cshtml.cs` được đánh dấu ignore"* hoặc *"Logic kiểm tra này phụ thuộc vào 3 service khác nhau cần kiến trúc sư quyết định"*).

---

## Mẫu Chuẩn hóa (Canonical Before / After Patterns)

### Pattern 1: Chuẩn hóa logic hiển thị UI sang DTO (`CA-WARN-02`)
- **Before (`Detail.cshtml` - Vi phạm chèn logic switch/case vào HTML)**:
  ```cshtml
  @{
      var badgeClass = Model.EventItem.Status switch { "Published" => "bg-success", _ => "bg-secondary" };
  }
  <span class="badge @badgeClass">@Model.EventItem.Status</span>
  ```
- **After (`EventDTO.cs` + `Detail.cshtml` - Chuẩn tái sử dụng UI-agnostic)**:
  ```csharp
  // Trong BusinessObjects/DTOs/EventDTO.cs
  public string StatusDisplayName => Status switch {
      "Published" => "Đã xuất bản",
      "Draft" => "Bản nháp",
      _ => Status ?? ""
  };
  public string StatusBadgeClass => Status switch {
      "Published" => "bg-success",
      "Draft" => "bg-secondary",
      _ => "bg-light text-dark"
  };
  ```
  ```cshtml
  @* Trong Detail.cshtml *@
  <span class="badge @Model.EventItem.StatusBadgeClass">@Model.EventItem.StatusDisplayName</span>
  ```

### Pattern 2: Đẩy LINQ Query & Data Access từ PageModel xuống Service (`CA-ERR-01`, `CA-ERR-02`)
- **Before (`Index.cshtml.cs` - Vi phạm gọi trực tiếp EF Core & LINQ)**:
  ```csharp
  public class IndexModel : PageModel {
      private readonly AppDbContext _context;
      public IndexModel(AppDbContext context) => _context = context;
      public IList<EventDTO> Events { get; set; }

      public async Task OnGetAsync(string searchTerm) {
          Events = await _context.Events
              .Include(e => e.Venue)
              .Where(e => e.Title.Contains(searchTerm))
              .Select(e => new EventDTO { Title = e.Title })
              .ToListAsync();
      }
  }
  ```
- **After (`Index.cshtml.cs` + `EventService.cs` - Chuẩn decoupled)**:
  ```csharp
  // Trong IndexModel (.cshtml.cs)
  public class IndexModel : PageModel {
      private readonly IEventService _eventService;
      public IndexModel(IEventService eventService) => _eventService = eventService;
      public IList<EventDTO> Events { get; set; }

      public async Task OnGetAsync(string searchTerm) {
          Events = await _eventService.SearchEventsAsync(searchTerm);
      }
  }
  ```

### Pattern 3: Dọn dẹp câu lệnh thừa (`CA-CLEAN-01` - Level 1 Safe Auto-fix)
- **Before (`Detail.cshtml.cs`)**:
  ```csharp
  using Microsoft.EntityFrameworkCore;
  using DAL.Repositories;
  
  public class DetailModel : PageModel {
      private readonly IEventService _eventService;
      // ... không có bất kỳ lệnh EF Core hay Repository nào trong file ...
  }
  ```
- **After (`Detail.cshtml.cs`)**:
  ```csharp
  public class DetailModel : PageModel {
      private readonly IEventService _eventService;
  }
  ```

---

## Whitelist & Ngoại lệ (`clean-arch-ignore`)
Để bỏ qua vi phạm cố ý (như trang Admin Debug hoặc công cụ test nội bộ), thêm comment sau vào ngay trước hoặc cùng dòng với khối code:
- **Trong C# (`.cs` / `.cshtml.cs` / `.razor.cs`)**: `// clean-arch-ignore`
- **Trong Razor (`.cshtml` / `.razor`)**: `@* clean-arch-ignore *@`

---

## Quy tắc Kiểm chứng Cuối cùng (Final Verification Checklist)
Sau khi hoàn tất tái cấu trúc (bất kể cấp độ nào), Agent BẮT BUỘC thực hiện tuần tự:
1. `dotnet build` $\rightarrow$ Xác nhận `Build succeeded. 0 Error(s)`.
2. `dotnet test` (nếu solution có unit test) $\rightarrow$ Xác nhận toàn bộ test case `Passed`.
3. Cập nhật nhật ký làm việc và danh sách module đã rà soát vào `.agents/docs/memory-snapshots.md`.
