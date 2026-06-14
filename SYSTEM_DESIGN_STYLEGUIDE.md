# SYSTEM DESIGN & UI STYLE GUIDE (AI-Agent Friendly)
This document serves as the single source of truth (SSOT) for UI development in the **EventHub** project. It contains strict specifications, CSS classes, DOM hierarchies, and dynamic styling rules to ensure AI agents can generate consistent, pixel-perfect frontend pages.

---

## 1. DESIGN SYSTEM & CSS TOKENS
All styles must rely on the following CSS Custom Properties defined in `site.css`. Do **not** hardcode values.

| CSS Variable | Value | Usage / Semantic Meaning |
| :--- | :--- | :--- |
| `--primary` | `#2563eb` | Brand color, main buttons, active links, primary elements (Royal Blue) |
| `--primary-hover` | `#1d4ed8` | State hover for primary buttons/elements |
| `--secondary` | `#d97706` | Warning, accent highlights (Amber) |
| `--secondary-hover`| `#b45309` | Hover state for secondary elements |
| `--bg-app` | `#f8fafc` | Application global body background |
| `--bg-card` | `#ffffff` | Background for cards, containers, sidebars, header |
| `--text-main` | `#0f172a` | Primary text color (slate-900) |
| `--text-muted` | `#64748b` | Subtitles, captions, metadata icons/text (slate-500) |
| `--border-color` | `#f1f5f9` | Dividers, borders between cards/lists (slate-100) |
| `--sidebar-bg` | `#ffffff` | Background color of left side navigation |
| `--sidebar-width` | `260px` | Width of the left sidebar navigation |
| `--radius-lg` | `16px` | Border-radius for Cards, Search containers |
| `--radius-md` | `12px` | Border-radius for Buttons, Form inputs, Sidebar links |
| `--shadow-soft` | `0 4px 20px 0 rgba(0,0,0,0.03)` | Box shadow for content cards and search bars |
| `--shadow-card` | `0 10px 30px -10px rgba(37,99,235,0.04), 0 1px 3px rgba(0,0,0,0.02)` | Box shadow for floating items/cards |

---

## 2. GLOBAL PAGE STRUCTURE (DOM HIERARCHY)
All Razor Pages (`.cshtml`) must follow this exact shell structure to support responsive layouts and sidebar positioning.

```html
<div class="app-container">
  <!-- Sidebar Panel -->
  <aside class="app-sidebar">
    <a href="/" class="sidebar-brand">
      <i class="bi bi-calendar-event"></i>
      <span>EventHub</span>
    </a>
    <nav class="sidebar-nav">
      <!-- Active link must have the 'active' class -->
      <a href="/Index" class="sidebar-link active">
        <i class="bi bi-house"></i>
        <span>Trang chủ</span>
      </a>
      <a href="/Bookmarks" class="sidebar-link">
        <i class="bi bi-bookmark"></i>
        <span>Đã lưu</span>
      </a>
    </nav>
    <div class="sidebar-footer">
      &copy; 2026 EventHub.
    </div>
  </aside>

  <!-- Main Content Viewport -->
  <main class="app-content">
    <div class="content-body">
      <!-- Minimal Header -->
      <div class="page-header-minimal">
        <span class="page-badge-minimal">Category / Section Name</span>
        <h1>Page Title</h1>
      </div>

      <!-- PAGE CONTENT GOES HERE -->

    </div>
    <footer class="content-footer">
      Hệ thống quản lý sự kiện UniEvent Hub
    </footer>
  </main>
</div>
```

---

## 3. UI COMPONENTS SPECIFICATION

### 3.1. Event Card Component (`.card-minimal`)
This card is used to display an individual event in a grid or list layout.

#### DOM Structure:
```html
<div class="card-minimal [is-past]">
  <!-- Bookmark Button: top-right corner of wrapper. Toggle bi-bookmark-fill or bi-bookmark -->
  <button type="button" class="btn-bookmark-floating" data-event-id="EVENT_ID">
    <i class="bi bi-bookmark"></i>
  </button>

  <div class="card-minimal-img-wrapper">
    <img src="IMAGE_URL" class="card-minimal-img" alt="Event Title" />
    
    <!-- Status Badge Overlay: top-left corner of wrapper. Must use correct status subclass -->
    <span class="card-badge-overlay [status-upcoming | status-ongoing | status-past]">
      [Sắp diễn ra | Đang diễn ra | Đã kết thúc]
    </span>
  </div>

  <div class="card-minimal-body">
    <div class="mb-2">
      <!-- Premium Tag for Category -->
      <a href="/Index?selectedTags=TAG_ID" class="badge-tag-premium">Tag Name</a>
    </div>
    
    <h3 class="card-minimal-title" title="Full Title">Event Title (Truncates to max 2 lines)</h3>
    
    <div class="card-group-minimal">
      <div class="card-minimal-meta">
        <i class="bi bi-calendar"></i>
        <span>Formatted Date</span>
      </div>
      <!-- Quota seats urgency class -->
      <div class="card-minimal-meta [seats-urgency-low | seats-urgency-soldout]">
        <i class="bi bi-people"></i>
        <span>Seats Left / Sold Out</span>
      </div>
    </div>

    <div class="card-action-row-minimal">
      <a href="/Events/Details?id=EVENT_ID" class="btn-outline-minimal">
        <span>Xem chi tiết</span>
        <i class="bi bi-arrow-right"></i>
      </a>
    </div>
  </div>
</div>
```

#### Dynamic Styling Logic Rules:
1. **`.is-past`**: If `Event.EndDate` is in the past, add this class to `.card-minimal`.
   - **Visual Effects**: Image grayscale `100%`, opacity `0.75`, border `dashed`.
2. **Badge Status Rules**:
   - `Event.StartDate > DateTime.Now` $\rightarrow$ Class: `status-upcoming`, Text: `Sắp diễn ra`.
   - `Event.StartDate <= DateTime.Now <= Event.EndDate` $\rightarrow$ Class: `status-ongoing`, Text: `Đang diễn ra`.
   - `Event.EndDate < DateTime.Now` $\rightarrow$ Class: `status-past`, Text: `Đã kết thúc`.
3. **Seats Urgency Rules**:
   - `Event.Capacity - Event.RegisteredCount <= 0` $\rightarrow$ Add class `seats-urgency-soldout` to `.card-minimal-meta`. Text: `Hết vé`.
   - `0 < Event.Capacity - Event.RegisteredCount <= 5` $\rightarrow$ Add class `seats-urgency-low` to `.card-minimal-meta`. Text: `Còn [N] chỗ`.
   - Otherwise $\rightarrow$ Use standard `card-minimal-meta` with text `[N] chỗ`.

---

### 3.2. Search and Filter Panel (`.search-card-minimal`)
Container containing search bars, select inputs, and filter tags.

#### DOM Structure:
```html
<form method="get" class="search-card-minimal">
  <div class="row g-3">
    <!-- Search Query Input -->
    <div class="col-md-4">
      <label class="form-label-minimal" for="SearchQuery">Từ khóa</label>
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-search"></i></span>
        <input type="text" id="SearchQuery" name="SearchQuery" class="form-control-minimal" placeholder="Tìm tên sự kiện..." value="CURRENT_QUERY" />
      </div>
    </div>
    
    <!-- Select Dropdown -->
    <div class="col-md-3">
      <label class="form-label-minimal" for="SortBy">Sắp xếp</label>
      <select id="SortBy" name="SortBy" class="form-select-minimal">
        <option value="date_asc">Ngày gần nhất</option>
        <option value="seats_desc">Còn nhiều chỗ</option>
      </select>
    </div>

    <!-- Submit Button & Filters -->
    <div class="col-md-2 d-flex align-items-end">
      <button type="submit" class="btn-primary-minimal w-100">Tìm kiếm</button>
    </div>
  </div>

  <!-- Tag Badges Selection Row -->
  <div class="mt-3">
    <label class="form-label-minimal">Chủ đề</label>
    <div class="d-flex flex-wrap gap-2">
      <!-- Repeat for each Tag -->
      <label class="tag-badge-minimal">
        <input type="checkbox" name="selectedTags" value="TAG_ID" class="tag-checkbox-minimal" [checked] onchange="this.form.submit()" />
        <span class="tag-label-minimal">Tag Name</span>
      </label>
    </div>
  </div>
</form>
```

---

### 3.3. Skeleton Loader (`.skeleton-card`)
Use as a placeholder while asynchronous actions (like loading card updates via API) are in progress.

```html
<div class="skeleton-card">
  <div class="skeleton-image"></div>
  <div class="skeleton-body">
    <div class="skeleton-line title"></div>
    <div class="skeleton-line text"></div>
    <div class="skeleton-line meta"></div>
    <div class="skeleton-line btn"></div>
  </div>
</div>
```

---

### 3.4. Pagination (`.pagination-minimal`)
Follow Bootstrap pagination override scheme but use standard minimalist styling.

```html
<nav aria-label="Page navigation">
  <ul class="pagination pagination-minimal justify-content-center">
    <li class="page-item [disabled]">
      <a class="page-link" href="?page=1" aria-label="Previous">
        <span aria-hidden="true">&laquo;</span>
      </a>
    </li>
    <li class="page-item active"><a class="page-link" href="?page=1">1</a></li>
    <li class="page-item"><a class="page-link" href="?page=2">2</a></li>
    <li class="page-item">
      <a class="page-link" href="?page=2" aria-label="Next">
        <span aria-hidden="true">&raquo;</span>
      </a>
    </li>
  </ul>
</nav>
```

---

## 4. DESIGN GUIDELINES & INTERACTION SPECIFICATION

### 4.1. Transitions and States
- **Sidebar Hover**: Elements matching `.sidebar-link:hover i` must apply a right translate animation (`transform: translateX(2px)`) with `transition: transform 0.2s ease`.
- **Card Hover**: `.card-minimal:hover` must shift upward (`transform: translateY(-6px)`) and increase shadow (`box-shadow: 0px 12px 28px rgba(0, 0, 0, 0.06)`). Inner image `.card-minimal-img` must scale up (`transform: scale(1.04)`). All these transitions must use `cubic-bezier(0.4, 0, 0.2, 1)`.
- **List view Override**: Grid cards that are displayed in horizontal list items should use class `.card-minimal-list` to disable the `translateY` and scale transitions.

### 4.2. Mobile Layout Toggle (Viewport Width <= 992px)
- The navigation sidebar `.app-sidebar` will translateX offscreen (`-100%`).
- Toggle navigation visibility by appending/removing the class `.show` to `.app-sidebar`.

---

## 5. DESIGN FLEXIBILITY & EXTENSION RULES (For Special Features)
To prevent rigid constraints and support page-specific variations (e.g., Event details, Ticket booking, Admin Dashboard), AI agents should follow these guidelines for custom features:

### 5.1. Page-Specific CSS (CSS Isolation)
- Do **not** pollute `site.css` with single-use styles.
- Create page-specific styles using Razor CSS isolation: `[PageName].cshtml.css`.
- Inherit properties from `site.css` by using the CSS custom variables (e.g., `var(--primary)`).

### 5.2. Layout Adaptability
The main layout (`.app-container`) structure is flexible:
- **Default Grid View**: For listings and search interfaces.
- **Split-Pane Layout**: For details + list views (e.g., ticket booking with side summary). Use `.d-flex.flex-column.flex-lg-row.gap-4` to handle responsive side panels.
- **Hero-Banner Layout**: For detail views (e.g., Event Details). The header banner may stretch full-width of `.content-body` with an image background and overlay blur filters.

### 5.3. Customizing Components
- **Card Variants**: If a feature requires a card showing less or more data (e.g., miniature ticket cards, admin list cards), agents can adapt the `.card-minimal` structure.
- Always retain structural constants: `--radius-lg` for card outer shell, `--radius-md` for buttons/inner controls, and `--shadow-soft` for cards to keep visual consistency.
- Sub-components such as `.card-minimal-body` can be styled differently or replaced inside custom card structures.

### 5.4. Accessibility & Color Contrast Rules (Anti-Fade Guidelines)
To prevent text from blending into backgrounds or having poor readability, agents must follow these contrast rules:
1. **Background & Text Matching**:
   - **Light Backgrounds (`#ffffff`, `--bg-app`)**: Must only use `--text-main` (`#0f172a`, high contrast) for body text and headers. Use `--text-muted` (`#64748b`) *only* for secondary metadata/labels. Never use light gray or soft blue for readable text.
   - **Primary Color Background (`--primary`)**: Text inside must be pure white (`#ffffff`). Never use gray or semi-transparent blue.
2. **Badge & Status Alerts**:
   - When utilizing light pastel backgrounds for tags/badges, the text inside must use the dark primary shade to maintain contrast:
     - *Light Blue Badge*: Background `rgba(37, 99, 235, 0.08)` $\rightarrow$ Text color must be `--primary` (`#2563eb`).
     - *Light Amber Badge*: Background `rgba(217, 119, 6, 0.08)` $\rightarrow$ Text color must be `--secondary` (`#d97706`).
3. **State Overlay (Opacity Limits)**:
   - For inactive or past states (e.g., `.is-past`), never reduce opacity below `0.7` for text containers to ensure they remain legible for users with visual impairments.

---

## 6. INTEGRATED PROJECT UI/UX DESIGN RULES
All features must comply with the global UI/UX guidelines to maintain architectural and visual consistency across subsystems.

### 6.1. Subsystem Technology Stack
1. **Blazor Subsystem (Registration Dashboard & Feedback Analytics)**:
   - Must build components exclusively using **MudBlazor** UI library.
   - For analytics dashboards, utilize MudBlazor charts or highly styled custom components fitting the dashboard's design language.
2. **MVC Subsystem (Identity) & Razor Pages Subsystem (Location & Event)**:
   - Standard integration is based on **Bootstrap** utility wrapper combined with the custom variables and components defined in `site.css`.

### 6.2. The "Good Taste" Design Guidelines (Aesthetics & Layout)
1. **Ditch Default Bootstrap Styles**:
   - Avoid using harsh, default Bootstrap rounded corners (`border-radius`) or plain primary buttons. Always override or extend using the minimalist tokens: `--radius-lg`, `--radius-md`, and customized shadows (`--shadow-soft`).
2. **Visual Hierarchy & Whitespace (Breathing Space)**:
   - Make components legible by styling with generous padding and margins (e.g., `.content-body` padding `2.5rem 3rem`, card body padding `1.5rem`).
   - Leave clean whitespace to establish clear visual pathways for the user's eyes.
3. **Anti-Slop (No UI Clutter)**:
   - Avoid excessive colors, redundant borders, or distracting flickering animations.
   - Animations (such as card translates, hover states) must be subtle, smooth, and configured using `transition: all 0.2s ease` or similar. They should always assist the user's navigation rather than create visual noise.



