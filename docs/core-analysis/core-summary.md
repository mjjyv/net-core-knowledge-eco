# Tổng Hợp & Nhận Diện Tri Thức Cốt Lõi Từ `reference-docs`

Tài liệu này tổng hợp toàn bộ các nội dung cốt lõi thu nhận từ kho tài liệu nghiên cứu gốc trong [`reference-docs/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs), bao gồm hồ sơ phân tích thiết kế hệ thống, tổng quan công nghệ chuyển dịch sang .NET 8 C#, hệ thống 15 bài học thực chiến và các tiêu chuẩn công nghệ phần mềm chuyên nghiệp.

---

## 1. Bối Cảnh Bài Toán & Tầm Nhìn Dự Án The Ecosystem

### 1.1. Nghịch lý Phân mảnh Thông tin (Information Fragmentation Paradox)
Trong môi trường học thuật và làm việc tri thức, người dùng thường bị phân mảnh trải nghiệm qua 3 công cụ độc lập:
1. **Sổ tay cá nhân ngoại tuyến (Local-First PKM):** An toàn, lưu file Markdown nhưng cô lập, thiếu tương tác cộng tác trực tiếp.
2. **Kênh trao đổi trực tuyến (Chat Hubs / Classroom):** Luồng tin trôi nhanh, nghèo nàn về cấu trúc, làm đứt gãy mạch tư duy chiều sâu.
3. **Cổng tri thức cộng đồng mở (Open Knowledge Hub):** Cung cấp tài liệu phân cấp bài bản nhưng chỉ tiêu thụ 1 chiều, không thể kéo nguyên vẹn về kho tri thức cá nhân.

### 1.2. Giải pháp Hợp nhất của The Ecosystem
Dự án **The Ecosystem (Unified Knowledge & Communication Ecosystem)** hợp nhất 3 phân hệ trên thành một hệ thống đồng nhất:
- **Local-First & Data Ownership:** Dữ liệu thuộc quyền sở hữu người dùng, lưu trữ định dạng mở (Markdown, SQLite, JSON).
- **Multiplayer Real-time Collaboration:** Cộng tác trực tiếp theo thời gian thực (con trỏ chuột nhận diện, bình luận nổi, lớp học tương tác) qua **SignalR**.
- **Cây tri thức phân cấp 4 tầng (Taxonomy):** `Topic (Chủ đề) -> Aspect (Khía cạnh) -> Category (Chuyên mục) -> Article (Bài viết)`.
- **Cơ chế liên kết sâu (`ecosystem://`):** Nhập tài liệu từ Web về kho ghi chú cá nhân chỉ bằng một cú nhấp chuột.
- **Privacy-Preserving AI:** Xử lý tóm tắt và hỏi đáp trên bộ nhớ RAM, bảo đảm không lưu vết (Zero Data Retention) và không dùng dữ liệu người dùng để huấn luyện mô hình.

---

## 2. Bản Lề Chuyển Dịch Công Nghệ Sang .NET 8 / C#

Toàn bộ các phân hệ phía máy chủ được chuẩn hóa và triển khai trên **.NET 8 C#** theo mô hình **Clean Architecture / Modular Monolith**:

| Phân hệ / Nhu cầu | Giải pháp Ban đầu (Node/Multi-stack) | Chuyển dịch Chuẩn hóa (.NET 8 C#) | Lợi thế Kỹ thuật |
|---|---|---|---|
| **Cổng tiếp nhận API** | NestJS (Express / Fastify) | **ASP.NET Core Web API (.NET 8)** | Hiệu năng Kestrel vượt trội, Native DI, Middleware pipeline mềm dẻo, OpenAPI/Swagger tích hợp sẵn. |
| **Cộng tác Thời gian thực** | WebSockets thuần / NestJS Gateway | **ASP.NET Core SignalR (SignalR Hubs)** | Quản lý Group theo phòng học/tài liệu, đàm phán kết nối tự động, tối ưu hóa gói tin nhị phân qua MessagePack. |
| **Truy xuất Dữ liệu (ORM)** | Prisma ORM / PostgreSQL | **Entity Framework Core 8 (EF Core)** | LINQ an toàn kiểu dữ liệu, Code First Migrations, Fluent API cấu hình quan hệ phân cấp, Change Tracker, Compiled Queries. |
| **Cơ sở Dữ liệu Quan hệ** | PostgreSQL | **SQL Server / PostgreSQL** | Lưu trữ toàn vẹn ACID cho cây tri thức 4 cấp, tài khoản người dùng, phiên lớp học. |
| **Xác thực & Phân quyền** | Passport.js / JWT tùy biến | **ASP.NET Core Identity & JWT Bearer** | Chuẩn bảo mật Microsoft, phân quyền Role-based (`HOST`, `PRESENTER`, `AUDIENCE`) và Policy-based. |
| **Tác vụ Nền & Thu thập** | Custom Ingestion Worker | **`BackgroundService` (IHostedService)** | Xử lý hàng đợi ngầm với `System.Threading.Channels`, thu thập phụ đề và xử lý dữ liệu tự động. |
| **Tài liệu hóa API** | NestJS Swagger | **OpenAPI / Swashbuckle / Scalar** | Sinh tài liệu tương tác tự động, cấu hình Authorize Bearer Token trực quan. |

---

## 3. Khung Kiến Trúc Solution Chuẩn (.NET 8 Clean Architecture)

Dự án tổ chức mã nguồn theo 4 tầng tách bạch:

```plaintext
TheEcosystem.sln
├── src/
│   ├── Ecosystem.Domain/              # Thực thể cốt lõi, Value Objects, Enums, Exceptions (Không phụ thuộc tầng ngoài)
│   │   ├── Entities/                  # Topic, Aspect, Category, Article, User, ClassroomSession
│   │   ├── Enums/                     # UserRole, SessionStatus, ArticleStatus
│   │   └── Common/                    # BaseEntity, IAggregateRoot
│   │
│   ├── Ecosystem.Application/         # Nghiệp vụ ứng dụng (CQRS / Services, DTOs, Interfaces, Mappers)
│   │   ├── Contracts/                 # PresenceDto, ArticleDto, AuthDtos, PaginationFilter
│   │   ├── Interfaces/                # IApplicationDbContext, IArticleService, ITokenService, IRealtimeHub
│   │   ├── Services/                  # Triển khai nghiệp vụ (ArticleService, AuthService, IngestionService)
│   │   └── Common/                    # PagedResult<T>, Exceptions, Validators
│   │
│   ├── Ecosystem.Infrastructure/      # Hiện thực tầng hạ tầng & tích hợp kỹ thuật
│   │   ├── Persistence/               # ApplicationDbContext, EF Core Configurations, Migrations, Repositories
│   │   ├── Realtime/                  # SignalR Hubs (CollaborationHub, ClassroomHub)
│   │   ├── BackgroundJobs/            # YouTubeIngestionWorker, ChannelQueue
│   │   └── Identity/                  # PasswordHasher, JwtTokenGenerator
│   │
│   └── Ecosystem.WebAPI/              # Điểm tiếp nhận HTTP/WS (Presentation Layer)
│       ├── Controllers/               # RESTful Controllers (AuthController, ArticlesController, SessionsController...)
│       ├── Middleware/                # GlobalExceptionMiddleware, RequestTimingMiddleware
│       ├── Extensions/                # ServiceCollectionExtensions
│       └── Program.cs                 # Cấu hình DI Container, Middleware Pipeline
│
└── tests/
    ├── Ecosystem.UnitTests/           # Kiểm thử đơn vị (xUnit, Moq, FluentAssertions)
    └── Ecosystem.IntegrationTests/    # Kiểm thử tích hợp (WebApplicationFactory)
```

---

## 4. Ánh Xạ 15 Buổi Học Thực Chiến (acad_net-core-slide) Vào Dự Án

Hệ thống 15 bài học trong slide được chia làm 15 Phase nền tảng, từng bước xây dựng nên backend hoàn chỉnh của The Ecosystem:

| Phase | Buổi học tương ứng | Chủ đề Trọng tâm | Áp dụng vào The Ecosystem (Backend) |
|:---:|---|---|---|
| **01** | Buổi 1 | Ôn tập C#, OOP, LINQ, Async/Await | Khởi tạo Solution Clean Architecture, thiết lập Project References và Domain Base Entities. |
| **02** | Buổi 2 | Web API, Middleware, Native DI | Cấu hình Middleware Pipeline, Service Lifetimes (Transient, Scoped, Singleton) trong DI container. |
| **03** | Buổi 3 | Controller, Routing, RESTful, Binding | Thiết kế Controllers cho Cây tri thức (Taxonomy: Topics, Aspects, Categories) theo chuẩn REST. |
| **04** | Buổi 4 | DTO, Validation, Swagger UI | Tạo DTOs, áp dụng Data Annotations validation chặt chẽ, cấu hình Swagger OpenAPI kiểm thử với Postman/cURL. |
| **05** | Buổi 5 | EF Core: DbContext, Code First, Migrations | Thiết lập `ApplicationDbContext`, cấu hình Connection String, tạo bảng dữ liệu bằng Migrations. |
| **06** | Buổi 6 | CRUD EF Core, LINQ, Quan hệ dữ liệu | CRUD đầy đủ cho bài viết Markdown (`Articles`) và quan hệ 4 cấp, tối ưu hóa truy vấn LINQ với `Include` và `AsNoTracking`. |
| **07** | Buổi 7 | N-Tier, Repository & Unit of Work, Mapping | Tách biệt Service - Repository Pattern, Generic Repository, Unit of Work, AutoMapper cho DTOs, Unit Test cho Service. |
| **08** | Buổi 8 | Authentication với JWT Bearer | API Đăng ký, Đăng nhập, băm mật khẩu bảo mật, phát hành JWT Token, Refresh Token, cấu hình Swagger Bearer. |
| **09** | Buổi 9 | Authorization (Roles, Claims, Policies) | Phân quyền 3 vai trò: `HOST`, `PRESENTER`, `AUDIENCE`; thiết lập Policies bảo vệ API quản trị và cập nhật nội dung. |
| **10** | Buổi 10 | File Upload, Configuration, Options Pattern | API Upload file đính kèm (ảnh minh họa, file `.md`), kiểm tra dung lượng/mime type, Options Pattern (`IOptions<T>`). |
| **11** | Buổi 11 | Pagination, Filtering, Searching, Sorting | Xây dựng API phân trang `PagedResult<T>`, tìm kiếm toàn văn tiêu đề/nội dung, bộ lọc danh mục và sắp xếp động. |
| **12** | Buổi 12 | Global Exception Handling & Logging | Custom Middleware bắt lỗi toàn cục chuẩn RFC 7807 `ProblemDetails`, Structured Logging với ILogger, Request Timing Middleware. |
| **13** | Buổi 13 | Swagger nâng cao, CORS, Docker hóa | Cấu hình CORS bảo mật, Swagger Documentation đầy đủ với XML comments, viết Dockerfile đa tầng tối ưu cho .NET 8. |
| **14** | Buổi 14 | Tích hợp Toàn diện & Rà soát Hệ thống | Ghép nối tất cả các phân hệ, đóng gói SignalR Hubs (`CollaborationHub`), đồng bộ gói `ecosystem://bundle`, loại bỏ cảnh báo mã nguồn. |
| **15** | Buổi 15 | Kiểm thử Toàn diện, Báo cáo & Nghiệm thu | Xây dựng kịch bản kiểm thử Happy Path, Negative Path, Postman E2E Test Suite, Seed Data hoàn chỉnh, hồ sơ nghiệm thu kỹ thuật. |

---

## 5. Tiêu Chuẩn Kỹ Thuật & Kỷ Luật Triển Khai (HEM & Standards)

Dự án áp dụng chặt chẽ các chỉ dẫn từ `software-engineering-standards.md` và `software-engineering-HEM.md`:
1. **Defensive System Design:** Xác thực đầu vào tại biên (Input Validation), xử lý ngoại lệ tường minh, không bao giờ nuốt lỗi ngầm (No Silent Failures).
2. **Single Source of Truth:** Schema DTOs và Entities là nguồn chân lý duy nhất.
3. **Built-in Observability:** Structured Logging, ghi vết lỗi và thời gian thực thi của từng yêu cầu.
4. **Git Discipline:** Commit theo chuẩn Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`), **luôn push lên remote git sau mỗi lần cập nhật mã nguồn**.
5. **Phase Acceptance & Verification:** Mỗi phase đều phải được nghiệm thu văn bản vào `reference-docs/plan/` và kèm tài liệu hướng dẫn kiểm thử chi tiết cho Dev vào `reference-docs/guides/`.
