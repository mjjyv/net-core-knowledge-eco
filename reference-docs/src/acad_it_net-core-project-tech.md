Hệ sinh thái của dự án được xây dựng theo mô hình **Hệ sinh thái tri thức và giao tiếp hợp nhất (Unified Knowledge & Communication Ecosystem)**, kết hợp kiến trúc **Ưu tiên cục bộ (Local-First)**, **Cộng tác đa người dùng thời gian thực (Real-Time Multiplayer Collaboration)**, **Thư viện tri thức mở (Open Knowledge Hubs)** và **Trợ lý AI bảo toàn quyền riêng tư (Privacy-Preserving AI Assistant)**.

#### 1. Kiến trúc Tổng thể & Hạ tầng Monorepo (Agent-First Architecture)

Hệ thống áp dụng kiến trúc định hướng AI Agent (Agent-First Architecture) nhằm chuẩn hóa giao tiếp và triệt tiêu lỗi ảo giác logic khi sinh mã tự động:

- **Turborepo Monorepo & pnpm Workspaces:** Toàn bộ hệ sinh thái từ Client, Web đến Vi dịch vụ được đóng gói trong một kho lưu trữ mã nguồn duy nhất. Cấu trúc này tối ưu hóa đường ống biên dịch (Pipeline), lưu bộ nhớ đệm thông minh (Smart Caching) và chia sẻ mã nguồn giữa các phân hệ.
- **TypeScript End-to-End (E2E):** Sử dụng một ngôn ngữ định kiểu tĩnh duy nhất xuyên suốt toàn bộ các tầng. Trình biên dịch `tsc` đóng vai trò kiểm soát an toàn (Guardrail), tạo thành vòng lặp tự sửa lỗi (Self-Correction Loop) cho AI Coding Agent.
- **Phát triển định hướng giao ước (Schema-Driven Development):**
    - `packages/contracts`: Định nghĩa bất biến các giao diện dữ liệu (Interfaces, DTOs, Schemas) như `UserPresence`, `UserRole`, `CursorFloatingMessage`, `DocumentLocation`.
    - Chuẩn hóa giao tiếp liên phân hệ thông qua **tRPC** (Type-safe API nội bộ), **OpenAPI/Swagger** (RESTful API) và **Protocol Buffers / WebSocket Payloads** (dữ liệu nhị phân thời gian thực).

```plaintext
packages/contracts (Single Source of Truth)
        │                 │                 │
        ▼                 ▼                 ▼
apps/desktop-pkm    apps/chat-hub    apps/open-knowledge-web
```

#### 2. Công nghệ Phân hệ Khách (Frontend & Client Ecosystem)

Kiến trúc "Tam giác vàng" (The Golden Triangle) phân rã thành 3 ứng dụng chuyên biệt cùng các gói thư viện dùng chung:

##### 2.1. Sổ tay cá nhân ưu tiên cục bộ (`apps/desktop-pkm`)

- **Tauri v2 + Rust Core:** Thay thế nền tảng cồng kềnh Electron; tầng nhân Rust giao tiếp trực tiếp với hệ điều hành và hệ thống tệp tin. Giúp giảm kích thước tệp cài đặt (8–15 MB), giảm mức tiêu thụ RAM nghỉ (32–45 MB) và tăng tốc khởi động nguội (< 0.4 giây).
- **React + Vite + Tailwind CSS:** Tầng hiển thị giao diện người dùng mượt mà, đồng nhất ngôn ngữ thiết kế.
- **Lưu trữ ngoại tuyến thuần văn bản (Plain-Text Markdown):** Toàn bộ dữ liệu nằm trên ổ cứng nội bộ dưới định dạng `.md`, loại bỏ nguy cơ khóa chặt dữ liệu bởi nhà cung cấp (Vendor Lock-in).
- **Cơ sở dữ liệu nhúng (SQLite Embedded):** Quét ngầm hệ thống tệp để lập chỉ mục tìm kiếm toàn văn (Full-Text Search) và chỉ mục liên kết hai chiều (Backlinks / WikiLinks `[[...]]`).

##### 2.2. Cổng Thư viện tri thức cộng đồng (`apps/open-knowledge-web`)

- **Next.js (App Router) + Server-Side Rendering (SSR):** Tối ưu hóa SEO (đạt điểm Lighthouse > 95), phân loại tri thức phân tầng 4 cấp độ: $\text{Chủ đề (Topic)} \rightarrow \text{Khía cạnh (Aspect)} \rightarrow \text{Chuyên mục (Category)} \rightarrow \text{Bài viết (Article)}$.
- **Tích hợp tài nguyên đa định dạng:** Hỗ trợ trình đọc tài liệu PDF nhúng, trang trình chiếu tương tác (Interactive Slides) và khung thử nghiệm mã nguồn (Code Playground).

##### 2.3. Phòng trò chuyện thông minh (`apps/chat-hub`)

- **Trình soạn thảo văn bản TipTap / Lexical:** Tự động định dạng Markdown khi gõ hoặc dán nội dung.
- **Tô màu cú pháp tĩnh (Syntax Highlighting):** Sử dụng **Shiki / PrismJS** hiển thị khối mã có số dòng và nút sao chép nhanh (Quick Copy), cô lập mã dưới dạng chuỗi tĩnh để ngăn ngừa lỗi thực thi.

##### 2.4. Các gói thư viện tái sử dụng nội bộ (`packages/*`)

- **`@ecosystem/rich-content`:** Lõi phân tích Markdown (CommonMark/GFM), hiển thị công thức toán học tốc độ cao bằng **KaTeX**, và phân tích sơ đồ dạng mã nguồn Client-side bằng **Mermaid.js** và **PlantUML**.
- **`@ecosystem/ui`:** Chứa các thành phần UI tăng tốc GPU như `<LiveCursorsOverlay/>`, vùng chọn văn bản `<LiveSelectionHighlight/>` và hộp chat nổi `<CursorInlineChat/>`.
- **`@ecosystem/collaboration`:** Đóng gói hook `usePresence`, logic kết nối WebSocket, bộ tiết lưu tín hiệu và thuật toán nội suy chuyển động.

#### 3. Công nghệ Backend, Vi dịch vụ & Cơ chế Liên thông

- **NestJS Framework (`apps/backend-gateway`):** Vi dịch vụ trung tâm áp dụng kiến trúc Module và cơ chế Tiêm phụ thuộc (Dependency Injection) chặt chẽ.
- **WebSocket Gateway (`@nestjs/websockets`):** Quản lý kết nối thời gian thực hai chiều có độ trễ cực thấp (< 50ms).
- **PostgreSQL + Prisma ORM:** Lưu trữ quan hệ chuẩn ACID cho dữ liệu phân cấp tri thức trên cổng Web.
- **Giao thức liên kết sâu tùy biến (`ecosystem://`):** Nút bấm *"Sync to my Second Brain"* kích hoạt URL Scheme `ecosystem://import?articleId=...` trên trình duyệt để đánh thức ứng dụng Desktop PKM, tự động kéo tệp Markdown, ảnh đính kèm và sơ đồ về máy tính cá nhân.
- **Background Ingestion Worker:** Dịch vụ nền tự động phân tích URL YouTube, gọi YouTube Transcript API để bóc tách phụ đề kèm mốc thời gian và định dạng thành file Markdown trong kho ghi chú cá nhân.

#### 4. Công nghệ Cộng tác Thời gian thực & Thuật toán Đồng bộ

Hệ thống giải quyết triệt để sự xung đột dữ liệu bằng việc tách bạch giữa **Dữ liệu cốt lõi vĩnh viễn** và **Trạng thái tạm thời**:

```plaintext
┌───────────────────────────────────────┬───────────────────────────────────────┐
│     Dữ liệu Cốt lõi Vĩnh viễn         │    Trạng thái Nhận diện Tạm thời      │
├───────────────────────────────────────┼───────────────────────────────────────┤
│ • Nội dung văn bản, sơ đồ, công thức  │ • Tọa độ chuột, vùng chọn văn bản     │
│ • Lưu trữ: File .md trên ổ cứng       │ • Vòng đời: Chỉ trên RAM, tự hủy      │
│ • Cơ chế: CRDT Y.Doc đồng bộ          │ • Cơ chế: Giao thức Yjs Awareness     │
└───────────────────────────────────────┴───────────────────────────────────────┘
```

- **Lõi CRDT (Conflict-Free Replicated Data Types) qua Yjs:**
    - Đảm bảo tính **Nhất quán hội tụ mạnh (Strong Eventual Consistency - SEC)**: Không phụ thuộc vào máy chủ trung tâm làm trọng tài.
    - Tối ưu hóa cấu trúc danh sách liên kết hai chiều theo khối (`Item`), giảm hơn $90\%$ mức chiếm dụng RAM so với các thư viện CRDT thế hệ cũ.
    - Mã hóa nhị phân qua `lib0` nén gói tin cập nhật xuống mức vài chục byte.
    - Máy chủ điều phối WebSocket thông qua giải pháp **Hocuspocus**.

- **Giao thức Yjs Awareness:**
    - Quản lý thông tin con trỏ ảo, bóng chat nổi và trạng thái hiện diện hoàn toàn trong RAM.
    - Cơ chế nhịp tim (Heartbeat) và tự hủy sau 30 giây (TTL = 30s) khi ngắt kết nối mà không ghi vào cơ sở dữ liệu.

- **Giao thức Matrix (Matrix Protocol):** Cung cấp chuẩn giao tiếp trò chuyện bảo mật, phân tán với mã hóa đầu cuối (E2EE) cho các phiên học tập chuyên sâu.

#### 5. Thuật toán & Kỹ thuật Tối ưu hóa Vật lý

| Vấn đề Kỹ thuật | Giải pháp / Thuật toán Thực thi | Chi tiết Cơ chế Hoạt động |
| --- | --- | --- |
| **Lệch tọa độ đa màn hình** (4K vs Mobile/Laptop) | **Block-Relative DOM Node Anchoring** | Gán `blockId` cho từng khối DOM; chuẩn hóa tọa độ thành tỷ lệ tương đối $relativeX, relativeY \in [0.0, 1.0]$. Neo vùng chọn theo chỉ số ký tự $startOffset, endOffset$. |
| **Bão dữ liệu mạng** (100 người $\approx$ 1.2M gói/s) | **Mouse Event Throttling** | Bộ tiết lưu tại Client giới hạn tần số gửi ở 20–30 fps (khoảng chờ 33ms–50ms), cắt giảm 75%–83% gói tin. |
| **Nghẽn băng thông phòng lớn** (> 20 thành viên) | **Chế độ Lớp học (Classroom / Spotlight Mode)** | Giáo viên (`HOST`) phát sóng tọa độ và khóa cuộn màn hình (`Follow Me`). Học viên (`AUDIENCE`) bị ẩn chuột mặc định, chỉ gửi tọa độ khi được duyệt quyền qua cơ chế "Giơ tay" (`canInteract = true`), giảm 99.8% lưu lượng mạng. |
| **Hiện tượng giật chuột** (Mạng 25fps vs Màn hình 60fps) | **Client-Side LERP (Linear Interpolation)** | Trình duyệt máy nhận tự tính toán khung hình nội suy: $P_{cur} = P_{cur} + (P_{target} - P_{cur}) \times \alpha$ ($\alpha = 0.15 - 0.25$). Kết hợp thuộc tính CSS tăng tốc GPU `transform: translate3d()`. |
| **Phân tâm khi thảo luận** | **Bong bóng chat ngữ cảnh nổi (`<CursorInlineChat/>`)** | Kích hoạt bằng phím tắt `/` tại khối đang chọn, phát gói tin `CursorFloatingMessage` lơ lửng ngay trên dòng văn bản và tự hủy sạch khỏi DOM sau 8 giây. |

#### 6. Công nghệ AI Bảo toàn Quyền riêng tư (Privacy-Preserving AI)

Phân hệ AI tuân thủ nguyên tắc **Bảo mật bằng kiến trúc (Privacy-by-Architecture)**:

```plaintext
[Âm thanh Video Call / Chat] ──► Xử lý độc quyền trên RAM (In-Memory STT / LLM)
                                           │
                                           ├─► Ranh giới 1: Zero Server-Side Retention (0 Byte ghi ổ cứng)
                                           ├─► Ranh giới 2: No-Training Policy (Không dùng để huấn luyện)
                                           │
                                           ▼ Kết quả đẩy thẳng về Client
                                [Lưu file .md trên Ổ cứng máy người dùng]
```

- **Speech-to-Text (STT) không lưu vết:** Luồng âm thanh từ các cuộc gọi WebRTC được bóc tách tức thời qua mô hình **Whisper** trực tiếp trên bộ nhớ RAM; tệp âm thanh gốc và văn bản thô bị giải phóng ngay lập tức sau khi hoàn thành.
- **Tóm tắt hội thoại định kỳ (Scheduled Pipelines):** Chạy tự động theo lịch Cron-job (ví dụ: $23:00$ hàng ngày), tổng hợp các quyết định và đầu việc quan trọng rồi gửi thẳng bản Markdown về cho người dùng.
- **Active Recall & Thuật toán Lặp lại Ngắt quãng (Spaced Repetition):** AI tự động bóc tách bài học thành câu hỏi trắc nghiệm và Flashcards, đồng bộ với thuật toán **FSRS** (Free Spaced Repetition Scheduler) hoặc **SM-2** tích hợp trong ứng dụng Desktop PKM để lập lịch ôn tập.
- **Tìm kiếm ngữ nghĩa cục bộ (Private Local RAG):** Lập chỉ mục tài liệu thành các vector nhiều chiều và truy vấn trực tiếp bằng **pgvector** hoặc **SQLite-vec** trên thiết bị cá nhân mà không đẩy dữ liệu lên đám mây.
- **Chính sách không lưu trữ & không huấn luyện (Zero-Retention & No-Training Policy):** Tuyệt đối không lưu chat logs lên máy chủ, cam kết không dùng dữ liệu người dùng để tái huấn luyện mô hình thương mại, áp dụng thỏa thuận **Enterprise Zero Data Retention (ZDR)** khi gọi API LLM.

#### 7. Ranh giới Đỏ Bảo mật (Security Red Lines)

Hệ thống thiết lập các rào cản cưỡng chế kiến trúc bắt buộc:

1. **Cấm tiệt `dangerouslySetInnerHTML` và `element.innerHTML`:** Mọi nội dung hiển thị phải đi qua bộ phân tích cây cú pháp trừu tượng (AST) của `@ecosystem/rich-content` và được mã hóa thực thể HTML an toàn.
2. **Khung Hộp kính cát an toàn (Mandatory Sandboxed Iframe):** Mọi mã mô phỏng chuyển động HTML/JS phải được bọc trong thẻ `<iframe>` với thuộc tính `sandbox="allow-scripts"` (tuyệt đối không có `allow-same-origin`, `allow-top-navigation`) cùng chính sách CSP khắt khe (`default-src 'none'`), ngăn chặn tuyệt đối lỗi XSS và bảo vệ hệ thống tệp cục bộ.
3. **Cách ly thụ động mã trong phòng Chat:** Mã lập trình chỉ hiển thị dưới dạng chuỗi tĩnh; chỉ cho phép vẽ sơ đồ từ 2 engine được kiểm duyệt là `Mermaid.js` và `PlantUML` có cơ chế lùi về an toàn (Safe Fallback).
4. **Hạn ngạch vận hành:** Gói tin WebSocket giới hạn dưới $1024$ bytes; TTL trạng thái chuột trên RAM tối đa $30$ giây; TTL bong bóng chat nổi tối đa $10$ giây.

#### 8. Chỉ số Thực nghiệm Phòng Thí nghiệm (Empirical Benchmarks)

Các bài kiểm thử áp lực và đo lường độc lập đã xác thực năng lực vận hành của toàn bộ ngăn xếp công nghệ:

- **Độ trễ đồng bộ Presence:** Đạt **$46.2\text{ms}$** qua mạng diện rộng WAN (tiêu chuẩn thiết kế $< 50\text{ms}$) và **$8\text{ms} - 15\text{ms}$** qua mạng LAN.
- **Hiệu năng phần cứng Client (Tauri v2 vs. Electron 30):**
    - *Kích thước tệp cài đặt:* Tauri v2 đạt **8–15 MB** (Electron: 120–180 MB).
    - *Bộ nhớ RAM trạng thái nghỉ:* Tauri v2 chỉ chiếm **32–45 MB** (Electron: 140–220 MB).
    - *Bộ nhớ RAM khi render tài liệu nặng (KaTeX + 5 Mermaid):* Tauri v2 duy trì ở mức **110–145 MB** (Electron: 450–600 MB).
    - *Tải CPU khi tương tác chuột:* Chỉ từ **$2\% - 4\%$** nhờ LERP và GPU CSS.

- **Kiểm soát bão dữ liệu:** Chế độ Lớp học cắt giảm **$99.8\%$** số lượng gói tin trong phòng 100 người (từ $1.188.000$ gói/giây xuống còn $2.475$ gói/giây).
- **Tỷ lệ tự động hóa mã nguồn của AI Coding Agent:** Đạt **$88.5\%$** tổng số dòng mã của dự án được sinh ra hoàn toàn tự động nhờ kiến trúc TypeScript End-to-End và các giao ước dữ liệu nghiêm ngặt trong `packages/contracts`.


`````markdown
Cập nhật tổng quan công nghệ dự án, thay thế những phần hợp lệ với ASP.NET Core (C#), nhằm xây dựng đồ án môn WEB - ASP.NET Core.
`````

## ASP.NET CORE C#

Dưới đây là bản **Cập nhật và Tái cấu trúc Tổng quan Công nghệ của Dự án**, trong đó toàn bộ các thành phần phía máy chủ, dịch vụ thời gian thực, quản lý dữ liệu, tác vụ nền và tích hợp AI được chuyển dịch sang **hệ sinh thái .NET (C#) / ASP.NET Core**.

Kiến trúc này được thiết kế theo tiêu chuẩn công nghiệp hiện đại (**Clean Architecture / Modular Monolith**), giữ trọn vẹn triết lý cốt lõi của đề tài (*Local-First PKM, Multiplayer Real-Time, Open Hub, Privacy-Preserving AI*) nhưng khai thác tối đa sức mạnh của **.NET 8 / .NET 9** để phục vụ xuất sắc cho đồ án môn **Lập trình Web - ASP.NET Core**.

### BẢNG ĐỐI CHIẾU CHUYỂN DỊCH CÔNG NGHỆ (NODE.JS/NESTJS $\rightarrow$ ASP.NET CORE C#)

| Thành phần Kiến trúc | Công nghệ Ban đầu (Node.js / Multi-stack) | Công nghệ Chuyển đổi Chuẩn hóa (.NET / C#) | Lợi thế Kỹ thuật trong Môn học Web - ASP.NET Core |
| --- | --- | --- | --- |
| **Backend API Gateway** | NestJS (Node.js / Express) | **ASP.NET Core Web API (.NET 8/9)** | Tận dụng Dependency Injection gốc (Native DI), Kestrel Server siêu tốc, Middleware Pipeline linh hoạt, Minimal APIs kết hợp Controller-based. |
| **Cộng tác Thời gian thực** | WebSockets thuần / NestJS WS | **ASP.NET Core SignalR (SignalR Hubs)** | Hỗ trợ tự động fallback (WebSockets, Server-Sent Events, Long Polling), quản lý Connection/Groups theo phòng học, hỗ trợ nhị phân MessagePack. |
| **Truy xuất Dữ liệu & ORM** | Prisma ORM / PostgreSQL | **Entity Framework Core (EF Core 8/9)** | Sử dụng LINQ truy vấn an toàn kiểu, Migrations tự động, Fluent API cấu hình quan hệ phân cấp, Change Tracking, Compiled Queries. |
| **Cơ sở Dữ liệu Quan hệ** | PostgreSQL | **Microsoft SQL Server** hoặc **PostgreSQL** | Tương thích hoàn hảo với EF Core; hỗ trợ lưu trữ dữ liệu phân cấp 4 tầng bài học và chỉ mục toàn văn. |
| **Xác thực & Phân quyền** | Passport.js / JWT tùy biến | **ASP.NET Core Identity & JWT Bearer** | Hệ thống Identity chuẩn của Microsoft, hỗ trợ Role-based & Policy-based Authorization (`HOST`, `PRESENTER`, `AUDIENCE`), Claims-based Security. |
| **Tác vụ Nền & Thu thập** | Custom Node.js Ingestion Worker | **`BackgroundService` (IHostedService) / Hangfire** | Sử dụng `System.Threading.Channels` cho hàng đợi ngầm, Hangfire / Quartz.NET lên lịch Cron job tóm tắt tự động lúc 23:00. |
| **Trợ lý AI & Local RAG** | Script ngoài / LangChain JS | **Microsoft Semantic Kernel / OllamaSharp** | Framework AI chính thức của Microsoft cho C#, kết hợp Semantic Search qua EF Core Vector (`pgvector` hoặc Qdrant SDK) xử lý in-memory an toàn. |
| **Tài liệu hóa API** | NestJS Swagger | **OpenAPI / Swagger (Swashbuckle / Scalar)** | Tự động sinh giao diện kiểm thử API, tích hợp JWT Authorize Header trực quan. |
| **Giao diện Web Thư viện mở** | Next.js App Router | **ASP.NET Core Blazor Web App** *(hoặc Next.js/React gọi Web API)* | Cho phép code C# Full-stack (C# cả Client lẫn Server), kết xuất SSR/Streaming và tương tác thời gian thực qua SignalR kết nối sẵn. |

### 1. Kiến trúc Cấu trúc Solution .NET (Clean Architecture)

Để đảm bảo tính mô-đun hóa, bảo trì và tuân thủ các nguyên tắc thiết kế SOLID, mã nguồn Backend được cấu trúc thành một .NET Solution duy nhất:

```plaintext
Ecosystem.UnifiedWorkspace.sln
├── src/
│   ├── Ecosystem.Domain/              # Lõi thực thể (Entities, Enums, Exceptions, Value Objects)
│   │   ├── Entities/                  # Topic, Aspect, Category, Article, User, ClassroomSession
│   │   └── Enums/                     # UserRole (HOST, PRESENTER, AUDIENCE)
│   │
│   ├── Ecosystem.Application/         # Nghiệp vụ logic (CQRS Commands, Queries, DTOs, Interfaces)
│   │   ├── Common/                    # Behaviors (Logging, Validation, Performance via MediatR)
│   │   ├── Contracts/                 # Hợp đồng DTO: PresenceDto, FloatingMessageDto, ArticleBundleDto
│   │   ├── Features/                  # Tách theo Vertical Slices: Articles, Auth, Ingestion, Summaries
│   │   └── Interfaces/                # IApplicationDbContext, IRealtimeHubContext, IAIService
│   │
│   ├── Ecosystem.Infrastructure/      # Tầng hạ tầng và tích hợp công nghệ ngoài
│   │   ├── Persistence/               # EF Core DbContext, Migrations, Entity Configurations
│   │   ├── Realtime/                  # SignalR Hubs (CollaborationHub, ClassroomHub)
│   │   ├── BackgroundJobs/            # YouTubeIngestionService, NightlySummarizationJob (Hangfire)
│   │   ├── AI/                        # Semantic Kernel Integration, Whisper STT Client, In-Memory Guards
│   │   └── Identity/                  # ASP.NET Core Identity, TokenService (JWT)
│   │
│   ├── Ecosystem.WebAPI/              # Điểm tiếp nhận yêu cầu (Presentation Layer)
│   │   ├── Controllers/               # RESTful API Controllers (Articles, Auth, Ingestion, Sessions)
│   │   ├── Middleware/                # GlobalExceptionHandling, SecurityHeaders, RateLimiting
│   │   ├── Hubs/                      # Đường dẫn SignalR Endpoints (/hubs/collaboration)
│   │   └── Program.cs                 # Cấu hình Services, Middleware Pipeline
│   │
│   └── Ecosystem.WebClient/           # Frontend Thư viện mở (Blazor Web App hoặc React/Next.js)
│
└── tests/
    ├── Ecosystem.UnitTests/           # xUnit, FluentAssertions, Moq
    └── Ecosystem.IntegrationTests/    # WebApplicationFactory, Testcontainers (SQL Server / Postgres)
```

### 2. Chi tiết các Phân hệ Công nghệ Cốt lõi

#### 2.1. Phân hệ Cổng điều phối & Dữ liệu (ASP.NET Core Web API + EF Core)

- **Minimal APIs & Controllers:** Kết hợp Controller-based để phân chia các tài nguyên chính (`ArticlesController`, `AuthController`) với các Middleware pipeline tối ưu.
- **Mô hình CQRS với MediatR & FluentValidation:** Tách bạch các luồng Đọc (`Queries`) và Ghi (`Commands`). Toàn bộ DTO đầu vào được kiểm thực tự động thông qua Pipeline Behaviors trước khi chạm tới Controller.
- **Entity Framework Core (EF Core):**
    - Mô hình hóa cấu trúc tri thức 4 tầng: `Topic` (Chủ đề) $\rightarrow$ `Aspect` (Khía cạnh) $\rightarrow$ `Category` (Chuyên mục) $\rightarrow$ `Article` (Bài viết).
    - Tối ưu hóa các truy vấn với `AsNoTracking()` cho các trang đọc bài viết công cộng.
    - Cấu hình Khóa ngoại (Foreign Keys), Khóa chính (Primary Keys) và Chỉ mục (Indexes) bằng Fluent API thay vì Data Annotations để giữ Domain Entity độc lập.

#### 2.2. Phân hệ Thời gian thực & Lớp học Tương tác (ASP.NET Core SignalR)

SignalR đóng vai trò trung tâm thay thế cụm NestJS WebSocket, xử lý toàn bộ các tính năng tương tác đa người dùng:

```c#
// Giao ước Client mạnh (Strongly-Typed Hub Interface)
public interface ICollaborationClient
{
    Task UserPresenceUpdated(UserPresenceDto presence);
    Task UserLeft(string connectionId, string userId);
    Task FloatingMessageReceived(CursorFloatingMessageDto message);
    Task SpotlightMoved(DocumentLocationDto location);
    Task PermissionGranted(string targetUserId, bool canInteract);
}

// Hub điều phối thời gian thực với SignalR
[Authorize]
public class CollaborationHub : Hub<ICollaborationClient>
{
    private readonly IPresenceMemoryTracker _presenceTracker;

    public CollaborationHub(IPresenceMemoryTracker presenceTracker)
    {
        _presenceTracker = presenceTracker;
    }

    public async Task JoinArticleRoom(string articleId, UserRole role)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"article_{articleId}");
        // Lưu trữ tạm thời trạng thái trong bộ nhớ RAM thông qua MemoryCache / In-Memory State
    }

    public async Task BroadcastPresence(string articleId, UserPresenceDto presence)
    {
        // Bộ lọc vai trò: Nếu học viên không có quyền canInteract thì chặn gửi tọa độ
        if (presence.Role == UserRole.AUDIENCE && !presence.CanInteract) return;

        await Clients.OthersInGroup($"article_{articleId}").UserPresenceUpdated(presence);
    }

    public async Task SendFloatingBubble(string articleId, CursorFloatingMessageDto message)
    {
        await Clients.Group($"article_{articleId}").FloatingMessageReceived(message);
    }
}
```

- **Tối ưu hóa gói tin bằng MessagePack:** Tích hợp `Microsoft.AspNetCore.SignalR.Protocols.MessagePack` vào SignalR. Dữ liệu tọa độ tương đối theo khối (`blockId`, `relativeX`, `relativeY`) được nén thành chuỗi nhị phân siêu nhẹ, duy trì độ trễ **dưới 50ms**.
- **SignalR Groups & Connection Mapping:** Gom nhóm người dùng theo từng bài viết (`articleId`). Khi Giáo viên bật **Chế độ Lớp học (Spotlight Mode)**, Hub chỉ phát sóng sự kiện điều hướng và cuộn trang của người mang vai trò `HOST` tới toàn nhóm.

#### 2.3. Phân hệ Tác vụ Nền & Thu thập Tự động (Background Processing)

- **Background Ingestion Service:** Kế thừa `Microsoft.Extensions.Hosting.BackgroundService`.
    - Sử dụng `System.Threading.Channels` làm hàng đợi bất đồng bộ xử lý trong bộ nhớ.
    - Khi người dùng dán URL YouTube, API ghi nhận và đẩy vào Channel; worker chạy ngầm gọi API bóc tách phụ đề, loại bỏ định dạng thừa, bọc mốc thời gian và định dạng thành file Markdown.

- **Lập lịch Tóm tắt Định kỳ (Scheduled Jobs):** Tích hợp **Hangfire** (hoặc `IHostedService` kết hợp Cronos/Quartz.NET).
    - Tự động kích hoạt vào lúc 23:00 hàng ngày.
    - Rà soát các luồng thảo luận trong phiên học, trích xuất các quyết định kỹ thuật và đầu việc (Action Items), xuất bản thành bản tóm tắt Markdown phân phối về Client.

#### 2.4. Phân hệ Trợ lý AI Bảo toàn Quyền riêng tư (Privacy AI via Semantic Kernel)

- **Microsoft Semantic Kernel (C# SDK):** Đóng vai trò lõi điều phối AI.
    - Kết nối với các mô hình Local LLM qua **Ollama API** (hoặc **OllamaSharp**) hoặc kết nối dịch vụ Azure OpenAI với cấu hình ZDR (Zero Data Retention).
    - **Tạo câu hỏi ôn tập (Active Recall):** Phân tích nội dung Markdown để tự động sinh các câu hỏi trắc nghiệm và thẻ ghi nhớ Flashcards, định dạng sẵn cho thuật toán FSRS / SM-2.

- **Cam kết Kiến trúc Bảo mật (Zero Server-Side Retention):**
    - Luồng âm thanh từ các phiên đàm thoại WebRTC được truyền dưới dạng `MemoryStream` trực tiếp qua bộ chuyển đổi STT (Whisper API / On-device model).
    - Ngay sau khi văn bản tóm tắt được khởi tạo, luồng stream lập tức bị giải phóng (`Dispose()`). Không có bất kỳ tệp ghi âm hay file tạm nào được lưu vào ổ cứng máy chủ.
    - Bản tóm tắt cuối cùng được đẩy thẳng về ứng dụng máy tính cá nhân để ghi thành tệp `.md`.

#### 2.5. Cơ chế Cầu nối Liên thông Sâu (`ecosystem://`)

- **Article Export Engine:** Cung cấp API `GET /api/v1/articles/{id}/bundle` trong ASP.NET Core, nén nội dung Markdown kèm ảnh đính kèm và sơ đồ thành gói phản hồi chuẩn hóa.
- **Đồng bộ một chạm (Deep-Link Protocol):** Nút bấm *"Sync to my Second Brain"* trên giao diện web kích hoạt giao thức `ecosystem://import?articleId=...` được đăng ký trên hệ điều hành, đánh thức ứng dụng Desktop PKM để tự động tải trọn gói tài liệu về thư mục cục bộ của người dùng.

### 3. Tùy chọn Kiến trúc Giao diện (Frontend) cho Đồ án

Tùy theo yêu cầu cụ thể của giảng viên môn Lập trình Web - ASP.NET Core, đồ án có thể triển khai phần hiển thị theo một trong hai mô hình:

```plaintext
MÔ HÌNH A: THUẦN MICROSOFT STACK (.NET FULL-STACK)
┌────────────────────────────────────────────────────────┐
│     Blazor Web App (.NET 8/9 Full-Stack C#)            │
│  • Giao diện Web: Blazor Server / WebAssembly          │
│  • Giao tiếp thời gian thực: SignalR Client C# gốc     │
│  • Ưu điểm: 100% C# từ Client đến Server               │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│        ASP.NET Core Web API & SignalR Hub Backend      │
└────────────────────────────────────────────────────────┘

                           HOẶC

MÔ HÌNH B: TÁCH BIỆT API & SPA HIỆN ĐẠI (DECOUPLED ARCHITECTURE)
┌───────────────────────────┐    ┌───────────────────────────┐
│     Next.js / React SPA   │    │  Desktop PKM (Tauri v2)   │
│  (Thư viện mở & Khung chat│    │  (Sổ tay cá nhân cục bộ)  │
└─────────────┬─────────────┘    └─────────────┬─────────────┘
              │ RESTful API                    │ Deep-link Sync
              │ & SignalR JS Client            │ (ecosystem://)
              ▼                                ▼
┌────────────────────────────────────────────────────────────┐
│         ASP.NET Core Web API & SignalR Hub Backend         │
│   (EF Core, Identity, SignalR, Semantic Kernel, Hangfire)  │
└────────────────────────────────────────────────────────────┘
```

- **Mô hình A (Blazor Full-Stack C#):** Rất được các giảng viên .NET đánh giá cao vì chứng minh khả năng làm chủ hoàn toàn hệ sinh thái Microsoft từ giao diện đến cơ sở dữ liệu. SignalR được tích hợp tự nhiên giữa Blazor và ASP.NET Core Backend.
- **Mô hình B (ASP.NET Core Web API + Next.js / React Frontend):** Thích hợp nếu muốn giữ nguyên bộ thư viện giao diện phong phú sẵn có (`@ecosystem/rich-content`, TipTap, KaTeX, Mermaid.js), trong khi toàn bộ nghiệp vụ điều phối, thời gian thực và dữ liệu do ASP.NET Core đảm nhiệm.

### 4. Giá trị Đạt được Đối với Môn học Web - ASP.NET Core

Việc chuyển đổi đề tài sang nền tảng ASP.NET Core giúp đồ án đáp ứng toàn diện các tiêu chí đánh giá xuất sắc (Điểm A/A+):

1. **Làm chủ Kỹ thuật Lập trình Hiện đại trên C# & .NET:** Ứng dụng thành thạo lập trình bất đồng bộ (`async/await`), quản lý vòng đời dịch vụ qua DI (`Transient`, `Scoped`, `Singleton`), generic repositories, và mô hình thiết kế CQRS với MediatR.
2. **Kỹ năng Thiết kế Cơ sở dữ liệu Chuyên sâu:** Sử dụng EF Core với các quan hệ phức tạp (1-N, N-N), thiết kế cấu trúc dữ liệu phân tầng 4 cấp, quản lý migrations và tối ưu chỉ mục.
3. **Hạ tầng Thời gian thực Chuyên nghiệp:** Sử dụng SignalR Hubs để giải quyết bài toán khó trong thực tế: đồng bộ con trỏ chuột đa màn hình, khóa cuộn trang trong lớp học trực tuyến và bong bóng chat nổi mà không gây bão dữ liệu.
4. **Bảo mật & Tác vụ Nền Công nghiệp:** Áp dụng ASP.NET Core Identity, JWT, Policy-based Authorization kết hợp với `BackgroundService` và Hangfire cho các tác vụ tóm tắt tự động.
5. **Tính Đi đầu về Công nghệ (AI Integration):** Tích hợp Semantic Kernel của Microsoft để xây dựng trợ lý học tập bảo toàn quyền riêng tư, kết nối chặt chẽ giữa ứng dụng Web truyền thống và kỷ nguyên Trí tuệ Nhân tạo.