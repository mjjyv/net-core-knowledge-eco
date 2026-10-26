# Kế Hoạch Tổng Thể 15 Phase Nền Tảng (Master Implementation Plan)

Tài liệu này xác lập kế hoạch triển khai, bảng checklist chi tiết và quy trình nghiệm thu cho **15 Phase nền tảng** của dự án **The Ecosystem Backend**, bám sát 15 bài học trong giáo trình `acad_net-core-slide`.

> [!NOTE]
> - Dự án tập trung vào **Backend** và **hướng dẫn kiểm thử toàn diện dành cho Developer**.
> - Các phase nâng cao (AI Semantic Kernel, WebRTC Audio Mesh, Tauri IPC, v.v.) sẽ được mở rộng sau khi hoàn thành 15 phase nền tảng.
> - Sau khi triển khai xong mỗi phase, tiến hành nghiệm thu văn bản vào thư mục [`reference-docs/plan/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan) và viết tài liệu hướng dẫn kiểm thử vào [`reference-docs/guides/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides).
> - **Kỷ luật bắt buộc:** Thực hiện `git commit` và `git push` lên remote sau mỗi lần cập nhật mã nguồn.

---

## Bảng Trạng Thái Tiến Độ 15 Phase Nền Tảng

| Phase | Bài giảng slide | Tên Phase & Trọng tâm Kỹ thuật | Trạng thái | Tài liệu Nghiệm thu (`plan/`) | Hướng dẫn Kiểm thử (`guides/`) |
|:---:|:---:|---|:---:|:---:|:---:|
| **01** | Buổi 01 | Solution Setup & Domain Entities | 📝 Ready | `PHASE-01-ACCEPTANCE.md` | `PHASE-01-TEST-GUIDE.md` |
| **02** | Buổi 02 | Web API Pipeline & Native DI | ⏳ Pending | `PHASE-02-ACCEPTANCE.md` | `PHASE-02-TEST-GUIDE.md` |
| **03** | Buổi 03 | Controllers & RESTful Routing | ⏳ Pending | `PHASE-03-ACCEPTANCE.md` | `PHASE-03-TEST-GUIDE.md` |
| **04** | Buổi 04 | DTOs, Model Validation & Swagger | ⏳ Pending | `PHASE-04-ACCEPTANCE.md` | `PHASE-04-TEST-GUIDE.md` |
| **05** | Buổi 05 | EF Core: DbContext & Migrations | ⏳ Pending | `PHASE-05-ACCEPTANCE.md` | `PHASE-05-TEST-GUIDE.md` |
| **06** | Buổi 06 | CRUD Bài Viết, LINQ & Quan Hệ Dữ Liệu | ⏳ Pending | `PHASE-06-ACCEPTANCE.md` | `PHASE-06-TEST-GUIDE.md` |
| **07** | Buổi 07 | N-Tier, Repository & Unit of Work | ⏳ Pending | `PHASE-07-ACCEPTANCE.md` | `PHASE-07-TEST-GUIDE.md` |
| **08** | Buổi 08 | Authentication với JWT Bearer Token | ⏳ Pending | `PHASE-08-ACCEPTANCE.md` | `PHASE-08-TEST-GUIDE.md` |
| **09** | Buổi 09 | Authorization: Roles, Claims & Policies | ⏳ Pending | `PHASE-09-ACCEPTANCE.md` | `PHASE-09-TEST-GUIDE.md` |
| **10** | Buổi 10 | File Upload, Configuration & Options Pattern | ⏳ Pending | `PHASE-10-ACCEPTANCE.md` | `PHASE-10-TEST-GUIDE.md` |
| **11** | Buổi 11 | Pagination, Filtering, Searching & Sorting | ⏳ Pending | `PHASE-11-ACCEPTANCE.md` | `PHASE-11-TEST-GUIDE.md` |
| **12** | Buổi 12 | Global Exception Handling & Logging | ⏳ Pending | `PHASE-12-ACCEPTANCE.md` | `PHASE-12-TEST-GUIDE.md` |
| **13** | Buổi 13 | Swagger Nâng Cao, CORS & Docker Container | ⏳ Pending | `PHASE-13-ACCEPTANCE.md` | `PHASE-13-TEST-GUIDE.md` |
| **14** | Buổi 14 | Tích Hợp Toàn Diện & Realtime SignalR Hub | ⏳ Pending | `PHASE-14-ACCEPTANCE.md` | `PHASE-14-TEST-GUIDE.md` |
| **15** | Buổi 15 | Kiểm Thử Toàn Diện, Seed Data & Nghiệm Thu | ⏳ Pending | `PHASE-15-ACCEPTANCE.md` | `PHASE-15-TEST-GUIDE.md` |

---

## Chi Tiết Checklist Nghiệm Thu Từng Phase

### Phase 01: Khởi Tạo Solution & Domain Entities (Tương ứng Buổi 1)
- [ ] Khởi tạo cấu trúc Solution chuẩn .NET 8 Clean Architecture (`TheEcosystem.sln`).
- [ ] Thiết lập các project: `Ecosystem.Domain`, `Ecosystem.Application`, `Ecosystem.Infrastructure`, `Ecosystem.WebAPI`, `Ecosystem.UnitTests`.
- [ ] Cấu hình Project References tuân thủ nguyên tắc Dependency Inversion.
- [ ] Định nghĩa `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`).
- [ ] Xây dựng Domain Entities: `Topic`, `Aspect`, `Category`, `Article`, `User`, `ClassroomSession`.
- [ ] Xây dựng Domain Enums: `UserRole`, `ArticleStatus`, `SessionStatus`.
- [ ] Biên dịch `dotnet build` đạt 0 lỗi, 0 cảnh báo.
- [ ] Viết tài liệu nghiệm thu `reference-docs/plan/PHASE-01-ACCEPTANCE.md`.
- [ ] Viết hướng dẫn kiểm thử `reference-docs/guides/PHASE-01-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 02: Web API Pipeline & Native Dependency Injection (Tương ứng Buổi 2)
- [ ] Cấu hình Web API application host và Middleware Pipeline trong `Program.cs`.
- [ ] Đăng ký các dịch vụ trong DI container với đủ 3 vòng đời: Transient, Scoped, Singleton.
- [ ] Viết Extension methods cấu hình dịch vụ gọn gàng.
- [ ] Cung cấp HealthCheck endpoint `/health` kiểm tra trạng thái dịch vụ.
- [ ] Viết tài liệu nghiệm thu `PHASE-02-ACCEPTANCE.md` & Test Guide `PHASE-02-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 03: Controllers & RESTful Routing (Tương ứng Buổi 3)
- [ ] Xây dựng `TaxonomyController` quản lý cây tri thức 3 cấp đầu (`Topics`, `Aspects`, `Categories`).
- [ ] Thiết lập chuẩn RESTful routes (`api/v1/taxonomy/...`) và HTTP methods (GET, POST, PUT, DELETE).
- [ ] Thực thi Model Binding từ Body, Route, Query.
- [ ] Trả về `ActionResult<T>` với status code ngữ nghĩa (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`).
- [ ] Viết tài liệu nghiệm thu `PHASE-03-ACCEPTANCE.md` & Test Guide `PHASE-03-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 04: DTOs, Model Validation & Swagger (Tương ứng Buổi 4)
- [ ] Tạo các lớp DTOs tách biệt hoàn toàn với Domain Entities.
- [ ] Áp dụng Data Annotations validation chặt chẽ cho toàn bộ DTOs đầu vào.
- [ ] Bật Swagger OpenAPI tự động sinh tài liệu schema cho DTOs.
- [ ] Xử lý tự động trả về 400 Bad Request kèm chi tiết lỗi khi dữ liệu không hợp lệ.
- [ ] Viết tài liệu nghiệm thu `PHASE-04-ACCEPTANCE.md` & Test Guide `PHASE-04-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 05: EF Core: DbContext, Connection String & Migrations (Tương ứng Buổi 5)
- [ ] Tạo `ApplicationDbContext` tại tầng `Infrastructure`.
- [ ] Đăng ký DbContext vào DI Container và đọc Connection String từ `appsettings.json`.
- [ ] Tạo Migration ban đầu `InitialCreate` cho các thực thể.
- [ ] Thực thi `dotnet ef database update` tạo schema cơ sở dữ liệu hoàn chỉnh.
- [ ] Viết tài liệu nghiệm thu `PHASE-05-ACCEPTANCE.md` & Test Guide `PHASE-05-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 06: CRUD Bài Viết, LINQ & Quan Hệ Dữ Liệu (Tương ứng Buổi 6)
- [ ] Cấu hình Fluent API định nghĩa quan hệ phân cấp: `Topic 1-N Aspect 1-N Category 1-N Article`.
- [ ] Xây dựng đầy đủ các API CRUD cho Bài viết (`ArticlesController`).
- [ ] Áp dụng LINQ tối ưu với `.Include()` cho quan hệ và `.AsNoTracking()` cho truy vấn đọc.
- [ ] Tự động sinh `Slug` chuẩn SEO từ `Title` và kiểm tra trùng lặp.
- [ ] Viết tài liệu nghiệm thu `PHASE-06-ACCEPTANCE.md` & Test Guide `PHASE-06-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 07: N-Tier Architecture, Repository & Unit of Work (Tương ứng Buổi 7)
- [ ] Triển khai `IRepository<T>`, `GenericRepository<T>`, `IUnitOfWork` và `UnitOfWork`.
- [ ] Chuyển toàn bộ logic xử lý từ Controller sang tầng Service (`ArticleService`, `TaxonomyService`).
- [ ] Triển khai mapping tự động hoặc bán tự động giữa Entity và DTO.
- [ ] Viết bộ Unit Tests kiểm thử độc lập cho tầng Service bằng xUnit và Moq.
- [ ] Viết tài liệu nghiệm thu `PHASE-07-ACCEPTANCE.md` & Test Guide `PHASE-07-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 08: Authentication Với JWT Bearer Token (Tương ứng Buổi 8)
- [ ] Xây dựng thực thể `User` và `RefreshToken`.
- [ ] Hiện thực cơ chế băm mật khẩu bảo mật (PasswordHasher).
- [ ] Hiện thực sinh và kiểm chứng JWT Bearer Token (`JwtTokenService`).
- [ ] Xây dựng API: `Register`, `Login`, `RefreshToken`.
- [ ] Cấu hình Swagger hỗ trợ Authorize với JWT Bearer Token.
- [ ] Viết tài liệu nghiệm thu `PHASE-08-ACCEPTANCE.md` & Test Guide `PHASE-08-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 09: Authorization: Roles, Claims & Policies (Tương ứng Buổi 9)
- [ ] Phân định 3 vai trò người dùng: `Host`, `Presenter`, `Audience`.
- [ ] Áp dụng thuộc tính `[Authorize(Roles = "...")]` bảo vệ các endpoint tạo/sửa/xóa nội dung.
- [ ] Xây dựng Policy-based authorization (kiểm tra quyền sở hữu bài viết hoặc quyền Admin).
- [ ] Kiểm thử phản hồi mã lỗi 401 Unauthorized và 403 Forbidden.
- [ ] Viết tài liệu nghiệm thu `PHASE-09-ACCEPTANCE.md` & Test Guide `PHASE-09-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 10: File Upload, Configuration & Options Pattern (Tương ứng Buổi 10)
- [ ] Xây dựng `FilesController` hỗ trợ upload và download file đính kèm.
- [ ] Kiểm tra định dạng (whitelist đuôi tệp và mime-type) và giới hạn dung lượng file (tối đa 5MB).
- [ ] Quản lý cấu hình thông qua Options Pattern (`IOptions<T>`, `IOptionsSnapshot<T>`).
- [ ] Quản lý cấu hình theo môi trường (`Development` vs `Production`).
- [ ] Viết tài liệu nghiệm thu `PHASE-10-ACCEPTANCE.md` & Test Guide `PHASE-10-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 11: Advanced Querying: Pagination, Filtering & Sorting (Tương ứng Buổi 11)
- [ ] Tạo mô hình trả về phân trang `PagedResult<T>` và tham số `PaginationParams`.
- [ ] Thực hiện phân trang cơ sở dữ liệu bằng `.Skip()` và `.Take()`.
- [ ] Hiện thực lọc theo CategoryId, AspectId, Status.
- [ ] Hiện thực tìm kiếm từ khóa trong Title và Summary.
- [ ] Hiện thực sắp xếp động theo các trường (Title, CreatedAt, ViewCount).
- [ ] Viết tài liệu nghiệm thu `PHASE-11-ACCEPTANCE.md` & Test Guide `PHASE-11-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 12: Global Exception Handling & Logging (Tương ứng Buổi 12)
- [ ] Xây dựng `GlobalExceptionMiddleware` bắt mọi ngoại lệ hệ thống chưa xử lý.
- [ ] Định dạng phản hồi lỗi đồng nhất theo chuẩn RFC 7807 (`ProblemDetails`).
- [ ] Cấu hình Structured Logging với `ILogger` ghi log chi tiết có kèm Request ID.
- [ ] Xây dựng `RequestTimingMiddleware` cảnh báo các request có thời gian xử lý lâu.
- [ ] Viết tài liệu nghiệm thu `PHASE-12-ACCEPTANCE.md` & Test Guide `PHASE-12-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 13: Swagger Nâng Cao, CORS & Đóng Gói Docker (Tương ứng Buổi 13)
- [ ] Bật XML Documentation, mô tả chi tiết các tham số và phản hồi của API trên Swagger UI.
- [ ] Cấu hình chính sách CORS bảo mật cho Web Client.
- [ ] Tối ưu hóa cấu hình cho môi trường Production.
- [ ] Viết `Dockerfile` đa tầng tối ưu dung lượng và an toàn bảo mật.
- [ ] Viết tài liệu nghiệm thu `PHASE-13-ACCEPTANCE.md` & Test Guide `PHASE-13-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 14: Tích Hợp Hệ Thống & Realtime SignalR Hub (Tương ứng Buổi 14)
- [ ] Xây dựng `CollaborationHub` (`/hubs/collaboration`) cho tính năng chia sẻ con trỏ và tin nhắn nổi.
- [ ] Xây dựng `ClassroomHub` (`/hubs/classroom`) cho tính năng đồng bộ phiên lớp học.
- [ ] Hoàn thiện API xuất bản gói dữ liệu `ecosystem://bundle` phục vụ đồng bộ Desktop PKM.
- [ ] Rà soát toàn bộ dự án, loại bỏ toàn bộ compiler warnings.
- [ ] Viết tài liệu nghiệm thu `PHASE-14-ACCEPTANCE.md` & Test Guide `PHASE-14-TEST-GUIDE.md`.
- [ ] Push commit lên Git.

### Phase 15: Kiểm Thử Toàn Diện, Seed Data & Nghiệm Thu (Tương ứng Buổi 15)
- [ ] Xây dựng bộ kiểm thử E2E đầy đủ: Happy Path, Negative Path, Phân quyền.
- [ ] Hoàn thiện bộ Postman Collection E2E và tệp Environment.
- [ ] Seed Data đầy đủ, chất lượng cao cho toàn bộ dữ liệu Cây tri thức và bài viết mẫu.
- [ ] Lập hồ sơ nghiệm thu kỹ thuật tổng thể cho dự án.
- [ ] Soạn thảo kịch bản demo và ngân hàng câu hỏi phản biện bảo vệ đồ án.
- [ ] Viết tài liệu nghiệm thu `PHASE-15-ACCEPTANCE.md` & Test Guide `PHASE-15-TEST-GUIDE.md`.
- [ ] Push commit hoàn tất lên Git.
