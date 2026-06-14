# FE-03 Search & Filter Events — Luồng hoạt động

> Đọc cái này trước khi bị hỏi. Giải thích được từng bước là okay.

---

## Tổng quan

Sinh viên vào trang `/Explore`, nhập keyword / chọn filter, bấm Tìm.  
Trang hiện danh sách event dạng card, có phân trang, giữ filter khi chuyển trang.

---

## Sơ đồ luồng

```
User bấm Tìm
    │
    ▼
[URL] /Explore?SearchVm.Keyword=NET&SearchVm.CategoryId=1&...
    │
    ▼
[PL] Index.cshtml.cs — OnGetAsync()
    ├─ Bind URL params → SearchVm (ViewModel)
    ├─ Load Categories + Tags song song (Task.WhenAll)
    ├─ Map SearchVm → EventSearchDTO
    │
    ▼
[BLL] SearchService — SearchEventsAsync(dto)
    ├─ Gọi EventRepository.BuildSearchQuery()  →  IQueryable (chưa chạy SQL)
    ├─ Chain thêm Where() theo từng filter
    ├─ CountAsync()   →  SQL #1: đếm tổng kết quả
    ├─ ToListAsync()  →  SQL #2: lấy 9 item trang hiện tại
    ├─ Map List<Event> → List<EventCardDTO>
    │
    ▼
[PL] PageModel nhận (items, totalCount)
    ├─ Gán vào SearchVm.Results
    │
    ▼
[View] Index.cshtml render
    ├─ Card grid (banner, tên, ngày, địa điểm, tags)
    ├─ Pagination (giữ nguyên filter params)
    └─ Empty state nếu không có kết quả
```

---

## 5 layer và nhiệm vụ của từng cái

| Layer | Project | File chính | Làm gì |
|---|---|---|---|
| **PL — View** | RazorPages | `Pages/Explore/Index.cshtml` | Render form filter, card grid, pagination |
| **PL — PageModel** | RazorPages | `Pages/Explore/Index.cshtml.cs` | Nhận request, gọi service, trả data về view |
| **DTO** | BusinessObjects | `DTOs/EventSearchDTO.cs`, `EventCardDTO.cs` | Trung gian truyền dữ liệu giữa PL và BLL |
| **BLL** | BLL | `Services/SearchService.cs` | Logic filter, phân trang, map Entity → DTO |
| **DAL** | DAL | `Repositories/EventRepository.cs` | Truy vấn DB, Include quan hệ |

---

## Giải thích chi tiết từng bước

### Bước 1 — Form submit (GET request)

Form dùng `method="get"` nên filter params đi vào URL thay vì body.  
Lý do: URL có thể bookmark, share, và khi chuyển trang vẫn giữ được filter.

```
/Explore?SearchVm.Keyword=NET&SearchVm.CategoryId=1&SearchVm.TagIds=2&SearchVm.PageNumber=2
```

Razor Pages tự bind URL → `SearchVm` nhờ `[BindProperty(SupportsGet = true)]`.

---

### Bước 2 — PageModel OnGetAsync

```csharp
// 1. Validate — keyword không được quá 200 ký tự
if (!ModelState.IsValid) return Page();

// 2. Load dropdown/checkbox data song song — không chờ nhau
var categoriesTask = _categoryRepo.GetAllAsync();
var tagsTask = _tagRepo.GetAllAsync();
await Task.WhenAll(categoriesTask, tagsTask);

// 3. Map ViewModel → DTO để gửi xuống BLL
var searchDto = new EventSearchDTO {
    Keyword = SearchVm.Keyword,
    CategoryId = SearchVm.CategoryId,
    TagIds = SearchVm.TagIds,
    TimeFilter = SearchVm.TimeFilter,
    PageNumber = SearchVm.PageNumber
};

// 4. Gọi service
var (items, totalCount) = await _searchService.SearchEventsAsync(searchDto);

// 5. Nhét kết quả vào ViewModel để View đọc
SearchVm.Results = items;
SearchVm.TotalCount = totalCount;
```

**Tại sao phải map sang DTO thay vì truyền thẳng ViewModel?**  
BLL không được biết ViewModel của web — nếu biết thì BLL phải reference RazorPages → circular dependency. DTO đặt ở `BusinessObjects` để cả hai tầng đều dùng được.

---

### Bước 3 — EventRepository.BuildSearchQuery()

```csharp
return _dbSet
    .AsNoTracking()                    // chỉ đọc, không cần EF theo dõi thay đổi
    .Include(e => e.Venue)             // join bảng Venues
    .Include(e => e.EventTags)
        .ThenInclude(et => et.Tag)     // join EventTags → Tags
    .Include(e => e.EventCategories);  // join EventCategories
```

**Trả về `IQueryable` — chưa gọi DB.** Đây chỉ là bản thiết kế của câu query.  
BLL sẽ tiếp tục thêm `Where()` vào, sau đó mới execute.

**Tại sao Include ngay từ đây?**  
Nếu không Include, sau này đọc `e.Venue.Name` thì EF sẽ phát thêm 1 query/event.  
Có 9 event = 9 query thừa. Gọi là **N+1 problem**. Include 1 lần = 1 SQL JOIN duy nhất.

---

### Bước 4 — SearchService: chain Where()

Đây là phần core nhất. Mỗi filter chỉ được append vào query **khi có giá trị hợp lệ**:

```csharp
var query = _eventRepo.BuildSearchQuery();

// Luôn filter — chỉ hiện Published
query = query.Where(e => e.Status == "Published");

// Keyword — chỉ filter nếu không rỗng
if (!string.IsNullOrEmpty(keyword))
    query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));

// Category — chỉ filter nếu có chọn
if (categoryId.HasValue && categoryId > 0)
    query = query.Where(e => e.EventCategories.Any(ec => ec.CategoryId == categoryId));

// Tags — OR logic: event có ít nhất 1 tag trong danh sách là đủ
if (tagIds.Count > 0)
    query = query.Where(e => e.EventTags.Any(et => tagIds.Contains(et.TagId)));

// TimeFilter
query = timeFilter switch {
    "Upcoming" => query.Where(e => e.StartTime > now),   // chưa bắt đầu
    "Ongoing"  => query.Where(e => e.StartTime <= now && e.EndTime >= now),  // đang diễn ra
    "Past"     => query.Where(e => e.EndTime < now),      // đã xong
    _          => query   // không filter thêm
};
```

**Tại sao dùng `IQueryable` chứ không load hết rồi filter?**  
`IQueryable` = câu query chưa chạy. EF Core gom tất cả `Where()` thành **1 câu SQL** rồi mới gửi xuống DB. Nếu load hết (`ToList()`) rồi mới filter thì kéo toàn bộ dữ liệu về RAM — cực kỳ chậm.

Sau đó execute 2 lần:

```csharp
// SQL #1: SELECT COUNT(*) — đếm tổng để tính số trang
var totalCount = await query.CountAsync();

// SQL #2: SELECT TOP 9 ... ORDER BY StartTime OFFSET skip — lấy đúng trang hiện tại
var events = await query
    .OrderBy(e => e.StartTime)
    .Skip((pageNumber - 1) * 9)
    .Take(9)
    .ToListAsync();
```

---

### Bước 5 — Map Entity → DTO

```csharp
var items = events.Select(e => new EventCardDTO {
    Id        = e.Id,
    Title     = e.Title,
    BannerUrl = e.BannerUrl,
    StartTime = e.StartTime,
    VenueName = e.Venue.Name,   // đọc được vì đã Include ở bước 3
    TagNames  = e.EventTags.Select(et => et.Tag.Name).ToList()
}).ToList();
```

BLL trả về `(List<EventCardDTO>, int TotalCount)` — **không bao giờ trả Entity ra ngoài**.  
Lý do: Entity gắn với DbContext, nếu trả ra ngoài có thể gây lazy loading ngoài ý muốn.

---

### Bước 6 — View render

```html
<!-- Card grid -->
@foreach (var card in Model.SearchVm.Results) {
    <!-- banner, title, starttime, venue, tag badges -->
}

<!-- Pagination — giữ filter params trong URL -->
<a asp-route-SearchVm.Keyword="@Model.SearchVm.Keyword"
   asp-route-SearchVm.PageNumber="@(Model.SearchVm.PageNumber + 1)">
    Sau »
</a>
```

Pagination dùng `asp-route-*` tag helper để tự build URL đúng với filter hiện tại.

---

## Câu hỏi hay bị hỏi

**"Tại sao 2 query CountAsync + ToListAsync mà không gộp 1?"**  
Không thể gộp — `COUNT(*)` và `SELECT data` là 2 mục đích khác nhau. SQL không cho phép trả cả hai trong 1 câu đơn giản.

**"Tại sao `AsNoTracking`?"**  
Trang Explore chỉ đọc, không update. Bỏ tracking = EF không cần giữ snapshot của từng entity trong bộ nhớ = nhanh hơn, ít RAM hơn.

**"Tag filter dùng OR hay AND?"**  
OR. Chọn tag IT và Music = event có IT **hoặc** Music đều hiện. Dùng `Any()` thay vì `All()`.

**"Nếu DB lỗi khi load Category/Tag thì sao?"**  
`try/catch` trong PageModel — log lỗi, giữ `Categories` và `Tags` rỗng, form vẫn render bình thường, chỉ dropdown trống thôi. Không crash trang.

**"Tại sao form dùng GET không dùng POST?"**  
GET params nằm trên URL nên có thể bookmark, copy link, và khi nhấn nút chuyển trang vẫn giữ được filter. POST không có URL nên mỗi lần reload là mất filter.

---

## DTO vs ViewModel — khác nhau chỗ nào

| | DTO (BusinessObjects) | ViewModel (RazorPages) |
|---|---|---|
| Đặt ở đâu | `BusinessObjects/DTOs/` | `RazorPages/ViewModels/` |
| Ai dùng | Cả BLL và PL | Chỉ PL (web) |
| Mục đích | Truyền data giữa các tầng | Chứa data cho 1 trang cụ thể |
| `EventSearchDTO` | Keyword, CategoryId, TagIds, TimeFilter, PageNumber | — |
| `EventSearchViewModel` | — | Gồm input params + Results + Categories + Tags |

`EventSearchViewModel` nhiều hơn vì nó còn chứa `Categories` và `Tags` để render dropdown/checkbox — thứ mà BLL không cần biết.
