# Clean Architecture Scan Report: MVC
**Scan Timestamp**: 2026-07-21 23:38:38  
**Target Directory**: MVC  
**Total Violations Found**: 3

## Summary by Rule ID
| Rule ID | Severity | Description | Count | Tier Level |
| :--- | :--- | :--- | :--- | :--- |
| `CA-WARN-01` | **Warning** | Bắt ngoại lệ nghiệp vụ riêng lẻ (catch ArgumentException / InvalidOperationException) tự ánh xạ vào form field | **0** | Level 2: Review Required |
| `CA-ERR-03` | **Error** | Chứa logic nghiệp vụ rẽ nhánh (if/else kiểm tra StartTime, EndTime, MaxCapacity, Status) tại tầng UI | **0** | Level 2: Review Required |
| `CA-ERR-01` | **Error** | Tiêm trực tiếp AppDbContext / IDbContext / DbSet vào tầng Presentation (.cshtml.cs, Controller, .razor) | **0** | Level 2: Review Required |
| `CA-WARN-02` | **Warning** | Chứa switch/case ánh xạ màu sắc UI (badgeClass, bg-) hoặc trạng thái nằm trực tiếp bên trong View HTML | **3** | Level 2: Review Required |
| `CA-ERR-02` | **Error** | Gọi trực tiếp LINQ/EF Core query (.Where, .Include, .FirstOrDefaultAsync, .ToListAsync) trong Presentation | **0** | Level 2: Review Required |
| `CA-CLEAN-01` | **Notice** | Code thừa cơ học: using DAL/EF Core khi không dùng DB, khối catch rỗng, hoặc comment code chết | **0** | Level 1: Safe Auto-fix |

## Detailed Violations List
| Rule ID | Severity | File : Line | Code Snippet | Recommended Action |
| :--- | :--- | :--- | :--- | :--- |
| `CA-WARN-02` | **Warning** | [MVC\Views\Admin\Users.cshtml:88](file:///MVC\Views\Admin\Users.cshtml#L88) | ` "Admin" => "bg-danger-subtle text-danger border-danger-su... ` | Review Required (Chuyển switch/case màu sắc sang DTO hoặc PageModel) |
| `CA-WARN-02` | **Warning** | [MVC\Views\Admin\Users.cshtml:89](file:///MVC\Views\Admin\Users.cshtml#L89) | ` "Organizer" => "bg-primary-subtle text-primary border-pri... ` | Review Required (Chuyển switch/case màu sắc sang DTO hoặc PageModel) |
| `CA-WARN-02` | **Warning** | [MVC\Views\Admin\Users.cshtml:90](file:///MVC\Views\Admin\Users.cshtml#L90) | ` _ => "bg-secondary-subtle text-secondary border-secondary... ` | Review Required (Chuyển switch/case màu sắc sang DTO hoặc PageModel) |
