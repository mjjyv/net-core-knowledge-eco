# Lộ Trình 15 Phase Nền Tảng (15-Phase Foundation Roadmap)

Lộ trình phát triển hệ thống Backend **The Ecosystem** được phân rã thành **15 Phase nền tảng**, bám sát và hiện thực hóa tri thức từ 15 bài học trong giáo trình `acad_net-core-slide` (từ Buổi 1 đến Buổi 15).

> [!IMPORTANT]
> Các phase nâng cao (như tích hợp AI Semantic Kernel RAG, WebRTC Audio Mesh, Tauri v2 IPC, và Advanced CRDT Engine) sẽ được mở rộng và đề cập sau khi hoàn thành toàn diện 15 phase nền tảng này.
> Dự án tập trung tuyệt đối vào **Backend** và các **tài liệu hướng dẫn kiểm thử toàn diện dành cho Dev**. Frontend chỉ thực hiện khi có yêu cầu cụ thể.

---

## 1. Bảng Tổng Quan 15 Phase Nền Tảng

| Phase | Bài giảng tương ứng | Tên Phase & Trọng tâm Kỹ thuật | Trọng tâm Triển khai trong The Ecosystem |
|:---:|:---:|---|---|
| **Phase 01** | Buổi 01 | Cấu Trúc Solution & Domain Entities | Khởi tạo Solution .NET 8 Clean Architecture, cấu hình dự án, xây dựng các Base Entities và Enums cốt lõi. |
| **Phase 02** | Buổi 02 | Web API Pipeline & Native DI | Thiết lập ASP.NET Core Web API, cấu hình Middleware Pipeline cơ bản và đăng ký Service Lifetimes trong DI. |
| **Phase 03** | Buổi 03 | Controllers & RESTful Routing | Xây dựng Controllers cho hệ thống Cây tri thức (Taxonomy: Topics, Aspects, Categories), áp dụng Model Binding và Action Results. |
| **Phase 04** | Buổi 04 | DTOs, Model Validation & Swagger | Tạo các DTOs chuẩn, kiểm soát dữ liệu đầu vào bằng Data Annotations và thiết lập Swagger UI phục vụ kiểm thử. |
| **Phase 05** | Buổi 05 | EF Core: DbContext & Migrations | Cấu hình `ApplicationDbContext`, Connection String, chạy Code First Migrations tạo bảng dữ liệu trên Database. |
| **Phase 06** | Buổi 06 | CRUD Bài Viết, LINQ & Quan Hệ Dữ Liệu | Hoàn thiện API CRUD cho Bài viết Markdown (`Articles`), thiết lập quan hệ 1-N, N-N, tối ưu truy vấn LINQ với `Include` và `AsNoTracking`. |
| **Phase 07** | Buổi 07 | N-Tier Architecture, Repository & Unit of Work | Tái cấu trúc theo N-Tier/Clean Architecture, triển khai Generic Repository & Unit of Work, tích hợp Object Mapping và Unit Tests. |
| **Phase 08** | Buổi 08 | Authentication với JWT Bearer | Xây dựng phân hệ xác thực tài khoản: Đăng ký, Đăng nhập, băm mật khẩu, cấp phát JWT Bearer Token, Refresh Token, tích hợp Swagger Bearer Auth. |
| **Phase 09** | Buổi 09 | Authorization: Roles, Claims & Policies | Phân quyền 3 vai trò (`HOST`, `PRESENTER`, `AUDIENCE`), kiểm soát quyền hạn bằng Claims và Custom Policies trên các API nhạy cảm. |
| **Phase 10** | Buổi 10 | File Upload, Configuration & Options Pattern | API Upload và quản lý tệp đính kèm (ảnh minh họa, file markdown) an toàn; quản trị môi trường và cấu hình qua Options Pattern (`IOptions<T>`). |
| **Phase 11** | Buổi 11 | Advanced Querying: Pagination, Filtering & Sorting | Hiện thực phân trang chuẩn `PagedResult<T>`, lọc theo danh mục/trạng thái, tìm kiếm toàn văn và sắp xếp động trên danh sách bài viết. |
| **Phase 12** | Buổi 12 | Global Exception Handling & Logging | Custom Middleware bắt lỗi tập trung chuẩn RFC 7807 `ProblemDetails`, Structured Logging với ILogger, Request Timing Middleware. |
| **Phase 13** | Buổi 13 | Swagger Nâng Cao, CORS & Docker Container | Cấu hình CORS bảo mật, bổ sung XML Documentation cho Swagger, viết `Dockerfile` đa tầng tối ưu hóa cho .NET 8. |
| **Phase 14** | Buổi 14 | Tích Hợp Hệ Thống & Realtime SignalR Hub | Ghép nối toàn bộ các phân hệ, hiện thực `CollaborationHub` và `ClassroomHub` thời gian thực, API bundle xuất bản cho Desktop PKM. |
| **Phase 15** | Buổi 15 | Kiểm Thử Toàn Diện, Seed Data & Nghiệm Thu | Xây dựng bộ test E2E (Happy & Negative Path), Seed Data mẫu hoàn chỉnh, kiểm tra hiệu năng, lập biên bản nghiệm thu và kịch bản demo đồ án. |

---

## 2. Chi Tiết Mục Tiêu & Checklist Từng Phase

### Phase 01: Khởi Tạo Cấu Trúc Solution & Thực Thể Miền (Domain Entities)
- **Mục tiêu:** Xây dựng khung nền móng vững chắc của Solution theo Clean Architecture.
- **Checklist triển khai:**
  - [ ] Khởi tạo Solution `TheEcosystem.sln` bằng .NET 8 CLI.
  - [ ] Tạo các project con: `Ecosystem.Domain`, `Ecosystem.Application`, `Ecosystem.Infrastructure`, `Ecosystem.WebAPI`, `Ecosystem.UnitTests`.
  - [ ] Thiết lập Project References theo quy tắc phụ thuộc 1 chiều của Clean Architecture.
  - [ ] Triển khai `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`).
  - [ ] Triển khai các Domain Entities: `Topic`, `Aspect`, `Category`, `Article`, `User`, `ClassroomSession`.
  - [ ] Triển khai các Enums: `UserRole`, `ArticleStatus`, `SessionStatus`.
  - [ ] Build solution thành công không lỗi biên dịch.

### Phase 02: Thiết Lập Web API Pipeline & Native Dependency Injection
- **Mục tiêu:** Cấu hình điểm tiếp nhận HTTP và khung tiêm phụ thuộc trong ASP.NET Core.
- **Checklist triển khai:**
  - [ ] Cấu hình Kestrel Server và HTTP request pipeline trong `Program.cs`.
  - [ ] Minh họa và cấu hình 3 vòng đời Service trong DI Container: `Transient`, `Scoped`, `Singleton`.
  - [ ] Tạo HealthCheck endpoint (`/health`) kiểm tra trạng thái hoạt động của API.
  - [ ] Viết Extension methods để đăng ký services gọn gàng (`ServiceCollectionExtensions`).

### Phase 03: Thiết Kế Controllers & RESTful Routing Cho Cây Tri Thức
- **Mục tiêu:** Xây dựng hệ thống Controller tiếp nhận CRUD dữ liệu cơ bản cho Taxonomy.
- **Checklist triển khai:**
  - [ ] Tạo `TaxonomyController` với các action quản lý Topics, Aspects, Categories.
  - [ ] Cấu hình Attribute Routing chuẩn RESTful (`/api/v1/taxonomy/...`).
  - [ ] Sử dụng các HTTP Methods: GET, POST, PUT, DELETE đúng ngữ nghĩa.
  - [ ] Minh chứng Model Binding (`[FromBody]`, `[FromRoute]`, `[FromQuery]`).
  - [ ] Trả về các Action Results chuẩn (`Ok`, `Created`, `NotFound`, `BadRequest`, `NoContent`).

### Phase 04: Data Transfer Objects (DTO), Validation & Swagger UI
- **Mục tiêu:** Tách rời mô hình dữ liệu nội bộ và biên giới ngoại vi, kiểm soát dữ liệu đầu vào.
- **Checklist triển khai:**
  - [ ] Tạo các DTOs cho Topic, Aspect, Category, Article.
  - [ ] Áp dụng Data Annotations validation (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]`).
  - [ ] Cấu hình Swagger/OpenAPI hiển thị schema mô tả các DTOs.
  - [ ] Kiểm thử tự động kích hoạt ModelState validation và trả về lỗi 400 Bad Request chi tiết.

### Phase 05: Tích Hợp Entity Framework Core, DbContext & Migrations
- **Mục tiêu:** Thiết lập tầng lưu trữ dữ liệu quan hệ với EF Core Code First.
- **Checklist triển khai:**
  - [ ] Cài đặt các gói NuGet EF Core (`Microsoft.EntityFrameworkCore`, `Design`, `SqlServer`/`Npgsql`).
  - [ ] Tạo lớp `ApplicationDbContext` kế thừa `DbContext`.
  - [ ] Cấu hình Connection String an toàn trong `appsettings.json` và `appsettings.Development.json`.
  - [ ] Khởi tạo bản Migration đầu tiên (`InitialCreate`) sinh toàn bộ bảng dữ liệu.
  - [ ] Cập nhật migration vào cơ sở dữ liệu (`dotnet ef database update`).

### Phase 06: CRUD Nghiệp Vụ Bài Viết Markdown, Quan Hệ Dữ Liệu & LINQ
- **Mục tiêu:** Hoàn thiện nghiệp vụ cốt lõi quản lý bài viết Markdown và quan hệ dữ liệu.
- **Checklist triển khai:**
  - [ ] Sử dụng Fluent API định nghĩa quan hệ phân cấp: `Topic -> Aspect -> Category -> Article`.
  - [ ] Hiện thực API CRUD đầy đủ cho Article (`Create`, `GetById`, `GetList`, `Update`, `Delete`).
  - [ ] Tối ưu hóa truy vấn LINQ: sử dụng `.Include()` nạp dữ liệu liên kết và `.AsNoTracking()` cho các truy vấn chỉ đọc.
  - [ ] Xử lý slug bài viết tự động từ tiêu đề và đảm bảo tính duy nhất.

### Phase 07: Kiến Trúc N-Tier, Repository Pattern, Unit of Work & Unit Tests
- **Mục tiêu:** Chuẩn hóa phân tách tầng nghiệp vụ, trừu tượng hóa truy cập dữ liệu và kiểm thử độc lập.
- **Checklist triển khai:**
  - [ ] Tạo `IRepository<T>` và `GenericRepository<T>`.
  - [ ] Tạo `IUnitOfWork` và `UnitOfWork` quản lý transaction và Commit đồng nhất.
  - [ ] Chuyển toàn bộ logic nghiệp vụ từ Controller sang `ArticleService`, `TaxonomyService`.
  - [ ] Triển khai chuyển đổi Entity <-> DTO (AutoMapper hoặc Manual Mapping).
  - [ ] Viết bộ Unit Tests (xUnit + Moq) kiểm thử độc lập cho tầng Service mà không cần Database thật.

### Phase 08: Xác Thực Người Dùng Với JWT Bearer Token
- **Mục tiêu:** Xây dựng hệ thống đăng nhập, cấp phát và xác minh JWT token an toàn.
- **Checklist triển khai:**
  - [ ] Tạo thực thể `User` và `RefreshToken`.
  - [ ] Hiện thực `PasswordHasher` băm mật khẩu một chiều an toàn.
  - [ ] Hiện thực `JwtTokenService` sinh JWT Bearer token chứa Claims (Id, Email, Role, FullName).
  - [ ] Tạo API `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh-token`.
  - [ ] Cấu hình Authentication Middleware và Swagger UI hỗ trợ Authorize Bearer Token.

### Phase 09: Phân Quyền Truy Cập (Role-Based, Claims-Based & Policy-Based)
- **Mục tiêu:** Bảo vệ tài nguyên hệ thống theo 3 cấp độ vai trò: `HOST`, `PRESENTER`, `AUDIENCE`.
- **Checklist triển khai:**
  - [ ] Phân định vai trò: `HOST` (toàn quyền), `PRESENTER` (tạo/sửa bài viết), `AUDIENCE` (chỉ đọc & bình luận).
  - [ ] Áp dụng `[Authorize(Roles = "...")]` trên các endpoint tạo/sửa/xóa bài viết và danh mục.
  - [ ] Thiết lập Policy-based Authorization (ví dụ: `RequireAuthorOrHost` khi chỉnh sửa bài viết).
  - [ ] Kiểm thử các kịch bản: 401 Unauthorized khi thiếu token, 403 Forbidden khi sai quyền.

### Phase 10: Tải Tệp Đính Kèm, Quản Lý Cấu Hình & Options Pattern
- **Mục tiêu:** Xử lý file đính kèm an toàn và tổ chức cấu hình ứng dụng chuẩn hóa.
- **Checklist triển khai:**
  - [ ] Xây dựng `FilesController` hỗ trợ upload ảnh và file `.md`.
  - [ ] Kiểm tra tính hợp lệ của tệp: phần mở rộng cho phép (whitelist: `.png, .jpg, .md`), kích thước tối đa (5MB).
  - [ ] Tạo class cấu hình strongly-typed (ví dụ: `FileStorageSettings`, `JwtSettings`).
  - [ ] Đăng ký và sử dụng `IOptions<T>` / `IOptionsSnapshot<T>` để inject cấu hình vào services.

### Phase 11: Nâng Cao Truy Vấn: Phân Trang, Bộ Lọc, Tìm Kiếm & Sắp Xếp
- **Mục tiêu:** Tối ưu hóa hiệu năng và trải nghiệm khi tải danh sách bài viết quy mô lớn.
- **Checklist triển khai:**
  - [ ] Tạo lớp `PagedResult<T>` và `PaginationParams`.
  - [ ] Triển khai phân trang Server-side bằng `.Skip()` và `.Take()`.
  - [ ] Hiện thực bộ lọc theo Category, Aspect, Status (`Published`/`Draft`).
  - [ ] Hiện thực tìm kiếm bài viết theo từ khóa trong Tiêu đề hoặc Tóm tắt.
  - [ ] Hiện thực sắp xếp động theo ngày tạo (`CreatedAt`), số lượt xem (`ViewCount`), tiêu đề (`Title`).

### Phase 12: Xử Lý Ngoại Lệ Tập Trung & Ghi Log Có Cấu Trúc
- **Mục tiêu:** Nâng cao tính ổn định và khả năng quan sát lỗi hệ thống theo chuẩn công nghiệp.
- **Checklist triển khai:**
  - [ ] Viết `GlobalExceptionMiddleware` bắt toàn bộ ngoại lệ chưa được xử lý.
  - [ ] Chuẩn hóa payload phản hồi lỗi theo đặc tả RFC 7807 (`ProblemDetails`).
  - [ ] Cấu hình Structured Logging bằng `ILogger` ghi log chi tiết lỗi kèm Request ID.
  - [ ] Viết `RequestTimingMiddleware` đo lường thời gian đáp ứng và cảnh báo các request chậm (> 500ms).

### Phase 13: Hoàn Thiện Swagger Documentation, CORS & Đóng Gói Docker
- **Mục tiêu:** Chuẩn hóa giao tiếp bên ngoài và sẵn sàng triển khai trên môi trường container.
- **Checklist triển khai:**
  - [ ] Bật sinh XML Documentation file và tích hợp vào Swagger UI.
  - [ ] Cấu hình chính sách CORS phân định rõ môi trường Development (cho phép localhost) và Production.
  - [ ] Viết `Dockerfile` đa tầng (multi-stage build) tối ưu dung lượng và bảo mật container chạy user phi đặc quyền.
  - [ ] Viết file `docker-compose.yml` khởi chạy đồng thời WebAPI và cơ sở dữ liệu.

### Phase 14: Tích Hợp Toàn Diện & Kênh Thời Gian Thực SignalR
- **Mục tiêu:** Ghép nối toàn bộ các phân hệ của The Ecosystem thành khối thống nhất.
- **Checklist triển khai:**
  - [ ] Triển khai `CollaborationHub` (`/hubs/collaboration`) quản lý trạng thái con trỏ và tin nhắn nổi.
  - [ ] Triển khai `ClassroomHub` (`/hubs/classroom`) quản lý điều phối phòng học.
  - [ ] Hoàn thiện API xuất bản gói dữ liệu `ecosystem://bundle` (`GET /api/v1/articles/{id}/bundle`).
  - [ ] Rà soát toàn bộ Solution, xử lý triệt để mọi cảnh báo biên dịch (Compiler Warnings).

### Phase 15: Kiểm Thử Toàn Diện, Chuẩn Bị Dữ Liệu Mẫu & Nghiệm Thu
- **Mục tiêu:** Đảm bảo độ tin cậy tuyệt đối của hệ thống trước khi nghiệm thu đồ án.
- **Checklist triển khai:**
  - [ ] Xây dựng kịch bản kiểm thử Happy Path và Negative Path cho toàn bộ các API.
  - [ ] Xuất bản bộ Postman Collection E2E kèm Environment variables hoàn chỉnh.
  - [ ] Khởi tạo Seed Data mẫu chất lượng cao (Users, Cây tri thức 4 cấp, Bài viết đầy đủ Markdown/KaTeX/Mermaid).
  - [ ] Lập biên bản nghiệm thu kỹ thuật tổng thể trong `reference-docs/plan/`.
  - [ ] Chuẩn bị kịch bản demo và tài liệu hướng dẫn kiểm thử cho Dev trong `reference-docs/guides/`.

---

## 3. Quy Trình Vận Hành Nghiệm Thu & Kiểm Thử Cho Mỗi Phase

Khi thực thi bất kỳ phase nào trong 15 phase trên:
1. **Lập kế hoạch & rà soát checklist:** Đảm bảo nắm rõ các yêu cầu kỹ thuật của bài học tương ứng.
2. **Triển khai mã nguồn Backend:** Viết mã nguồn sạch, tuân thủ Clean Architecture và Clean Code.
3. **Biên dịch & kiểm thử nội bộ:** `dotnet build` và `dotnet test` phải chạy thành công không có lỗi.
4. **Viết tài liệu hướng dẫn kiểm thử cho Dev:** Đặt vào [`reference-docs/guides/PHASE-XX-TEST-GUIDE.md`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides).
5. **Nghiệm thu văn bản:** Lập biên bản nghiệm thu phase vào [`reference-docs/plan/PHASE-XX-ACCEPTANCE.md`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan).
6. **Cam kết & Đẩy mã nguồn:** Thực hiện commit theo Conventional Commits và **push git ngay lập tức** lên remote repository.
