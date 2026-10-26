# The Ecosystem - Unified Knowledge & Communication Ecosystem

> **Hệ Sinh Thái Tri Thức & Giao Tiếp Hợp Nhất (Backend ASP.NET Core .NET 8 / C#)**  
> Kho lưu trữ: `net-core-knowledge-eco`  
> Kiến trúc: **Clean Architecture / Modular Monolith**

---

## 1. Giới Thiệu Dự Án

**The Ecosystem** là hệ sinh thái số hiện đại giải quyết triệt để **Nghịch lý Phân mảnh Thông tin (Information Fragmentation Paradox)** trong học tập và nghiên cứu công nghệ. Hệ thống hợp nhất 3 phân hệ vốn bị cô lập:

1. **Sổ tay cá nhân ưu tiên cục bộ (Local-First PKM):** Lưu trữ ghi chú Markdown trên máy người dùng, tôn trọng quyền sở hữu dữ liệu tuyệt đối.
2. **Không gian cộng tác & lớp học tương tác thời gian thực (Multiplayer Real-Time Chat & Classroom Hub):** Trao đổi trực tiếp, đồng bộ con trỏ chuột và ghi chú nổi theo thời gian thực với độ trễ thấp qua **ASP.NET Core SignalR**.
3. **Cổng thư viện tri thức cộng đồng mở (Open Knowledge Hub):** Cung cấp cấu trúc tri thức phân cấp 4 tầng (`Topic -> Aspect -> Category -> Article`) kèm cơ chế liên thông sâu (`ecosystem://`) giúp người dùng kéo toàn bộ gói tri thức về máy chỉ với một cú nhấp chuột.

---

## 2. Ngăn Xếp Công Nghệ Backend (Backend Tech Stack)

Hệ thống Backend được phát triển hoàn toàn trên nền tảng **.NET 8 (C#)**:

- **Framework:** ASP.NET Core Web API (.NET 8.0)
- **Kiến trúc:** Clean Architecture (Domain $\rightarrow$ Application $\rightarrow$ Infrastructure $\rightarrow$ WebAPI)
- **ORM & Data Access:** Entity Framework Core 8 (Code First, Migrations, LINQ to Entities)
- **Cơ sở dữ liệu:** Microsoft SQL Server / PostgreSQL
- **Giao tiếp thời gian thực:** ASP.NET Core SignalR (SignalR Hubs, MessagePack binary serialization)
- **Xác thực & Phân quyền:** ASP.NET Core Identity, JWT Bearer Token, Role-based & Policy-based Authorization (`HOST`, `PRESENTER`, `AUDIENCE`)
- **Tác vụ nền (Background Jobs):** `BackgroundService` (`IHostedService`), `System.Threading.Channels`
- **Tài liệu hóa API:** OpenAPI 3.0, Swagger UI (Swashbuckle)
- **Kiểm thử:** xUnit, Moq, FluentAssertions, `WebApplicationFactory`
- **Đóng gói & Triển khai:** Docker, Docker Compose, Kestrel Server

---

## 3. Lộ Trình 15 Phase Nền Tảng (15 Foundation Phases)

Dự án được phân rã thành **15 Phase nền tảng**, bám sát và hiện thực hóa tri thức từ 15 bài học trong giáo trình `acad_net-core-slide`:

> [!NOTE]
> Các tính năng nâng cao (AI Semantic Kernel RAG, WebRTC Audio Mesh, Tauri v2 IPC) sẽ được tiếp tục triển khai sau khi hoàn thành 15 phase nền tảng này. Dự án tập trung tối đa vào **Backend** và **hướng dẫn kiểm thử cho Dev**; Frontend chỉ thực hiện khi có yêu cầu cụ thể.

| Phase | Bài giảng tương ứng | Tên Phase & Trọng tâm Kỹ thuật | Trạng thái | Nghiệm thu (`plan/`) | Hướng dẫn Dev (`guides/`) |
|:---:|:---:|---|:---:|:---:|:---:|
| **01** | Buổi 01 | Solution Setup & Domain Entities | 📝 Ready | [`PHASE-01-ACCEPTANCE`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan) | [`PHASE-01-TEST-GUIDE`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides) |
| **02** | Buổi 02 | Web API Pipeline & Native DI Lifetimes | ⏳ Pending | `PHASE-02-ACCEPTANCE` | `PHASE-02-TEST-GUIDE` |
| **03** | Buổi 03 | Controllers & RESTful Routing (Taxonomy) | ⏳ Pending | `PHASE-03-ACCEPTANCE` | `PHASE-03-TEST-GUIDE` |
| **04** | Buổi 04 | DTOs, Model Validation & Swagger UI | ⏳ Pending | `PHASE-04-ACCEPTANCE` | `PHASE-04-TEST-GUIDE` |
| **05** | Buổi 05 | EF Core: DbContext, Connection & Migrations | ⏳ Pending | `PHASE-05-ACCEPTANCE` | `PHASE-05-TEST-GUIDE` |
| **06** | Buổi 06 | CRUD Bài Viết, LINQ & Quan Hệ Dữ Liệu | ⏳ Pending | `PHASE-06-ACCEPTANCE` | `PHASE-06-TEST-GUIDE` |
| **07** | Buổi 07 | N-Tier Architecture, Repository & Unit of Work | ⏳ Pending | `PHASE-07-ACCEPTANCE` | `PHASE-07-TEST-GUIDE` |
| **08** | Buổi 08 | Authentication với JWT Bearer Token | ⏳ Pending | `PHASE-08-ACCEPTANCE` | `PHASE-08-TEST-GUIDE` |
| **09** | Buổi 09 | Authorization: Roles, Claims & Policies | ⏳ Pending | `PHASE-09-ACCEPTANCE` | `PHASE-09-TEST-GUIDE` |
| **10** | Buổi 10 | File Upload, Configuration & Options Pattern | ⏳ Pending | `PHASE-10-ACCEPTANCE` | `PHASE-10-TEST-GUIDE` |
| **11** | Buổi 11 | Pagination, Filtering, Searching & Sorting | ⏳ Pending | `PHASE-11-ACCEPTANCE` | `PHASE-11-TEST-GUIDE` |
| **12** | Buổi 12 | Global Exception Handling & Structured Logging | ⏳ Pending | `PHASE-12-ACCEPTANCE` | `PHASE-12-TEST-GUIDE` |
| **13** | Buổi 13 | Swagger Nâng Cao, CORS & Docker hóa | ⏳ Pending | `PHASE-13-ACCEPTANCE` | `PHASE-13-TEST-GUIDE` |
| **14** | Buổi 14 | Tích Hợp Toàn Diện & Realtime SignalR Hub | ⏳ Pending | `PHASE-14-ACCEPTANCE` | `PHASE-14-TEST-GUIDE` |
| **15** | Buổi 15 | Kiểm Thử Toàn Diện, Seed Data & Nghiệm Thu | ⏳ Pending | `PHASE-15-ACCEPTANCE` | `PHASE-15-TEST-GUIDE` |

Xem chi tiết checklist và tiêu chuẩn chuyển tiếp tại:
- [Kế hoạch 15 Phase Tổng thể](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan/MASTER_PLAN_15_PHASES.md)
- [Bản đồ Lộ trình 15 Phase](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/docs/roadmap/foundation-15-phases.md)

---

## 4. Cấu Trúc Kho Lưu Trữ (Repository Structure)

```plaintext
theEcosystem/
├── AGENT.md                        # Quy chuẩn vận hành & nguyên tắc bất biến cho AI Agent
├── README.md                       # Tài liệu tổng quan dự án (tệp này)
├── .gitignore                      # Cấu hình bỏ qua các tệp tạm thời và build output của .NET
│
├── docs/                           # Hệ thống tài liệu kiến trúc & thiết kế hệ thống
│   ├── README.md                   # Mục lục và hướng dẫn hệ thống tài liệu
│   ├── core-analysis/              # Nhận diện & tổng hợp tri thức cốt lõi
│   ├── architecture/               # Đặc tả Clean Architecture, CSDL EF Core, API & SignalR
│   ├── roadmap/                    # Bản đồ lộ trình 15 phase nền tảng
│   └── standards/                  # Tiêu chuẩn lập trình C#, kiểm thử & kỷ luật Git
│
├── reference-docs/                 # Kho tài nguyên tham khảo gốc & nghiệm thu
│   ├── src/                        # 15 bài giảng slide, hồ sơ phân tích thiết kế, tiêu chuẩn HEM
│   ├── core/                       # Tài liệu chuẩn Microsoft .NET Core Extensions & Tutorials
│   ├── plan/                       # MASTER_PLAN_15_PHASES.md và các biên bản nghiệm thu phase
│   └── guides/                     # Hướng dẫn kiểm thử Backend chi tiết cho Developer từng phase
│
└── src/                            # Mã nguồn ứng dụng Backend (.NET 8 Clean Architecture)
    ├── Ecosystem.Domain/           # Thực thể miền cốt lõi, Enums, Value Objects
    ├── Ecosystem.Application/      # DTOs, Interfaces, Logic nghiệp vụ Services
    ├── Ecosystem.Infrastructure/   # EF Core DbContext, Repositories, SignalR, Identity
    └── Ecosystem.WebAPI/           # Controllers, Middleware, Kestrel Program.cs
```

---

## 5. Hướng Dẫn Cài Đặt & Chạy Thử Nghiệm

### Yêu cầu tiên quyết
- **.NET 8 SDK** (phiên bản 8.0 trở lên): Kiểm tra bằng lệnh `dotnet --version`.
- Hệ quản trị CSDL (SQL Server hoặc PostgreSQL / SQLite).
- Docker (tùy chọn, phục vụ chạy container).

### Khởi chạy Backend
```bash
# 1. Khôi phục dependencies
dotnet restore

# 2. Biên dịch giải pháp
dotnet build

# 3. Cập nhật cơ sở dữ liệu (khi đã cấu hình DbContext)
dotnet ef database update --project src/Ecosystem.Infrastructure --startup-project src/Ecosystem.WebAPI

# 4. Khởi chạy Web API
dotnet run --project src/Ecosystem.WebAPI
```

Sau khi khởi chạy, truy cập Swagger UI để tương tác trực tiếp với API:
- URL Swagger: `https://localhost:5001/swagger` hoặc `http://localhost:5000/swagger`.

---

## 6. Kỷ Luật Phát Triển & Quy Chuẩn Đóng Góp

1. **Push Git sau mỗi lần cập nhật:** Mọi thay đổi mã nguồn sau khi biên dịch `dotnet build` và kiểm thử thành công đều **phải được commit và push ngay lập tức lên remote** (`origin/main`).
2. **Quy chuẩn thông điệp Commit:** Tuân thủ Conventional Commits:
   - `feat(...)`: Tính năng mới
   - `fix(...)`: Sửa lỗi
   - `docs(...)`: Tài liệu
   - `test(...)`: Kiểm thử
   - `refactor(...)`: Tái cấu trúc mã nguồn
3. **Nghiệm thu văn bản:** Mỗi phase khi hoàn thành đều phải được nghiệm thu vào `reference-docs/plan/PHASE-XX-ACCEPTANCE.md` và kèm tài liệu kiểm thử cho dev tại `reference-docs/guides/PHASE-XX-TEST-GUIDE.md`.
