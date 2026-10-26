# Tiêu Chuẩn Kỹ Thuật & Kỷ Luật Phát Triển Phần Mềm

Tài liệu này quy định các tiêu chuẩn kỹ thuật phần mềm, phong cách lập trình C# .NET 8, quy trình kiểm thử và kỷ luật quản trị mã nguồn áp dụng cho toàn bộ dự án **The Ecosystem**.

---

## 1. Tiêu Chuẩn Lập Trình C# .NET 8 (Clean Code Guidelines)

1. **Quy ước đặt tên (Naming Conventions):**
   - **PascalCase:** Tên Class, Record, Struct, Interface (`I...`), Method, Public Property, Enum.
   - **camelCase:** Tham số phương thức (method parameters), biến cục bộ (local variables).
   - **_camelCase (với dấu gạch dưới):** Private readonly fields được inject qua Constructor (ví dụ: `private readonly IArticleRepository _articleRepository;`).
   - **Hậu tố DTO / Request / Response:** `ArticleDto`, `CreateArticleRequest`, `LoginResponse`.
2. **Nullable Reference Types (`<Nullable>enable</Nullable>`):**
   - Mọi project đều bật nullable reference types. Không sử dụng `!` (null-forgiving operator) bừa bãi trừ khi đã kiểm tra null bảo đảm tuyệt đối.
3. **Bất biến & Phòng thủ (Immutability & Defensive Programming):**
   - Sử dụng `record` hoặc `readonly struct` cho các DTOs truyền nhận dữ liệu.
   - Kiểm tra điều kiện đầu vào (Guard Clauses) ở ngay đầu phương thức:
     ```csharp
     ArgumentNullException.ThrowIfNull(request);
     if (string.IsNullOrWhiteSpace(request.Title))
         throw new ValidationException("Tiêu đề không được để trống.");
     ```
4. **Không nuốt lỗi ngầm (No Silent Failures):**
   - Tuyệt đối cấm các khối `catch (Exception) { }` rỗng làm mất dấu vết lỗi. Mọi lỗi phải được ghi log hoặc rethrow theo mô hình ngoại lệ phù hợp.
5. **Xử lý Bất đồng bộ (Async/Await Best Practices):**
   - Mọi phương thức bất đồng bộ đều có hậu tố `Async` (ví dụ: `GetByIdAsync`, `CreateArticleAsync`).
   - Luôn tiếp nhận và chuyển tiếp `CancellationToken cancellationToken = default`.

---

## 2. Tiêu Chuẩn Phân Tách Trách Nhiệm (Clean Architecture & SOLID)

1. **Single Responsibility Principle (SRP):** Controller chỉ làm nhiệm vụ tiếp nhận HTTP request, gọi Service xử lý và trả về HTTP response. Mọi logic nghiệp vụ thuộc về tầng `Application`. Mọi truy xuất CSDL thuộc về tầng `Infrastructure`.
2. **Open/Closed Principle (OCP):** Mở rộng tính năng bằng cách bổ sung Service/Handler mới, tránh sửa đổi các class lõi đang chạy ổn định.
3. **Dependency Inversion Principle (DIP):** Phụ thuộc vào interface (`IArticleRepository`, `IUnitOfWork`), không phụ thuộc trực tiếp vào lớp cụ thể (`ArticleRepository`, `ApplicationDbContext`).

---

## 3. Chiến Lược Kiểm Thử Đa Tầng (Testing Strategy)

1. **Kiểm thử đơn vị (Unit Tests):**
   - Sử dụng framework **xUnit**, thư viện giả lập **Moq** và cú pháp assertions **FluentAssertions**.
   - Cấu trúc theo mẫu AAA: *Arrange - Act - Assert*.
   - Tập trung kiểm thử logic nghiệp vụ tại tầng `Application` và các quy tắc miền tại tầng `Domain`.
2. **Kiểm thử tích hợp (Integration Tests):**
   - Sử dụng `WebApplicationFactory<Program>` để kiểm thử toàn diện HTTP pipeline, Routing, Middleware và Model Validation.
3. **Kiểm thử API Thực nghiệm (Dev Verification):**
   - Mỗi phase sau khi hoàn thiện đều phải có file kịch bản kiểm thử:
     - cURL commands chạy trực tiếp từ Terminal.
     - Kịch bản Happy Path (dữ liệu hợp lệ, status 200/201).
     - Kịch bản Negative Path (dữ liệu sai, status 400/401/403/404/409).

---

## 4. Kỷ Luật Quản Trị Phiên Bản Mã Nguồn (Git Discipline)

1. **Quy tắc bắt buộc:** **Luôn thực hiện Push Git lên Remote Repository sau mỗi lần cập nhật mã nguồn.**
2. **Quy chuẩn thông điệp Cam kết (Conventional Commits):**
   - `feat:` Thêm tính năng mới (ví dụ: `feat(articles): implement CRUD endpoints with EF Core`).
   - `fix:` Sửa lỗi (ví dụ: `fix(auth): correct token expiration calculation`).
   - `docs:` Thêm hoặc cập nhật tài liệu (ví dụ: `docs(plan): add acceptance report for phase 01`).
   - `test:` Thêm hoặc sửa kiểm thử (ví dụ: `test(service): add unit tests for ArticleService`).
   - `refactor:` Tái cấu trúc mã nguồn mà không đổi logic (ví dụ: `refactor(di): extract service collection extensions`).
   - `chore:` Công việc phụ trợ, cập nhật cấu hình build/package (ví dụ: `chore(deps): add Swashbuckle OpenAPI`).
3. **Quy trình Push Git an toàn:**
   ```bash
   # 1. Kiểm tra trạng thái
   git status
   # 2. Biên dịch và kiểm thử trước khi commit
   dotnet build
   # 3. Add và Commit
   git add .
   git commit -m "feat(scope): detailed message"
   # 4. Push ngay lập tức lên remote
   git push origin main
   ```
