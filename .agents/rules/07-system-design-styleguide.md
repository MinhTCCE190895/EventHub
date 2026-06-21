# Luật Thiết kế Hệ thống & Giao diện (System Design & UI Style Guide Rules)

Quy chuẩn thiết kế giao diện (UI Style Guide) và cấu trúc UI thống nhất cho dự án **EventHub**. Tất cả các Agent AI khi thực hiện sửa đổi hoặc sinh mới code HTML/CSS/Razor Pages/Blazor trong dự án bắt buộc phải tuân thủ nghiêm ngặt các quy tắc dưới đây.

---

## 1. DESIGN SYSTEM & CSS TOKENS
Mọi style phải dựa trên các CSS Custom Properties được khai báo tập trung tại `:root` trong `site.css`. TUYỆT ĐỐI không hardcode mã màu hoặc giá trị shadow/radius tùy tiện.

| CSS Variable | Value | Usage / Semantic Meaning |
| :--- | :--- | :--- |
| `--primary` | `#2563eb` | Royal Blue - Màu chủ đạo, nút chính, active links |
| `--primary-hover` | `#1d4ed8` | Trạng thái hover của nút/phần tử chủ đạo |
| `--secondary` | `#d97706` | Amber - Màu nhấn cho cảnh báo, trạng thái đặc biệt |
| `--secondary-hover`| `#b45309` | Trạng thái hover của màu nhấn |
| `--bg-app` | `#f8fafc` | Nền chung của ứng dụng |
| `--bg-card` | `#ffffff` | Nền của cards, containers, sidebars |
| `--text-main` | `#0f172a` | Màu chữ nội dung/tiêu đề chính (slate-900) |
| `--text-muted` | `#64748b` | Màu chữ phụ, chú thích, metadata (slate-500) |
| `--border-color` | `#f1f5f9` | Đường viền phân cách siêu nhẹ |
| `--radius-lg` | `16px` | Bo góc cho Cards, Search Containers |
| `--radius-md` | `12px` | Bo góc cho Buttons, Form inputs, Sidebar links |
| `--shadow-soft` | `0 4px 20px 0 rgba(0,0,0,0.03)` | Bóng đổ nhẹ cho khối lớn, search bar |

---

## 2. BỐ CỤC TRANG CHUẨN (GLOBAL PAGE STRUCTURE)
Các trang chức năng thông thường thuộc Razor Pages phải tuân thủ cấu trúc phân cấp DOM sau để đảm bảo hoạt động mượt mà của sidebar và khả năng hiển thị responsive:

```html
<div class="app-container">
  <!-- Sidebar Panel -->
  <aside class="app-sidebar">
    <a href="/" class="sidebar-brand">
      <i class="bi bi-calendar-event"></i> <span>EventHub</span>
    </a>
    <nav class="sidebar-nav">
      <!-- Active link must have the 'active' class -->
      <a href="/Index" class="sidebar-link active">
        <i class="bi bi-house"></i> <span>Trang chủ</span>
      </a>
      <a href="/Bookmarks" class="sidebar-link">
        <i class="bi bi-bookmark"></i> <span>Đã lưu</span>
      </a>
    </nav>
    <div class="sidebar-footer">&copy; 2026 EventHub.</div>
  </aside>

  <!-- Main Content Viewport -->
  <main class="app-content">
    <div class="content-body">
      <!-- Minimal Header -->
      <div class="page-header-minimal">
        <span class="page-badge-minimal">Category/Badge Name</span>
        <h1>Page Title</h1>
      </div>
      <!-- PAGE CONTENT HERE -->
    </div>
    <footer class="content-footer">Hệ thống quản lý sự kiện UniEvent Hub</footer>
  </main>
</div>
```

---

## 3. ĐẶC TẢ SỰ KIỆN & QUY TẮC LOGIC DYNAMIC STYLING

### 3.1. Thẻ sự kiện `.card-minimal`
Giao diện thẻ sự kiện hiển thị danh sách (Grid/List) phải tuân thủ DOM sau:
```html
<div class="card-minimal [is-past]">
  <button type="button" class="btn-bookmark-floating" data-event-id="EVENT_ID">
    <i class="bi bi-bookmark"></i>
  </button>
  <div class="card-minimal-img-wrapper">
    <img src="IMAGE_URL" class="card-minimal-img" alt="Event Title" />
    <span class="card-badge-overlay [status-upcoming | status-ongoing | status-past]">Trạng thái</span>
  </div>
  <div class="card-minimal-body">
    <div class="mb-2"><a href="#" class="badge-tag-premium">Tag Name</a></div>
    <h3 class="card-minimal-title">Tiêu đề sự kiện (Tối đa 2 dòng)</h3>
    <div class="card-group-minimal">
      <div class="card-minimal-meta"><i class="bi bi-calendar"></i><span>Date</span></div>
      <div class="card-minimal-meta [seats-urgency-low | seats-urgency-soldout]">
        <i class="bi bi-people"></i><span>Số chỗ</span>
      </div>
    </div>
    <div class="card-action-row-minimal">
      <a href="/Events/Details?id=EVENT_ID" class="btn-outline-minimal">
        <span>Xem chi tiết</span> <i class="bi bi-arrow-right"></i>
      </a>
    </div>
  </div>
</div>
```

### 3.2. Logic gán CSS Class theo dữ liệu sự kiện:
1. **Trạng thái kết thúc (`.is-past`)**: Nếu `Event.EndDate < DateTime.Now`, bắt buộc thêm class `.is-past` vào `.card-minimal`.
2. **Badge trạng thái diễn ra (`.card-badge-overlay`)**:
   - `Event.StartDate > DateTime.Now` $\rightarrow$ Class: `status-upcoming` (Sắp diễn ra)
   - `Event.StartDate <= DateTime.Now <= Event.EndDate` $\rightarrow$ Class: `status-ongoing` (Đang diễn ra)
   - `Event.EndDate < DateTime.Now` $\rightarrow$ Class: `status-past` (Đã kết thúc)
3. **Mức độ khẩn cấp số lượng vé (`seats-urgency`)**:
   - Số chỗ còn lại $\le 0$ $\rightarrow$ Class: `seats-urgency-soldout` (Text: `Hết vé`)
   - $0 <$ Số chỗ còn lại $\le 5$ $\rightarrow$ Class: `seats-urgency-low` (Text: `Còn [N] chỗ`)
   - Còn lại $> 5$ $\rightarrow$ Giữ class `.card-minimal-meta` chuẩn (Text: `[N] chỗ`)

---

## 4. QUY TẮC LINH HOẠT VÀ TỰ DO TÙY BIẾN (DESIGN FLEXIBILITY)
- **CSS Isolation**: Không ghi đè tùy tiện các style đặc thù lên `site.css`. Sử dụng tính năng CSS Isolation của Razor Page (`[PageName].cshtml.css`) cho các style riêng lẻ của từng trang. Thừa hưởng các CSS variables từ `site.css`.
- **Layout Adaptability**: Cho phép tự do lựa chọn bố cục phù hợp với nghiệp vụ (Grid View cho tìm kiếm, Split-Pane cho booking vé, hoặc Hero-Banner cho chi tiết sự kiện).
- **Component Tùy biến**: Được phép thay đổi cấu trúc/thành phần bên trong `.card-minimal` hoặc tạo cấu trúc card mới cho các tính năng đặc thù, miễn là giữ vững các giá trị token về bo góc (`--radius-lg`), bóng đổ (`--shadow-soft`) để không làm mất đi tính thẩm mỹ nhất quán của dự án.

---

## 5. ĐỘ TƯƠNG PHẢN MÀU SẮC & TRẢI NGHIỆM ĐỌC (COLOR CONTRAST)
- **Trên nền sáng (`#ffffff`, `--bg-app`)**: Bắt buộc dùng `--text-main` (`#0f172a`) cho chữ đọc chính. Không dùng màu xám quá nhạt gây mờ mắt.
- **Màu chữ trên nút màu nổi**: Chữ trên nút có nền `--primary` hoặc `--secondary` bắt buộc phải là màu trắng (`#ffffff`).
- **Màu Badge/Alert**: Khi dùng màu nền pastel nhạt, text bên trong phải dùng tone màu đậm của chính màu đó (ví dụ: nền xanh nhạt `rgba(37,99,235,0.08)` đi với chữ màu đậm `--primary`).
- **Giới hạn Opacity**: Các trạng thái mờ đi (như `.is-past`) không được phép giảm `opacity` xuống dưới `0.7`.

---

## 6. NGUYÊN TẮC "GOOD TASTE" & TRÁNH "RÁC" GIAO DIỆN (ANTI-SLOP)
- **Tự tinh chỉnh giá trị**: Không rập khuôn sử dụng thông số mặc định thô cứng của Bootstrap. Sử dụng các biến token đã thiết kế sẵn.
- **Khoảng trắng phóng khoáng**: Thiết lập điểm nhấn thị giác rõ rệt thông qua padding/margin rộng rãi.
- **Tối giản chuyển động**: Các hiệu ứng hover, translate chỉ được sử dụng ở mức độ mượt mà, tinh tế và trực tiếp hỗ trợ trải nghiệm điều hướng của người dùng. Tránh các hiệu ứng chuyển màu lòe loẹt hoặc nhấp nháy làm mỏi mắt.
- **Không sử dụng Icon Emoji**: TUYỆT ĐỐI không sử dụng bất kỳ ký tự emoji nào làm icon trang trí trên toàn bộ giao diện người dùng (ví dụ: 🎟️, 📊, 🚀, 📈, 🗣️, 🏢, 📚, ⚙️, 🔒, 🚫, ✍️, ⭐, 🌟). Chỉ sử dụng Bootstrap Icons (`<i class="bi bi-*"></i>`) hoặc CSS icons chuẩn khi thực sự cần thiết.
