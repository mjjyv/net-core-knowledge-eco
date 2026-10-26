# 1. Triết lý vận hành vô hạn (The Infinite Runtime Principles)

Để AI agent vận hành độc lập suốt nhiều tháng mà không bị ảo giác, suy thoái chất lượng hay tắc nghẽn bộ nhớ, toàn bộ hệ thống phải tuân thủ 4 nguyên lý nền tảng:

- **Tách rời tuyệt đối giữa tính toán và ký ức (Compute-Memory Decoupling):** Cửa sổ ngữ cảnh (Context Window) của LLM chỉ đóng vai trò bộ nhớ RAM tạm thời. Bộ nhớ chân lý (Source of Truth) nằm 100% tại hệ thống tệp tin tĩnh của kho mã nguồn. Không bao giờ duy trì một phiên làm việc (session) kéo dài qua nhiều tác vụ; mỗi tác vụ là một phiên tính toán phi trạng thái (Stateless Compute).
- **Hợp đồng là chân lý duy nhất (Contract-First Anchorage):** Mọi giao tiếp giữa các module, dịch vụ hoặc tầng kiến trúc phải được đóng băng bằng lược đồ dữ liệu và đặc tả giao tiếp chuẩn mực (OpenAPI, Protobuf, JSON Schema) trước khi viết mã logic. Agent bị cấm suy đoán cấu trúc dữ liệu của thành phần khác.
- **Cô lập ranh giới ảnh hưởng (Minimal Blast Radius):** Triển khai kiến trúc Clean Architecture / Ports & Adapters nhằm tách biệt lõi nghiệp vụ khỏi cơ sở dữ liệu, framework và giao thức I/O. Mỗi tác vụ chỉ cho phép can thiệp vào một danh sách tệp xác định trước (Whitelist), chặn tuyệt đối việc sửa đổi lan sang các tệp không liên quan.
- **Bảo vệ kiến trúc bằng hàm mục tiêu tự động (Architectural Fitness Functions):** Sử dụng các công cụ kiểm tra tự động (như ArchUnit, dependency-cruiser, linters) tích hợp tại môi trường cục bộ và đường ống CI/CD nhằm ngăn chặn hiện tượng phụ thuộc ngược, mã độc hại hoặc nợ kỹ thuật ngay khi agent vừa tạo mã.

---

# 2. Mô hình 4 tầng bộ nhớ ngoài (4-Tier Memory Architecture)

Hệ thống phân tầng bộ nhớ theo tần suất truy cập và khối lượng token nhằm tối ưu hóa chi phí nạp ngữ cảnh:

```plaintext
┌─────────────────────────────────────────────────────────────┐
│  L0: KERNEL MEMORY (< 1.500 tokens)                         │
│  Quy tắc bất biến, ranh giới an toàn, bản đồ chỉ mục        │
└──────────────────────────────┬──────────────────────────────┘
                               │ Nạp cố định vào System Prompt
┌──────────────────────────────▼──────────────────────────────┐
│  L1: ARCHITECTURAL MEMORY (1.500 - 2.500 tokens)            │
│  Mô hình C4, Bounded Contexts, Hồ sơ quyết định (ADRs)       │
└──────────────────────────────┬──────────────────────────────┘
                               │ Nạp theo phiên thiết kế / tính năng lớn
┌──────────────────────────────▼──────────────────────────────┐
│  L2: DOMAIN & TECHNICAL DEEP MEMORY (Theo nhu cầu: 1k - 3k)  │
│  Tài liệu SDLC 1.0 - 7.0, Hợp đồng API, Lược đồ DB, Runbooks │
└──────────────────────────────┬──────────────────────────────┘
                               │ Nạp động theo tác vụ nguyên tử
┌──────────────────────────────▼──────────────────────────────┐
│  L3: EPHEMERAL EXECUTION MEMORY (< 1.000 tokens)            │
│  Phiếu tác vụ hiện tại, active_context.md, bộ đệm nợ kỹ thuật│
└─────────────────────────────────────────────────────────────┘
  (Xả sạch và ghi nhận trạng thái sau mỗi chu kỳ thực thi)
```

---

| Tầng bộ nhớ | Phạm vi dữ liệu | Ngưỡng nạp Token | Thời điểm & Cách thức nạp |
| --- | --- | --- | --- |
| **L0: Bộ nhớ Định tuyến & Ràng buộc Gốc (Kernel Memory)** | Lệnh cấm tuyệt đối (không nuốt lỗi ngầm, cấm sửa file ngoài danh mục), tiêu chuẩn mã sạch, bản đồ điều hướng. | < 1.500 tokens | Nạp cố định vào `system_prompt` của tất cả các phiên làm việc. |
| **L1: Bộ nhớ Kiến trúc & Ranh giới (Architectural Memory)** | Bounded Contexts (DDD), biểu đồ kiến trúc mức cao C4 (HLD), các bản ghi quyết định kiến trúc cốt lõi (ADRs). | 1.500 – 2.500 tokens | Nạp khi khởi tạo Epic, tính năng mới hoặc phân rã thiết kế hệ thống. |
| **L2: Bộ nhớ Kỹ thuật Chuyên biệt (Domain Deep Memory)** | Tài liệu 7 giai đoạn SDLC (SRS, NFRs, ERD, OpenAPI specs, Testing Plans, Runbooks). | 1.000 – 3.000 tokens / lần | Nạp động theo nhu cầu (On-Demand) dựa trên module mà tác vụ đang xử lý. |
| **L3: Bộ nhớ Trạng thái Thực thi (Ephemeral Memory)** | Ticket tác vụ nguyên tử đang xử lý, con trỏ trạng thái `active_context.md`, bộ đệm ghi nhận nợ kỹ thuật. | < 1.000 tokens | Đọc và ghi liên tục; tự động xả sạch (Flush) sau khi hoàn tất tác vụ. |

---

# 3. Cấu trúc thư mục chuẩn hóa toàn diện (System Repo Layout)

Bộ khung kết hợp giữa quản trị bộ nhớ agent, 7 giai đoạn SDLC và kiến trúc Clean Architecture:

```plaintext
project-root/
│
├── .agent/                                  # [L0 - KERNEL MEMORY] Ký ức điều phối tác tử
│   ├── rules/
│   │   ├── 00-core-invariants.md            # Bất biến: cấm nuốt lỗi ngầm, nguyên tắc nguồn chân lý duy nhất
│   │   ├── 01-coding-standards.md           # Chuẩn Clean Code: SOLID, DRY, typing, linter
│   │   ├── 02-security-baseline.md          # An ninh: OWASP Top 10, xác thực biên, least privilege
│   │   └── 03-testing-pyramid.md            # Quy chuẩn kiểm thử: Unit, Integration, Contract, E2E
│   ├── index/
│   │   └── context_map.json                 # Bản đồ máy đọc định tuyến tác vụ đến tài liệu và mã nguồn
│   └── system_prompt.md                     # Lời nhắc hệ thống chuẩn hóa khi khởi động phiên
│
├── tasks/                                   # [L3 - WORKING MEMORY] Bộ nhớ thực thi tức thời
│   ├── roadmap.md                           # Bản đồ tiến độ dài hạn và danh mục Milestones
│   ├── active_context.md                    # Con trỏ phiên: Tác vụ đang chạy, blockers, trạng thái bàn giao
│   ├── queue/                               # Hàng đợi các phiếu tác vụ nguyên tử (Atomic Tickets)
│   │   └── TASK-2026-001_auth_jwt.md        # File ticket chi tiết
│   ├── active/                              # Thư mục chứa ticket đang thực thi trong phiên
│   ├── done/                                # Lưu trữ ticket đã hoàn tất kèm mã commit
│   └── technical_debt.md                    # Sổ ghi nợ kỹ thuật phát sinh (cấm sửa chéo)
│
├── docs/                                    # [L1 & L2 - PERSISTENT KNOWLEDGE] Kho tri thức sống
│   ├── 01-discovery/                        # SDLC 1: Lean Canvas, chân dung người dùng, báo cáo PoC
│   ├── 02-requirements/                     # SDLC 2: SRS, User Stories BDD, ma trận NFRs
│   │   ├── functional_specs.md              # Đặc tả chức năng chi tiết
│   │   └── quality_attributes_nfr.md        # Ma trận SLA/SLO, độ trễ p95/p99, Error Budget
│   ├── 03-architecture/                     # SDLC 3 (HLD): Kiến trúc mức cao, Bounded Contexts theo C4
│   │   ├── system-overview.c4.md            # Sơ đồ C4 Level 1 & Level 2
│   │   └── bounded-contexts.md              # Phân rã miền nghiệp vụ (DDD)
│   ├── 04-design-lld/                       # SDLC 3 (LLD): Thiết kế mức thấp, ERD, máy trạng thái
│   │   ├── database-erd.md                  # Sơ đồ quan hệ thực thể, phân vùng, chỉ mục
│   │   └── state-machines.md                # Biểu đồ chuyển đổi trạng thái giao dịch
│   ├── 05-contracts/                        # Nguồn chân lý giao tiếp bất biến
│   │   ├── openapi/                         # Đặc tả REST API chuẩn (YAML/JSON)
│   │   ├── protobuf/                        # Định nghĩa schema nhị phân gRPC
│   │   └── events/                          # Cấu trúc sự kiện Kafka/RabbitMQ Async Payloads
│   ├── 06-adrs/                             # Hồ sơ quyết định kiến trúc và đánh đổi kỹ thuật
│   │   ├── ADR-001-modular-monolith.md      # Lý do lựa chọn mô hình kiến trúc
│   │   └── ADR-002-postgres-saga-outbox.md  # Quyết định giao dịch phân tán
│   ├── 07-qa-testing/                       # SDLC 5: Kế hoạch kiểm thử, kịch bản Chaos, ma trận kiểm thử
│   └── 08-operations/                       # SDLC 6 & 7: Vận hành, SRE, FinOps, Runbooks
│       ├── runbooks/                        # Sổ tay quy trình ứng cứu sự cố chuẩn (SOPs)
│       ├── telemetry/                       # Cấu hình OpenTelemetry, Alerting, Logs
│       └── finops-budget.md                 # Định mức chi phí và hạn ngạch tài nguyên Cloud
│
├── src/                                     # MÃ NGUỒN CỐT LÕI (Clean / Hexagonal Architecture)
│   ├── core/                                # Tầng nghiệp vụ độc lập hoàn toàn (Domain Entities & Use Cases)
│   │   ├── domain/                          # Aggregates, Entities, Value Objects, Domain Events
│   │   └── usecases/                        # Logic điều phối nghiệp vụ
│   ├── ports/                               # Giao diện trừu tượng hóa (Interfaces)
│   │   ├── inbound/                         # Cổng vào: Interfaces cho Controllers gọi
│   │   └── outbound/                        # Cổng ra: Interfaces cho DB, Cache, External Services
│   └── adapters/                            # Hiện thực hóa công nghệ kỹ thuật cụ thể
│       ├── inbound/                         # HTTP Controllers, GraphQL Resolvers, Event Consumers
│       └── outbound/                        # SQL Repositories, Redis Cache, AWS S3 Client, Gateways
│
└── tests/                                   # BỘ KIỂM THỬ ĐA TẦNG ĐỘC LẬP
    ├── unit/                                # Kiểm thử hàm/logic cô lập (mili-giây)
    ├── integration/                         # Kiểm thử liên tầng với Testcontainers (DB, Queue)
    ├── contract/                            # Kiểm thử hợp đồng giao tiếp (Pact)
    ├── e2e/                                 # Kiểm thử luồng nghiệp vụ hoàn chỉnh
    └── fitness/                             # Kiểm thử bảo vệ kiến trúc (ArchUnit / Dependency Rules)
```

---

# 4. Quy chuẩn biểu mẫu máy đọc (Machine-Readable Schemas)

## 4.1. Con trỏ trạng thái thời gian thực (`tasks/active_context.md`)

Tệp này không được vượt quá 100 dòng, được ghi đè và làm mới liên tục:

```markdown
# ACTIVE CONTEXT POINTER

- **Current Session ID:** SESS-2026-0924-01
- **Current Task:** TASK-2026-089 (Process Checkout Use Case)
- **Status:** EXECUTING (Step 3: Implementation)
- **Active Files Locked:**
  - `src/core/usecases/process_checkout.py`
  - `src/ports/outbound/payment_gateway_port.py`
  - `tests/unit/usecases/test_process_checkout.py`
- **Blockers / Unresolved Questions:** None
- **Last Verification:** Unit test RED confirmed (pytest failed as expected with NotImplementedError)
- **Next Immediate Action:** Implement checkout validation logic and transaction state transition
```

---

## 4.2. Phiếu tác vụ nguyên tử (`tasks/queue/TASK-xxxx.md`)

Mỗi tác vụ tuân thủ tiêu chuẩn INVEST, can thiệp tối đa 2–3 tệp mã nguồn và 1 tệp kiểm thử:

```markdown
# ATOMIC TASK: TASK-2026-089

## 1. Metadata & Ràng buộc
- **SDLC Phase:** 4.4 (Backend Core Logic)
- **Quality Attributes:** Functional Suitability, Security
- **Prerequisite Tasks:** TASK-2026-088 (Database Migration) COMPLETED
- **Priority:** P1-High

## 2. Hợp đồng quy chiếu (Single Source of Truth)
- **API Contract:** `docs/05-contracts/openapi/billing-v1.yaml` (`POST /api/v1/billing/checkout`)
- **Arch Decision:** `docs/06-adrs/ADR-002-postgres-saga-outbox.md`
- **NFR Metrics:** Latency p95 < 250ms, bắt buộc hỗ trợ Idempotency-Key

## 3. Kiểm soát ranh giới ảnh hưởng (Blast Radius Control)
- **WHITELIST (Chỉ được phép sửa):**
  - `src/core/usecases/process_checkout.py`
  - `src/ports/outbound/payment_gateway_port.py`
  - `tests/unit/usecases/test_process_checkout.py`
- **BLACKLIST (Cấm tuyệt đối sửa đổi):**
  - `src/adapters/outbound/postgres_repo.py`
  - `docs/05-contracts/*`
  - Mọi tệp ngoài danh mục Whitelist

## 4. Tiêu chí nghiệm thu (BDD / Acceptance Criteria)
- Scenario 1: Giao dịch thành công
  - **Given:** Giỏ hàng hợp lệ, số dư tài khoản đủ, Idempotency-Key mới.
  - **When:** Kích hoạt usecase `ProcessCheckout`.
  - **Then:** Tạo giao dịch `PENDING`, phát sự kiện `CheckoutInitiated`, trả về HTTP 201.
- Scenario 2: Trùng lặp khóa giao dịch (Idempotent Request)
  - **Given:** Gửi request với Idempotency-Key đã tồn tại trong vòng 24 giờ.
  - **When:** Kích hoạt usecase `ProcessCheckout`.
  - **Then:** Không trừ tiền lần 2, trả về đúng bản ghi giao dịch trước đó kèm HTTP 200.
- Scenario 3: Lỗi cổng thanh toán bên thứ ba
  - **Given:** Cổng thanh toán ngoại vi phản hồi mã 503 hoặc Timeout.
  - **When:** Thực thi thanh toán.
  - **Then:** Kích hoạt Circuit Breaker, chuyển trạng thái giao dịch sang `FAILED_RETRYABLE`, không nuốt lỗi ngầm.

## 5. Cổng kiểm định đầu ra (Definition of Done - DoD)
- [ ] 100% kịch bản BDD trên được bao phủ bằng Unit Test độc lập.
- [ ] `flake8` và `mypy --strict` vượt qua không có lỗi typing hoặc style.
- [ ] Kiểm thử phụ thuộc kiến trúc (`pytest tests/fitness/`) xác nhận tầng core không import adapters.
```

---

## 4.3. Bản đồ định tuyến tri thức (`.agent/index/context_map.json`)

Cung cấp chỉ mục giúp agent định vị chính xác tài liệu cần đọc mà không cần quét toàn bộ kho lưu trữ:

```json
{
  "domains": {
    "billing": {
      "bounded_context": "docs/03-architecture/bounded-contexts.md#billing",
      "requirements": "docs/02-requirements/functional_specs.md#billing-checkout",
      "contracts": [
        "docs/05-contracts/openapi/billing-v1.yaml",
        "docs/05-contracts/events/billing_events.json"
      ],
      "adrs": [
        "docs/06-adrs/ADR-002-postgres-saga-outbox.md"
      ],
      "source_paths": {
        "core": "src/core/usecases/billing/",
        "ports": "src/ports/*/billing_*.py",
        "adapters": "src/adapters/*/billing_*.py"
      },
      "test_paths": [
        "tests/unit/usecases/billing/",
        "tests/integration/billing/"
      ]
    }
  }
}
```

---

## 4.4. Bản ghi quyết định kiến trúc (`docs/06-adrs/ADR-xxxx.md`)

Bảo vệ định hướng kỹ thuật không bị sai lệch qua thời gian dài:

```markdown
# ADR-002: Áp dụng Mô hình Outbox Pattern và Transactional Saga cho Khối Thanh toán

- **Trạng thái:** ACCEPTED
- **Ngày quyết định:** 2026-09-24
- **Bối cảnh:**
  Cần thực hiện cập nhật trạng thái đơn hàng cục bộ và gửi sự kiện thông báo sang Message Broker (Kafka) mà không gây bất đồng bộ dữ liệu khi xảy ra sự cố mạng.
- **Quyết định:**
  Triển khai Outbox Pattern trên PostgreSQL: Lưu sự kiện vào bảng `outbox_events` trong cùng một Database Transaction của nghiệp vụ. Một tiến trình Debezium (CDC) độc lập sẽ đẩy sự kiện lên Kafka.
- **Đánh đổi chấp nhận (Consequences):**
  - *Tích cực:* Đảm bảo tính nhất quán tuyệt đối giữa dữ liệu nội bộ và sự kiện phân tán; loại bỏ rủi ro 2PC.
  - *Tiêu cực:* Chấp nhận tính nhất quán cuối cùng (Eventual Consistency) phía người dùng; tăng độ trễ gửi message từ 50-100ms.
```

---

# 5. Quy trình thực thi tự hiệu chỉnh 5 bước (Self-Correcting Execution Protocol)

Mỗi tác vụ được thực thi độc lập trong một phiên tính toán tinh gọn theo chu trình khép kín:

```plaintext
[Bước 1: Bootstrap & Map]
   │  - Đọc L0 (.agent/rules) + context_map.json
   │  - Nạp Ticket (L3) + Hợp đồng API (L2)
   │  - Khóa tệp Whitelist; Chuyển Ticket sang tasks/active/
   ▼
[Bước 2: Test-Driven Anchorage]
   │  - Viết Unit/Integration Test dựa trên BDD Scenarios
   │  - Chạy test xác nhận trạng thái RED (Thất bại chuẩn xác)
   ▼
[Bước 3: Minimal Defensive Implementation]
   │  - Viết mã logic vừa đủ để test chuyển sang GREEN (YAGNI)
   │  - Xử lý lỗi tường minh, bảo vệ bất biến dữ liệu
   │  - Nếu thấy lỗi ngoài danh mục -> ghi vào tasks/technical_debt.md
   ▼
[Bước 4: Quality Gate & Architectural Fitness]
   │  - Chạy linter, strict type-checker, contract tests, fitness tests
   │  - Lỗi? -> Phân tích stack trace và tự sửa (Tối đa 3 vòng lặp)
   │  - Vượt quá 3 lần? -> Git rollback, ghi blocker, dừng phiên xin can thiệp
   ▼
[Bước 5: Handover & Context Flush]
   │  - Tạo commit Git chuẩn Conventional Commits
   │  - Chuyển Ticket sang tasks/done/, làm mới active_context.md
   │  - ĐÓNG HOÀN TOÀN PHIÊN (Purge 100% token)
```

---

## Chi tiết cơ chế tự phục hồi (Self-Healing Gate):

1. **Kiểm tra tự động tầng 1 (Cú pháp & Kiểu dữ liệu):** Thực thi `ruff check .` và `mypy --strict src/`. Nếu có cảnh báo, agent tự sửa ngay lập tức.
2. **Kiểm tra tự động tầng 2 (Tính đúng đắn logic & Hợp đồng):** Thực thi kiểm thử đơn vị và hợp đồng. Mã trả về phải khớp tuyệt đối với schema OpenAPI.
3. **Kiểm tra tự động tầng 3 (Ranh giới kiến trúc):** Chạy kiểm thử Fitness functions. Nếu phát hiện tệp thuộc `src/core/` import trực tiếp thư viện từ `src/adapters/` hoặc framework ngoài, hệ thống lập tức hủy mã vừa sinh.
4. **Quy tắc 3 lần thử (3-Strike Rule):**
    - Nếu sau 3 lần tự sửa mà mã nguồn vẫn không vượt qua bộ kiểm tra tự động, agent kích hoạt lệnh `git restore .` để hoàn nguyên về trạng thái commit sạch trước đó.
    - Agent ghi lại toàn bộ stack trace và phân tích nguyên nhân vào `tasks/active_context.md`, đánh dấu trạng thái `BLOCKED_AWAITING_HUMAN`, và dừng phiên để chống suy thoái hệ thống hoặc lặp vô tận.

---

# 6. Động cơ tiến hóa theo yêu cầu (Evolutionary Engine)

Khi người dùng cần bổ sung tính năng mới hoặc thay đổi bài toán nghiệp vụ, hệ thống nạp yêu cầu qua quy trình 3 giai đoạn mà không làm xáo trộn các tính năng đang vận hành:

```plaintext
[Yêu cầu tính năng mới từ người dùng]
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│ Giai đoạn A: Đồng hóa Yêu cầu & Thiết kế (SDLC 1 - 3)        │
│ 1. Cập nhật docs/02-requirements/functional_specs.md         │
│ 2. Đóng băng Contract mới tại docs/05-contracts/             │
│ 3. Lưu vết quyết định đánh đổi vào docs/06-adrs/             │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ Giai đoạn B: Phân rã Tác vụ Nguyên tử                       │
│ 1. Sinh các ticket TASK-xxxx.md với Whitelist file rõ ràng  │
│ 2. Cập nhật quan hệ phụ thuộc vào tasks/roadmap.md          │
│ 3. Đẩy ticket vào hàng đợi tasks/queue/                     │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│ Giai đoạn C: Thực thi Tự chủ Vô hạn                          │
│ - AI Agent tuần tự xử lý từng ticket theo quy trình 5 bước   │
│ - Chạy kiểm thử hồi quy (Regression Testing) toàn diện       │
│ - Tích hợp CI/CD và bàn giao bản phát hành an toàn           │
└──────────────────────────────────────────────────────────────┘
```

---

- **Bảo vệ mã nguồn cũ:** Toàn bộ tính năng cũ được bảo vệ bởi mạng lưới hợp đồng và các bài kiểm thử hồi quy tự động. Mã mới chỉ được mở rộng thông qua các Use Case mới và Adapter mới theo nguyên lý Open/Closed (SOLID) mà không sửa đổi mã nghiệp vụ đang chạy ổn định.
- **Duy trì tầm nhìn dài hạn:** Nhờ các tệp ADRs và NFRs matrix, dù trải qua hàng trăm chu kỳ phát triển với nhiều agent khác nhau, hệ thống vẫn duy trì tính nhất quán về kiến trúc, bảo mật và hiệu năng kỹ thuật như một thực thể duy nhất.


```
Hãy đi sâu vào nội dung mục 1. Triết lý vận hành vô hạn (The Infinite Runtime Principles), cung cấp decimal outline toàn diện xoay quanh First Principles, Leverage, Inversion.
```