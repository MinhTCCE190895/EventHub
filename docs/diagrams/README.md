# Kho Biểu Đồ PlantUML theo Use Case (UC) - Dự án EventHub

Thư mục này lưu trữ các biểu đồ thiết kế hệ thống chuẩn **PlantUML (`.puml`)** được phân chia cấu trúc rõ ràng theo từng Use Case (UC) để phục vụ cho tài liệu Software Design Specification (SDS) và rà soát kiến trúc.

---

## Cấu trúc thư mục

```text
docs/diagrams/
├── README.md                              ← Tài liệu quy chuẩn & hướng dẫn (Tiếng Việt)
├── UC01_Login/                            ← Use Case 1: Đăng nhập hệ thống (Login)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
│   ├── CommunicationDiagram.puml          ← Biểu đồ giao tiếp (III.1.2 - Tiếng Anh)
│   └── StateDiagram.puml                  ← Biểu đồ trạng thái phiên làm việc (III.2 - Tiếng Anh)
├── UC02_Logout/                           ← Use Case 2: Đăng xuất hệ thống (Logout)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
│   ├── CommunicationDiagram.puml          ← Biểu đồ giao tiếp (III.1.2 - Tiếng Anh)
├── UC03_Register/                         ← Use Case 3: Đăng ký tài khoản mới (Register)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC04_BrowseAndDiscoverEvents/          ← Use Case 4: Duyệt và Khám phá Sự kiện (Browse & Discover Events)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC05_SearchAndFilterEvents/            ← Use Case 5: Tìm kiếm và Lọc Sự kiện (Search & Filter Events)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC06_MonitorEventWeather/              ← Use Case 6: Theo dõi Thời tiết Sự kiện (Monitor Event Weather)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC07_ManageEventBookmarks/             ← Use Case 7: Quản lý Sự kiện đã Lưu (Manage Event Bookmarks)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự chính (ToggleBookmark - Thêm/Xóa)
│   └── SequenceDiagram_ViewList.puml      ← Biểu đồ tuần tự xem danh sách sự kiện đã lưu
├── UC08_ExecuteTicketRegistration/        ← Use Case 8: Thực hiện Đăng ký Vé (Execute Ticket Registration)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC09_ParticipateInLiveQA/              ← Use Case 9: Tham gia Hỏi đáp Trực tiếp (Participate in Live Q&A)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC10_FollowOrganizerProfiles/          ← Use Case 10: Theo dõi Đơn vị Tổ chức (Follow Organizer Profiles)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC11_SubmitEventProposals/             ← Use Case 11: Gửi Đề xuất Ý tưởng Sự kiện (Submit Event Proposals)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC12_SubmitEventFeedback/              ← Use Case 12: Gửi Đánh giá Sự kiện (Submit Event Feedback)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC13_ViewFeedbackList/                 ← Use Case 13: Xem Danh sách Đánh giá (View Feedback List)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC14_ManageEventContent/               ← Use Case 14: Quản lý Nội dung Sự kiện (Manage Event Content)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự chính (Tạo sự kiện & khởi tạo EventReminder tự động)
│   ├── SequenceDiagram_List.puml          ← Biểu đồ tuần tự Xem danh sách sự kiện theo Organizer/Admin
│   ├── SequenceDiagram_Update.puml        ← Biểu đồ tuần tự Cập nhật thông tin sự kiện & lịch nhắc nhở
│   └── SequenceDiagram_DeleteAndStatus.puml ← Biểu đồ tuần tự Đổi trạng thái (SignalR broadcast) & Xóa sự kiện
├── UC15_MaintainEvent/                    ← Use Case 15: Bảo trì & Cập nhật Sự kiện (Maintain Event)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự chính (Cập nhật sự kiện)
│   └── SequenceDiagram_DeleteAndStatus.puml ← Biểu đồ tuần tự Xóa sự kiện & kiểm tra ràng buộc vé
├── UC16_ConfigureVenueLogistics/          ← Use Case 16: Cấu hình Địa điểm & Sức chứa (Configure Venue Logistics)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự chính (Cập nhật sức chứa & kiểm tra MaxCapacity)
│   ├── SequenceDiagram_Create.puml        ← Biểu đồ tuần tự Thêm mới địa điểm
│   ├── SequenceDiagram_List.puml          ← Biểu đồ tuần tự Xem danh sách địa điểm
│   └── SequenceDiagram_Delete.puml        ← Biểu đồ tuần tự Xóa địa điểm & kiểm tra ràng buộc sự kiện
├── UC17_ReviewEventProposals/             ← Use Case 17: Duyệt Đề xuất Ý tưởng Sự kiện (Review Event Proposals)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC18_MonitorRealTimeDashboard/         ← Use Case 18: Giám sát Bảng điều khiển Thời gian thực (Monitor Real-time Dashboard)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
├── UC19_ManageSystemAccess/               ← Use Case 19: Quản lý Quyền truy cập Hệ thống (Manage System Access)
│   ├── SequenceDiagram.puml               ← Biểu đồ tuần tự chính (Khóa / Xóa tài khoản - Soft vs Hard Delete)
│   └── SequenceDiagram_ListUsers.puml     ← Biểu đồ tuần tự chi tiết Xem danh sách người dùng hệ thống paged
├── UC20_EnforceAccountSecurity/           ← Use Case 20: Thực thi Chính sách Bảo mật Tài khoản (Enforce Account Security)
│   └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
└── UC21_AnalyzePerformanceMetrics/        ← Use Case 21: Phân tích Chỉ số Hiệu năng & Đánh giá (Analyze Performance Metrics)
    └── SequenceDiagram.puml               ← Biểu đồ tuần tự (III.1.1 - Tiếng Anh)
```

---

## Quy chuẩn Vẽ Biểu Đồ Chuẩn Hóa (Đã thống nhất cho toàn dự án)

Để đảm bảo tính nhất quán cao nhất và phản ánh 100% mọi Class C# có tham gia xử lý dữ liệu cho toàn bộ các Use Case tiếp theo (`UC02`, `UC03`...), các biểu đồ PlantUML BẮT BUỘC tuân theo 6 quy tắc sau:

1. **Ngôn ngữ:**
   - Nội dung bên trong các biểu đồ (`.puml`): **Tiếng Anh (English)**.
   - Tài liệu chỉ mục và hướng dẫn (`README.md`): **Tiếng Việt (Vietnamese)**.

2. **Định danh thành phần tham gia (Participants / Objects - Phản ánh 100% C# Class):**
   - Chỉ sử dụng **Tên Class thực thi đơn giản (Simple Concrete Class Names)**, không dùng tiền tố tên biến và không dùng tên Interface.
   - **Client/Browser:** Ghi rõ tên View Class / Component cụ thể (Ví dụ: `LoginView`, `RegisterView`).
   - **Controller:** Ghi rõ tên Controller xử lý (Ví dụ: `AccountController`, `EventsController`).
   - **Service:** Ghi rõ tên Service Class (Ví dụ: `UserService`, `EventService`).
   - **Repository:** Ghi rõ tên Concrete Repository (Ví dụ: `BaseRepository<User>`, `EventRepository`).
   - **ORM DbContext:** Ghi rõ tên DbContext (Ví dụ: `AppDbContext`).
   - **Database:** Chỉ ghi tên đơn giản là `Database`.

3. **Luồng xử lý dữ liệu chuẩn C# & Chỉ vẽ cột có tham gia xử lý:**
   - Luồng kết nối dữ liệu khi chạm đến Database sẽ đi qua 6 cột chuẩn:
     `View -> Controller -> Service -> Repository -> AppDbContext -> Database`
   - **Quy tắc lọc cột:** **Chỉ vẽ những cột (`Class` / `Layer`) THỰC SỰ có tham gia vào luồng tin nhắn (`Message flow`) của Use Case đó.** Cột nào không tham gia (Ví dụ: `Logout` không gọi `Service`, `Repository`, `AppDbContext`, `Database`) thì **KHÔNG VẼ** để biểu đồ tối giản, sạch sẽ và không có cột trống.
   - **Bắt buộc có Activation Bar (`activate` / `deactivate`) cho mọi cột xuất hiện trên biểu đồ:**
     + `View` (Client): `activate` ngay khi bắt đầu gửi request và `deactivate` khi nhận được response/redirect cuối cùng.
     + `Database` (DB) (nếu có): `activate` khi nhận lệnh truy vấn SQL từ `AppDbContext` và `deactivate` ngay sau khi trả dữ liệu về `AppDbContext`.
     + Các layer trung gian (`Controller`, `Service`, `Repository`, `AppDbContext`): `activate` khi được gọi hàm và `deactivate` sau khi hoàn tất xử lý.
     + **Lời gọi nội bộ (`Self-Call / Internal Message Nested Activation Box`):** Khi một object gọi hàm nội bộ của chính nó (`X -> X : InternalMethod()`), **bắt buộc** phải đặt cặp `activate X` và `deactivate X` ngay phía dưới lời gọi đó để PlantUML vẽ hộp xử lý lồng (`Nested Activation Box`) nổi trên đường sinh mệnh (`Lifeline`) theo đúng hình mẫu:
       ```plantuml
       Controller -> Controller : Check ModelState.IsValid
       activate Controller
       deactivate Controller
       ```


4. **Không vẽ Box gom Layer & Tránh tham số Deprecated:**
   - Không sử dụng các khối `box "Layer" ... end box` để tập trung tối đa vào luồng tin nhắn giữa các class C#.
   - Không sử dụng các tham số cũ đã bị deprecate (`skinparam ParticipantPadding`, `skinparam BoxPadding`) để tránh cảnh báo trên IDE. Chỉ giữ lại tham số hiển thị tối giản chuẩn:
     ```plantuml
     skinparam monochrome true
     skinparam shadowing false
     ```

5. **Quản lý Cookie & Session:**
   - Không vẽ cột `Auth` riêng. Mọi thao tác xác thực, ghi nhận Session (`SignInAsync`, `SignOutAsync`) được thể hiện dưới dạng **Self-message** tại Controller:
     ```plantuml
     Controller -> Controller : HttpContext.SignInAsync(CookieScheme, principal, authProps)
     ```

6. **Đồng bộ Sequence (III.1.1) và Communication (III.1.2 - Chuẩn Figure II-7):**
   - Luồng tuần tự (`SequenceDiagram.puml`) sử dụng `autonumber`, `monochrome true` và đầy đủ Activation Bar cho các cột tham gia.
   - Luồng giao tiếp (`CommunicationDiagram.puml`) BẮT BUỘC tuân theo notation chuẩn **Figure II-7**:
     + **Bắt buộc có từ khóa `allowmixing` ngay dưới `@startuml`:** Vì PlantUML mặc định coi `actor` thuộc Sequence/UseCase diagram và `object` thuộc Object diagram. Từ khóa `allowmixing` giúp trình biên dịch cho phép vẽ đồng thời `actor` và `object` trên cùng một canvas mà không bị lỗi `Syntax Error`:
       ```plantuml
       @startuml Xxx_CommunicationDiagram
       allowmixing
       ```
     + Có biểu tượng `Actor` (Ví dụ `actor "Guest" as Actor`) tương tác với View đầu tiên.
     + Các object khai báo dạng hình chữ nhật có dấu 2 chấm phía trước tên class: `object ": ClassName" as Alias`.
     + Sử dụng chuẩn đen trắng tối giản (`skinparam monochrome true`, `skinparam shadowing false`) không tô màu để đồng bộ hiển thị sạch sẽ khi in ấn:
       ```plantuml
       skinparam monochrome true
       skinparam shadowing false
       ```
     + **Bố trí gom gọn theo lưới (`Grid Layout`) & Đường thẳng, nhãn chữ không chồng chéo:** Để đường nối đi thẳng trực tiếp (không cong uốn lượn) và hình vẽ gọn gàng (nhãn chữ có thể đè lên đường nối để tiết kiệm diện tích, nhưng các cụm nhãn **tuyệt đối không được chồng đè lên nhau**), TUYỆT ĐỐI KHÔNG dùng `skinparam Padding` (tránh cảnh báo deprecation) và BẮT BUỘC cấu hình:
       ```plantuml
       skinparam linetype polyline
       skinparam nodesep 65
       skinparam ranksep 65

       together {
           object ": LoginView" as View
           object ": UserService" as Service
       }
       together {
           actor "Guest" as Actor
           object ": AccountController" as Controller
       }

       View -[hidden]right- Service
       Actor -[hidden]right- Controller

       Actor -up- View : 1: Input >
       View -down- Controller : 1.1: POST >
       ```
     + **Chỉ hiển thị thông điệp yêu cầu chuyển tiếp (`Forward Messages / Calls Only - Chuẩn Figure II-7`):** Khác với Sequence Diagram cần vẽ rõ mũi tên trả về (`-->`), trong UML Communication Diagram, **kết quả trả về đồng bộ (`synchronous return`) được ngầm định (`implicit`)** sau khi lời gọi hàm thực thi xong. Việc vẽ thêm nhãn trả về (`4: Return <`, `5: HTTP 302 <`) vừa làm hình bị chồng chéo chữ, vừa sai lệch với hình mẫu. Do đó BẮT BUỘC chỉ hiển thị các thông điệp gọi đi/yêu cầu (`forward calls`) kèm số thứ tự (`1`, `1.1`, `1.1.1`...) và ký hiệu hướng truyền `>` hoặc `<` (`1: message >`).

---

## Hướng dẫn xem / Export file `.puml`

### 1. Preview trực tiếp trên VS Code / Cursor IDE
- Cài đặt extension **PlantUML** (của tác giả *jebbs*).
- Mở file `.puml` bất kỳ $\rightarrow$ Nhấn `Alt + D` (hoặc `Cmd + Option + D` trên macOS) để preview ngay trong IDE.

### 2. Xuất ra ảnh PNG / SVG bằng CLI
```powershell
java -jar plantuml.jar -tsvg docs/diagrams/UC01_Login/*.puml
```
