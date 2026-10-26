# Software Engineering Standards & Guidelines
### 1.0 Xác định yêu cầu và đặc tả hệ thống (Requirement Engineering & Specifications)
1.1 Phân tích và làm rõ bài toán nghiệp vụ
1.1.1 Thu thập yêu cầu cốt lõi (Functional Requirements) và chỉ số vận hành (Non-Functional Requirements: SLA, latency, throughput).
1.1.2 Xác định ranh giới hệ thống: những gì làm và những gì dứt khoát không làm (In-scope vs. Out-of-scope).
1.1.3 Phân tích ca sử dụng biên và các trường hợp lỗi tiềm ẩn ngay từ giai đoạn ý niệm (Failure Modes & Edge Cases).
1.2 Thiết kế hợp đồng giao tiếp và mô hình hóa dữ liệu (Contract-First Design)
1.2.1 Chuẩn hóa lược đồ dữ liệu và giao thức giao tiếp (Schema-first: OpenAPI, Protobuf, JSON Schema).
1.2.2 Định nghĩa trạng thái và vòng đời dữ liệu (State Machines & Lifecycle Modeling).
1.2.3 Thiết lập cơ chế kiểm thử tính tương thích ngược (Backward Compatibility Contracts).

### 2.0 Kiến trúc hệ thống và thiết kế phần mềm (System Architecture & Software Design)
2.1 Nguyên tắc phân tách trách nhiệm và mô-đun hóa
2.1.1 Tách biệt lõi xử lý nghiệp vụ (Core Engine) khỏi giao diện hiển thị và giao thức I/O (Ports & Adapters / Clean Architecture).
2.1.2 Thiết lập nguyên tắc nguồn chân lý duy nhất (Single Source of Truth) cho dữ liệu và cấu hình.
2.1.3 Trừu tượng hóa các phụ thuộc ngoại vi (Dependency Inversion) để phục vụ việc cô lập và giả lập khi kiểm thử.
2.2 Thiết kế phòng thủ và chịu lỗi tầng hệ thống (Defensive System Design)
2.2.1 Áp dụng mô hình ngắt mạch (Circuit Breaker), giới hạn tốc độ (Rate Limiting) và cơ chế giảm tải dần (Graceful Degradation).
2.2.2 Kiểm soát vòng đời tiến trình, giải phóng tài nguyên và xử lý tín hiệu hủy bỏ (Process Lifecycle, Tree-kill, Clean Shutdown).
2.2.3 Chiến lược xử lý sự cố đồng thời: cô lập luồng, loại trừ tranh chấp (Race Conditions) và bế tắc (Deadlocks).
2.3 Bảo mật kiến trúc theo mặc định (Security by Default)
2.3.1 Xác thực đầu vào chặt chẽ tại biên (Input Validation, Sanitization, Allowlist over Blocklist).
2.3.2 Quản lý danh tính, phân quyền tối thiểu (Principle of Least Privilege) và mã hóa dữ liệu tại chỗ/khi truyền tải.
2.3.3 Ngăn ngừa rò rỉ dữ liệu nhạy cảm qua log, telemetry hoặc API payload bên thứ ba.

### 3.0 Kỷ luật triển khai và tiêu chuẩn mã nguồn (Implementation Discipline & Code Standards)
3.1 Kỷ luật mã hóa và quản lý trạng thái
3.1.1 Tuân thủ phong cách lập trình thống nhất (Style Guides, Linter rules, Idiomatic Patterns).
3.1.2 Xử lý lỗi tường minh, cấm hoàn toàn việc bỏ qua lỗi ngầm (Explicit Error Handling, No Silent Failures).
3.1.3 Quản trị bất biến (Immutability) và hạn chế tối đa trạng thái biến đổi toàn cục (Global Mutable State).
3.2 Thiết kế khả năng quan sát nội tại (Built-in Observability)
3.2.1 Ghi log có cấu trúc (Structured Logging) kèm theo ngữ cảnh tương quan (Trace ID, Request ID).
3.2.2 Thu thập chỉ số vận hành (Metrics: Counter, Gauge, Histogram) tại các điểm nghẽn trọng yếu.
3.2.3 Đo lường phân tán (Distributed Tracing) để định vị độ trễ xuyên suốt các dịch vụ.

### 4.0 Chiến lược kiểm thử đa tầng (Comprehensive Testing Strategy)
4.1 Kiểm thử cô lập và kiểm thử đơn vị (Unit Testing)
4.1.1 Bao phủ kiểm thử dựa trên rủi ro: ưu tiên thuật toán phức tạp, chuyển đổi dữ liệu và trạng thái biên.
4.1.2 Áp dụng kiểm thử dựa trên thuộc tính (Property-based Testing) và kiểm thử làm mờ (Fuzzing) cho các parser/format nhạy cảm.
4.2 Kiểm thử tích hợp và hợp đồng giao tiếp (Integration & Contract Testing)
4.2.1 Kiểm thử giao tiếp liên tiến trình và gọi mạng qua môi trường hộp cát (Sandbox/Containerized Test Environments).
4.2.2 Xác thực tính bất biến của hợp đồng API giữa các thành phần độc lập (Consumer-Driven Contract Testing).
4.3 Kiểm thử độ tin cậy và hiệu năng (Reliability & Performance Testing)
4.3.1 Kiểm thử tải, độ ổn định đường dài và độ biến thiên tài nguyên (Load, Soak, and Stress Testing).
4.3.2 Kiểm thử mô phỏng sự cố thực tế (Chaos Engineering: mạng chập chờn, disk full, process crash bất ngờ).

### 5.0 Tự động hóa phân phối và quản trị phát hành (Automation & Release Governance)
5.1 Kỷ luật quản lý phiên bản mã nguồn (VCS Discipline)
5.1.1 Quy ước nhánh phát triển và lịch sử cam kết rõ ràng (Trunk-based or Feature Branching, Conventional Commits).
5.1.2 Thực thi rà soát mã nguồn bắt buộc (Peer Review Checklist) kết hợp agent kiểm tra tự động trước khi hợp nhất.
5.2 Quy trình tích hợp và phân phối tự động (CI/CD Pipelines)
5.2.1 Chặn đứng mã lỗi tại cổng: tự động chạy linting, quét lỗ hổng bảo mật (SAST/DAST) và kiểm thử hồi quy.
5.2.2 Khóa cứng phụ thuộc và chữ ký số gói phát hành để bảo vệ chuỗi cung ứng (Dependency Pinning, Software Bill of Materials - SBOM).
5.3 Chiến lược triển khai an toàn (Safe Deployment Strategies)
5.3.1 Triển khai theo từng giai đoạn: phát hành Canary, triển khai Blue-Green hoặc kiểm soát qua cờ tính năng (Feature Flags).
5.3.2 Tự động kích hoạt cơ chế quay lui (Automated Rollback) khi tỷ lệ lỗi vượt ngưỡng cho phép.

### 6.0 Vận hành, bảo trì và quản trị tiến hóa (Operations & Evolutionary Governance)
6.1 Quản trị vận hành thực tế
6.1.1 Thiết lập ngưỡng cảnh báo thông minh dựa trên độ lệch chuẩn, tránh gây báo động giả (Alert Fatigue).
6.1.2 Quy trình xử lý sự cố chuẩn hóa (Standard Operating Procedures / Runbooks) và hậu kiểm không đổ lỗi (Blameless Post-mortem).
6.2 Kiểm soát nợ kỹ thuật và quản trị tài liệu
6.2.1 Ghi nhận các quyết định kiến trúc quan trọng bằng văn bản (Architecture Decision Records - ADR).
6.2.2 Lên kế hoạch tái cấu trúc định kỳ và cập nhật phiên bản thư viện cốt lõi theo vòng đời bảo mật.
6.2.3 Định nghĩa hồ sơ lập luận đảm bảo an toàn/chất lượng (Assurance Cases) để duy trì tính nhất quán khi quy mô dự án mở rộng.
