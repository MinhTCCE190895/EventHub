# FE-03 Search & Filter Events — Luồng hoạt động thực tế

> Đọc cái này trước khi lên bảng vấn đáp để nắm chắc luồng xử lý của phân hệ Tìm kiếm & Lọc sự kiện trên trang chủ.

---

## 🛠️ Luồng hoạt động (Sơ đồ tổng quan)

```
Sinh viên bấm bộ lọc / gõ từ khóa trên Giao diện
    │
    ▼
Index.cshtml ── JS submitInstant() chạy ngầm
    │ (Gửi HTTP GET request ngầm bằng fetch lên server)
    ▼
Index.cshtml.cs (PageModel) ── OnGetAsync()
    ├─ Tự động bind tham số URL -> SearchVm (ViewModel)
    ├─ Lấy danh sách Bookmarks nếu là Student đã đăng nhập
    ├─ Load danh sách Categories + Tags song song (hiển thị lại lên bộ lọc)
    ├─ Đóng gói SearchVm -> EventSearchDTO gửi đi
    │
    ▼
SearchService.cs (BLL) ── SearchEventsAsync(dto)
    ├─ Gọi EventRepository.BuildSearchQuery() (SQL JOIN nạp sẵn chống N+1)
    ├─ Nối thêm điều kiện lọc Where (Keywords, Category, Tags, TimeFilter, StartDate/EndDate)
    ├─ Chạy query CountAsync() (SQL đếm tổng số kết quả)
    ├─ Chạy query Skip().Take().ToListAsync() (SQL phân trang lấy 9 item)
    ├─ Map List<Event> -> List<EventCardDTO>
    │
    ▼
Index.cshtml.cs nhận kết quả trả về
    ├─ Gán vào SearchVm.Results
    │
    ▼
Index.cshtml render HTML mới
    └─ JS bóc tách cục kết quả mới đè vào giao diện hiện tại (Không reload trang)
```

---

## 📂 Các File Tham Gia Trực Tiếp Trong Luồng

| Tầng | Tên File | Chức năng cụ thể trong code |
|---|---|---|
| **View (PL)** | `Pages/Index.cshtml` | Render form tìm kiếm, danh sách tag, card grid/list và xử lý AJAX ngầm. |
| **PageModel (PL)** | `Pages/Index.cshtml.cs` | Nhận request GET, nạp dropdown categories/tags, gọi service và trả HTML về. |
| **ViewModel** | `ViewModels/EventSearchViewModel.cs` | Lưu trữ bộ lọc người dùng chọn và danh sách kết quả sự kiện hiển thị. |
| **DTOs** | `EventSearchDTO.cs`, `EventCardDTO.cs` | Trung gian truyền dữ liệu sạch giữa PL và BLL, không cho tầng ngoài chọc thẳng vào Entity. |
| **Service (BLL)** | `Services/SearchService.cs` | Logic filter động, phân trang DB, sắp xếp theo tên/ngày/độ phổ biến. |
| **Repository (DAL)** | `Repositories/EventRepository.cs` | Thực hiện câu query EF Core có nạp sẵn liên kết để tránh N+1 Query. |

---

## 🚀 Giải Thích Từng Bước Code Chạy Thực Tế (Kèm Comment Giải Thích)

### Bước 1: Trình duyệt gửi request (fetch AJAX ngầm)
Khi người dùng tương tác với bộ lọc trên `Index.cshtml`, hàm JS `submitInstant()` thu thập dữ liệu form và gửi ngầm lên URL trang chủ `/` hoặc `/Index`:
```javascript
function submitInstant() {
    // Đọc tất cả giá trị nhập/chọn từ thẻ form có ID searchForm
    const formData = new FormData(form);
    const params = new URLSearchParams();
    
    // Chỉ lấy những tham số có giá trị thực tế, bỏ qua những cái rỗng để URL trông gọn gàng
    for (const [key, value] of formData.entries()) {
        if (value) params.append(key, value);
    }
    
    // Tạo đường dẫn mới chứa bộ lọc mới (ví dụ: /Index?SearchVm.Keyword=NET)
    const newUrl = `${window.location.pathname}?${params.toString()}`;
    
    // Đổi link trên thanh URL của trình duyệt để sinh viên copy/bookmark được, nhưng không làm tải lại trang
    window.history.pushState({ path: newUrl }, '', newUrl);

    // Bắt đầu gọi fetch AJAX ngầm gửi yêu cầu lên server
    fetch(newUrl)
        .then(response => response.text()) // Nhận chuỗi HTML trả về
        .then(html => {
            // Dùng DOMParser để dịch chuỗi HTML thành một cây DOM ảo trong bộ nhớ
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            
            // Tìm và ghi đè danh sách sự kiện mới thay thế cho danh sách sự kiện cũ trên UI
            document.getElementById('resultsWrapper').innerHTML = doc.getElementById('resultsWrapper').innerHTML;
            
            // Vì danh sách sự kiện mới có các nút chuyển trang mới, ta phải gọi hàm này để gán lại sự kiện click
            bindPaginationLinks();
        });
}
```

### Bước 2: Nhận request và nạp dữ liệu tại PageModel
Trong `Pages/Index.cshtml.cs`, nhờ thuộc tính `SupportsGet = true`, toàn bộ param trên URL được bind tự động vào biến `SearchVm`. Hàm `OnGetAsync` xử lý:
```csharp
// Đánh dấu để ASP.NET Core tự động trích xuất các tham số từ query string URL bỏ vô ViewModel này
[BindProperty(SupportsGet = true)]
public EventSearchViewModel SearchVm { get; set; } = new();

public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
{
    if (!ModelState.IsValid) return Page();

    // 1. Kiểm tra bookmark nếu sinh viên đã đăng nhập và là Student để đánh dấu ngôi sao yêu thích trên Card
    if (CurrentStudentId.HasValue && User.IsInRole("Student"))
    {
        BookmarkedEventIds = await _context.Bookmarks
            .Where(b => b.StudentId == CurrentStudentId.Value)
            .Select(b => b.EventId)
            .ToListAsync(cancellationToken);
    }

    // 2. Load danh mục và tag để hiển thị ra dropdown và danh sách checkbox lọc
    SearchVm.Categories = (await _categoryRepo.GetAllAsync(cancellationToken)).ToList();
    SearchVm.Tags = (await _tagRepo.GetAllAsync(cancellationToken)).ToList();

    // 3. Ánh xạ sang đối tượng DTO để truyền dữ liệu sạch xuống tầng BLL, tránh circular dependency
    var searchDto = new EventSearchDTO {
        Keyword = SearchVm.Keyword,
        CategoryId = SearchVm.CategoryId,
        TagIds = SearchVm.TagIds,
        TimeFilter = SearchVm.TimeFilter,
        StartDate = SearchVm.StartDate,
        EndDate = SearchVm.EndDate,
        PageNumber = SearchVm.PageNumber,
        SortBy = SearchVm.SortBy
    };

    // 4. Gọi Service xử lý logic nghiệp vụ lọc và phân trang ở BLL
    var (items, totalCount) = await _searchService.SearchEventsAsync(searchDto, cancellationToken);

    // 5. Gán kết quả nhận được vào ViewModel để đẩy ra View hiển thị lên màn hình
    SearchVm.Results = items;
    SearchVm.TotalCount = totalCount;

    return Page();
}
```

### Bước 3: Build query nạp sẵn chống N+1 ở DAL
Trong `EventRepository.cs`, phương thức `BuildSearchQuery` trả về một `IQueryable` đã nạp sẵn (Eager Loading) toàn bộ dữ liệu liên quan:
```csharp
public IQueryable<Event> BuildSearchQuery()
{
    // BuildSearchQuery chỉ trả về IQueryable (câu lệnh SQL dự kiến) chứ chưa chạy SQL xuống Database
    return _dbSet
        .AsNoTracking() // Dùng AsNoTracking vì trang Explore chỉ đọc, không sửa đổi gì, tắt tracking giúp EF chạy rất nhanh
        .Include(e => e.Venue) // JOIN bảng Venues để lấy sẵn VenueName
        .Include(e => e.Organizer) // JOIN bảng Users lấy tên người tổ chức sự kiện
        .Include(e => e.Bookings) // JOIN bảng Bookings để đếm lượng vé đã đặt
        .Include(e => e.EventTags).ThenInclude(et => et.Tag); // JOIN bảng trung gian nạp kèm Tag để hiển thị badge tag trên card
}
```

### Bước 4: Chain lọc động & phân trang ở BLL
Trong `SearchService.cs`, logic nối câu truy vấn `Where` và thực thi lấy dữ liệu trang hiện tại:
```csharp
public async Task<(List<EventCardDTO> Items, int TotalCount)> SearchEventsAsync(EventSearchDTO searchDto, CancellationToken cancellationToken)
{
    var query = _eventRepo.BuildSearchQuery(); // Lấy bộ khung query JOIN tối ưu

    // Luôn luôn chỉ hiện các sự kiện đã được duyệt công bố (Status là Published)
    query = query.Where(e => e.Status == "Published");

    // Chỉ lọc từ khóa (Keywords) nếu người dùng có gõ từ khóa thực tế vào ô tìm kiếm
    if (!string.IsNullOrEmpty(searchDto.Keyword))
        query = query.Where(e => e.Title.Contains(searchDto.Keyword) || e.Description.Contains(searchDto.Keyword));

    // Lọc Multi-tag bằng logic OR: Chỉ cần sự kiện dính ít nhất 1 tag trong list TagIds là hiển thị
    if (searchDto.TagIds.Count > 0)
        query = query.Where(e => e.EventTags.Any(et => searchDto.TagIds.Contains(et.TagId)));

    // Chạy câu SQL COUNT(*) xuống DB để lấy tổng số bản ghi khớp lọc trước khi phân trang
    var totalCount = await query.CountAsync(cancellationToken);

    // Tính toán phân trang và chạy câu SQL SELECT OFFSET/FETCH lấy đúng 9 sự kiện của trang đó lên RAM
    var skip = (searchDto.PageNumber - 1) * EventSearchDTO.PageSize;
    var events = await query.Skip(skip).Take(EventSearchDTO.PageSize).ToListAsync(cancellationToken);

    // Dùng AutoMapper để map danh sách Entities sang DTO gọn nhẹ để chuyển trả về PageModel
    var items = _mapper.Map<List<EventCardDTO>>(events);

    return (items, totalCount);
}
```
