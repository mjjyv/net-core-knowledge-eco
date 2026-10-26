# Tài liệu Kỹ thuật Hệ thống The Ecosystem (Documentation System)

Chào mừng bạn đến với trung tâm tài liệu kỹ thuật của dự án **The Ecosystem (Hệ sinh thái Tri thức và Giao tiếp Hợp nhất - Unified Knowledge & Communication Ecosystem)**, được xây dựng và chuẩn hóa trên nền tảng **ASP.NET Core (.NET 8 C#)** theo mô hình **Clean Architecture / Modular Monolith**.

---

## 1. Bản đồ Cấu trúc Tài liệu (`docs/`)

Hệ thống tài liệu được tổ chức phân cấp, khoa học và liên kết trực tiếp với các tài liệu nghiên cứu gốc trong [`reference-docs/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs):

```plaintext
docs/
├── README.md                               # Điểm vào chính của hệ thống tài liệu (tài liệu này)
│
├── core-analysis/
│   └── core-summary.md                     # Nhận diện & tổng hợp tri thức cốt lõi từ reference-docs
│
├── architecture/
│   ├── clean-architecture.md               # Kiến trúc tổng thể Solution .NET 8 theo Clean Architecture
│   ├── database-design.md                  # Thiết kế Cơ sở dữ liệu EF Core, ERD & Cây tri thức 4 cấp
│   └── api-and-realtime.md                 # Đặc tả RESTful API (OpenAPI) & SignalR Real-Time Hubs
│
├── roadmap/
│   └── foundation-15-phases.md             # Bản đồ 15 Phase nền tảng (mapping 15 bài học acad_net-core)
│
└── standards/
    └── engineering-standards.md            # Tiêu chuẩn kỹ thuật phần mềm, C# Clean Code & Testing
```

---

## 2. Quan hệ Giữa `docs/` và `reference-docs/`

| Thư mục | Vai trò | Mục đích & Quy cách Sử dụng |
|---|---|---|
| [`reference-docs/src/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/src) | **Kho tri thức gốc (Source of Truth)** | Chứa toàn bộ slide bài giảng 15 buổi (`acad_net-core-slide*.md`), hồ sơ phân tích thiết kế hệ thống chi tiết (`acad_it_net-core-project-*.md`), tiêu chuẩn công nghệ phần mềm (`software-engineering-*.md`). |
| [`reference-docs/core/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/core) | **Tài liệu kỹ thuật nền tảng .NET** | Các tài liệu tham khảo chính thống từ Microsoft (.NET Extensions, Dependency Injection, Logging, Configuration, Workers, Channels, Caching, Options...). |
| [`reference-docs/plan/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan) | **Kế hoạch & Hồ sơ nghiệm thu Phase** | Nơi lưu trữ Kế hoạch triển khai tổng thể (`MASTER_PLAN_15_PHASES.md`) và **biên bản nghiệm thu chi tiết từng phase** (`PHASE-XX-ACCEPTANCE.md`) sau khi hoàn thành. |
| [`reference-docs/guides/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides) | **Tài liệu hướng dẫn kiểm thử Backend cho Dev** | Chứa các bài hướng dẫn kiểm thử toàn diện (`PHASE-XX-TEST-GUIDE.md`) sau khi triển khai xong mỗi phase (cURL, Postman/HTTP files, kịch bản Happy/Negative, seed data, troubleshooting). |
| [`docs/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/docs) | **Tài liệu Kiến trúc & Dự án Hệ thống** | Hệ thống hóa kiến trúc, thiết kế CSDL, quy chuẩn phát triển và lộ trình triển khai của chính mã nguồn dự án đang được lập trình. |

---

## 3. Định hướng Trọng tâm Kỹ thuật

- **Trọng tâm tuyệt đối:** Backend ASP.NET Core Web API, Entity Framework Core, SignalR, Identity & Security, Background Services.
- **Tiêu chuẩn chất lượng:** Mọi phase đều có **Checklist kiểm thử**, **Tài liệu nghiệm thu** trong [`reference-docs/plan/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/plan) và **Hướng dẫn kiểm thử toàn diện cho Dev** trong [`reference-docs/guides/`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides).
- **Phần mở rộng Frontend:** Sẽ chỉ triển khai khi có yêu cầu cụ thể.
- **Kỷ luật phát triển:** Push Git sau mỗi lần cập nhật mã nguồn lên repository từ xa.
