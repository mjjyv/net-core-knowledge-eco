# Thiết Kế Kiến Trúc Hệ Thống .NET 8 (Clean Architecture)

Tài liệu này đặc tả kiến trúc phần mềm chuẩn hóa cho **The Ecosystem Backend**, áp dụng triết lý **Clean Architecture** (kiến trúc củ hành - Onion / Hexagonal Architecture) trên nền tảng **.NET 8 (C#)**.

---

## 1. Nguyên Tắc Thiết Kế Cốt Lõi

1. **Độc lập về Framework & Giao diện:** Lõi nghiệp vụ (Domain & Application) hoàn toàn không phụ thuộc vào ASP.NET Core Web API, UI hay thư viện bên ngoài.
2. **Quy tắc phụ thuộc một chiều (Dependency Rule):** Mã nguồn chỉ phụ thuộc từ ngoài vào trong:
   $$\text{WebAPI} \longrightarrow \text{Infrastructure} \longrightarrow \text{Application} \longrightarrow \text{Domain}$$
   Tầng `Domain` không tham chiếu bất kỳ project nào khác.
3. **Đảo ngược phụ thuộc (Dependency Inversion Principle - DIP):** Tầng `Application` định nghĩa các giao diện (Interfaces như `IArticleRepository`, `IUnitOfWork`, `IApplicationDbContext`, `ITokenService`). Tầng `Infrastructure` đảm nhận việc hiện thực các interface này.
4. **Nguồn chân lý duy nhất (Single Source of Truth):** Thực thể miền (Domain Entities) quy định trạng thái và quy tắc dữ liệu toàn vẹn của hệ thống.

---

## 2. Cấu Trúc Chi Tiết Các Tầng (Solution Layout)

```plaintext
TheEcosystem.sln
├── src/
│   ├── Ecosystem.Domain/              # Tầng Miền Lõi (Core Domain)
│   ├── Ecosystem.Application/         # Tầng Ứng dụng & Nghiệp vụ (Application Layer)
│   ├── Ecosystem.Infrastructure/      # Tầng Hạ tầng & Tích hợp (Infrastructure Layer)
│   └── Ecosystem.WebAPI/              # Tầng Tiếp nhận Yêu cầu (Presentation Layer)
└── tests/
    ├── Ecosystem.UnitTests/           # Unit Tests (Application & Domain)
    └── Ecosystem.IntegrationTests/    # Integration Tests (WebAPI & Persistence)
```

---

## 3. Đặc Tả Từng Tầng Kỹ Thuật

### 3.1. Tầng `Ecosystem.Domain` (Core Domain)
- **Mục đích:** Chứa các khái niệm nghiệp vụ thuần túy của hệ thống tri thức The Ecosystem.
- **Thành phần:**
  - `Common/`: `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`), `IAggregateRoot`.
  - `Entities/`:
    - `Topic`: Chủ đề lớn của cây tri thức (ví dụ: Lập trình .NET, Khoa học Máy tính).
    - `Aspect`: Khía cạnh chuyên sâu thuộc Topic (ví dụ: Backend Architecture, Realtime Systems).
    - `Category`: Chuyên mục cụ thể thuộc Aspect (ví dụ: ASP.NET Core Web API, EF Core).
    - `Article`: Bài viết tri thức, ghi chú Markdown, siêu dữ liệu (Tags, Backlinks, Trạng thái).
    - `User`: Tài khoản người dùng (Email, PasswordHash, FullName, Role, RefreshTokens).
    - `ClassroomSession`: Phiên lớp học trực tuyến thời gian thực.
  - `Enums/`: `UserRole` (`Host`, `Presenter`, `Audience`), `ArticleStatus` (`Draft`, `Published`, `Archived`), `SessionStatus`.
  - `Exceptions/`: `DomainException`, `EntityNotFoundException`.

### 3.2. Tầng `Ecosystem.Application` (Business Logic & Contracts)
- **Mục đích:** Điều phối các ca sử dụng nghiệp vụ (Use Cases), chuyển đổi dữ liệu và định nghĩa hợp đồng.
- **Thành phần:**
  - `Contracts/`:
    - DTOs cho các phân hệ: `ArticleDto`, `CreateArticleDto`, `UpdateArticleDto`, `TopicDto`, `UserDto`, `LoginRequestDto`, `LoginResponseDto`, `RegisterRequestDto`.
    - DTOs cho Real-time: `UserPresenceDto`, `CursorFloatingMessageDto`, `ArticleBundleDto`.
  - `Common/`:
    - `PagedResult<T>`: Mô hình dữ liệu phân trang chuẩn (`Items`, `PageIndex`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage`).
    - `PaginationParams`: Tham số truy vấn phân trang, tìm kiếm và lọc (`PageNumber`, `PageSize`, `SearchTerm`, `SortBy`, `SortDescending`).
  - `Interfaces/`:
    - `IApplicationDbContext`: Cung cấp các `DbSet<T>` dưới dạng trừu tượng.
    - `IRepository<T>` & `IUnitOfWork`: Hợp đồng cho mẫu thiết kế Repository.
    - `IArticleService`, `IAuthService`, `ITaxonomyService`, `IFileStorageService`.
    - `ITokenService`: Hợp đồng phát hành và xác thực JWT token.
    - `IRealtimeHubContext`: Giao tiếp gửi tin nhắn thời gian thực qua SignalR.
  - `Mappings/`: Cấu hình ánh xạ giữa Entities và DTOs (thủ công hoặc qua Mapper).

### 3.3. Tầng `Ecosystem.Infrastructure` (Data & External Integrations)
- **Mục đích:** Triển khai các chi tiết kỹ thuật công nghệ bên ngoài.
- **Thành phần:**
  - `Persistence/`:
    - `ApplicationDbContext`: Kế thừa `DbContext` của EF Core, quản lý các DbSet.
    - `Configurations/`: Sử dụng Fluent API cấu hình bảng dữ liệu, ràng buộc, chỉ mục (Indexes).
    - `Repositories/`: `GenericRepository<T>`, `ArticleRepository`, `UnitOfWork`.
    - `Migrations/`: Thư mục chứa các tệp Migration của EF Core.
  - `Identity/`:
    - `PasswordHasher`: Hiện thực băm và kiểm tra mật khẩu bằng thuật toán an toàn (PBKDF2/BCrypt/Argon2).
    - `JwtTokenService`: Hiện thực tạo JWT access token kèm claims và refresh token an toàn.
  - `Realtime/`:
    - `CollaborationHub`: SignalR Hub tiếp nhận và phân phối trạng thái con trỏ và tin nhắn nổi.
    - `ClassroomHub`: SignalR Hub quản lý phiên lớp học và quyền điều phối (`Host`, `Presenter`, `Audience`).
  - `Files/`: `LocalFileStorageService` quản lý lưu trữ file đính kèm trên thư mục đĩa cục bộ.
  - `BackgroundJobs/`: `IngestionWorker` xử lý hàng đợi tác vụ nền (Channels / HostedService).

### 3.4. Tầng `Ecosystem.WebAPI` (Presentation & HTTP Delivery)
- **Mục đích:** Tiếp nhận các HTTP Requests, kích hoạt Middleware pipeline, điều hướng đến Controller thích hợp và trả về HTTP Responses chuẩn.
- **Thành phần:**
  - `Controllers/`:
    - `AuthController`: Đăng ký, đăng nhập, cấp lại token, lấy thông tin cá nhân.
    - `TaxonomyController`: Quản lý danh mục 4 cấp (Topics, Aspects, Categories).
    - `ArticlesController`: CRUD bài viết, tìm kiếm, lọc, phân trang, xuất bản gói `bundle`.
    - `FilesController`: Upload và tải file đính kèm với cơ chế bảo vệ dung lượng/định dạng.
    - `SessionsController`: Quản lý các phiên phòng học trực tuyến.
  - `Middleware/`:
    - `GlobalExceptionMiddleware`: Bắt mọi ngoại lệ chưa bắt được, ghi log và trả về RFC 7807 `ProblemDetails`.
    - `RequestTimingMiddleware`: Đo thời gian thực thi của từng request, cảnh báo nếu vượt ngưỡng hiệu năng.
  - `Extensions/`:
    - `ServiceCollectionExtensions`: Đóng gói việc đăng ký DI (`AddApplicationServices`, `AddInfrastructureServices`, `AddJwtAuthentication`).
  - `Program.cs`: Cấu hình Host, Middleware Pipeline (CORS, Authentication, Authorization, Exception Handling, Swagger UI, MapControllers, MapHub).

---

## 4. Luồng Xử Lý Một Request Điển Hình

```plaintext
[Client (Postman / Web Browser / cURL)]
                  │
                  ▼
         [Kestrel Web Server]
                  │
                  ▼
      [Middleware Pipeline]
       ├── RequestTimingMiddleware
       ├── GlobalExceptionMiddleware (RFC 7807)
       ├── CORS Middleware
       ├── Authentication Middleware (JWT Bearer)
       └── Authorization Middleware (Roles & Policies)
                  │
                  ▼
         [ArticlesController]
                  │
                  ▼
         [ArticleService (Application)]
                  │
                  ▼
         [IUnitOfWork / IArticleRepository (Domain Contract)]
                  │
                  ▼
    [ArticleRepository (Infrastructure / EF Core)]
                  │
                  ▼
      [ApplicationDbContext / SQL Server]
```

---

## 5. Kỷ Luật Phụ Thuộc & Đóng Gói (Packaging Guidelines)

- **Cấm:** Không bao giờ gọi trực tiếp `DbContext` từ trong Controllers; mọi thao tác nghiệp vụ phải đi qua Service hoặc Repository để đảm bảo tính kiểm thử được (Testability) và bảo trì dài hạn.
- **Dữ liệu trả về:** Không bao giờ trả trực tiếp Entity của EF Core ra ngoài API; luôn chuyển đổi sang DTO để bảo vệ thông tin nội bộ và tránh lỗi vòng lặp tuần tự hóa JSON (Circular References).
- **Xử lý bất đồng bộ:** $100\%$ các phương thức I/O (Database, File, SignalR) đều sử dụng `async / await` kèm truyền `CancellationToken` đúng quy chuẩn.
