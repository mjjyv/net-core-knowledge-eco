---
role: acad
topic: it
---
Buổi 15: Kiểm thử, Báo cáo và Bảo vệ dự án

## Mục lục

```text
Buổi 15: Kiểm thử, Báo cáo và Bảo vệ dự án
│
├── 1. Lý thuyết nền tảng
│   ├── 1.1. Vì sao phải kiểm thử trước khi bảo vệ?
│   │   ├── 1.1.1. Kiểm thử là gì?
│   │   └── 1.1.2. Các loại kiểm thử cần làm trước buổi bảo vệ
│   ├── 1.2. Cấu trúc báo cáo đồ án
│   └── 1.3. Quy trình buổi bảo vệ
│
├── 2. Lý thuyết chính
│   ├── 2.1. Hướng dẫn trình bày
│   │   ├── 2.1.1. Checklist nhanh trước khi bảo vệ
│   │   └── 2.1.2. Kịch bản demo chuẩn
│   ├── 2.2. Tiêu chí đánh giá
│   │   ├── 2.2.1. Bảng điểm chi tiết
│   │   └── 2.2.2. Cách tối ưu điểm từng tiêu chí
│   ├── 2.3. Quy trình buổi bảo vệ
│   │   ├── 2.3.1. Trình bày & Demo
│   │   ├── 2.3.2. Phản biện
│   │   └── 2.3.3. Câu hỏi phản biện thường gặp
│   ├── 2.4. Ghi nhớ & Lỗi thường gặp
│   └── 2.5. Tổng kết học phần
│
├── 3. Ví dụ nền tảng
│   ├── 3.1. Checklist kiểm thử bằng Postman
│   ├── 3.2. Kịch bản demo mẫu (10 phút)
│   └── 3.3. Mẫu báo cáo đồ án
│
├── 4. Thực hành / Lab cơ bản
│   ├── 4.1. Demo
│   ├── 4.2. Bảo vệ dự án
│   └── 4.3. Phản biện
│
├── 5. Bài tập mở rộng
│   └── 5.1. Bài chuẩn bị (5 bài, giải chi tiết)
│
├── 6. Bảng thuật ngữ
│
└── 7. Cheatsheet
```

## 1. Lý thuyết nền tảng

### 1.1. Vì sao phải kiểm thử trước khi bảo vệ?

> **Ghi chú bổ sung (kiến thức bị thiếu trong file gốc):** File gốc nhảy thẳng vào "Checklist" mà không giải thích **vì sao** phải kiểm thử.

#### 1.1.1. Kiểm thử là gì?

**Kiểm thử (Testing)** là quá trình **chạy thử** phần mềm với các **tình huống đầu vào** khác nhau để xác nhận rằng nó hoạt động đúng như kỳ vọng — và phát hiện lỗi **trước khi** người khác (giảng viên, người dùng) phát hiện.

> **Triết lý:** *"Nếu bạn không test, người dùng sẽ test — và họ sẽ tìm ra bug vào lúc bạn không muốn nhất."*

#### 1.1.2. Các loại kiểm thử cần làm trước buổi bảo vệ

> **Ghi chú bổ sung:** File gốc chỉ đề cập "Test Happy Path, luồng dữ liệu sai, phân quyền" — mình mở rộng thành hệ thống phân loại rõ ràng.

| Loại test | Mục đích | Ví dụ |
|---|---|---|
| **Happy Path** | Luồng thành công — mọi thứ đúng | Login `admin/123` → nhận token |
| **Negative Test** | Luồng dữ liệu sai | Login `admin/wrongpass` → 401 |
| **Boundary Test** | Giá trị biên | `pageSize=0`, `pageSize=51` |
| **Authorization Test** | Phân quyền | User gọi API Admin → 403 |
| **Authentication Test** | Xác thực | Gọi API bảo mật không token → 401 |
| **Validation Test** | Kiểm tra validate | Tạo product thiếu `name` → 400 |
| **Integration Test** | Luồng liên hoàn | Đăng ký → Login → Tạo đơn hàng → Thanh toán |
| **Performance Test** | Hiệu năng cơ bản | Gọi API có phân trang 1000 bản ghi < 1s |
| **UI Test** | Giao diện | Click nút Login → hiển thị lỗi rõ ràng |

### 1.2. Cấu trúc báo cáo đồ án

> **Ghi chú bổ sung:** File gốc chỉ liệt kê 5 phần — mình mở rộng thành cấu trúc chi tiết.

Báo cáo đồ án chuẩn gồm **5 phần chính**:

```text
1. Mở đầu
   ├── Trang bìa (tên trường, đề tài, GVHD, thành viên)
   ├── Lời cảm ơn
   ├── Mục lục
   └── Giới thiệu đề tài (mục tiêu, phạm vi)

2. Cơ sở lý thuyết
   ├── Công nghệ sử dụng (.NET 8, EF Core, JWT...)
   ├── Kiến trúc tổng quan (N-tier / Clean Architecture)
   └── Các khái niệm nền tảng (REST, JWT, DI...)

3. Phân tích & Thiết kế
   ├── Yêu cầu chức năng (use cases)
   ├── Sơ đồ ERD (quan hệ các bảng)
   ├── Danh sách API Endpoints (bảng: Method | Route | Mô tả | Auth)
   ├── Sơ đồ kiến trúc (layers, DI flow)
   └── Thiết kế bảo mật (JWT flow, role matrix)

4. Triển khai
   ├── Môi trường cài đặt (Visual Studio, .NET SDK version)
   ├── Cấu trúc thư mục project
   ├── Ảnh chụp Swagger (tất cả endpoints)
   ├── Ảnh chụp Postman (test cases)
   ├── Ảnh chụp Frontend (nếu có)
   ├── Các đoạn code quan trọng (JWT, Middleware, Repository)
   └── Kết quả đạt được (số endpoint, tính năng)

5. Tổng kết
   ├── Ưu điểm
   ├── Hạn chế
   ├── Hướng phát triển
   └── Tài liệu tham khảo
```

### 1.3. Quy trình buổi bảo vệ

> **Ghi chú bổ sung:** File gốc không mô tả rõ timeline.

**Timeline chuẩn (20-30 phút/nhóm):**

```text
[0-2 phút]   Giới thiệu nhóm + đề tài
             → Không đọc slide, nói tự nhiên

[2-10 phút]  Trình bày slide
             → Kiến trúc, CSDL, các chức năng chính

[10-18 phút] Demo trực tiếp
             → Swagger/Postman hoặc UI (KHÔNG mở Visual Studio)
             → Test Happy Path + 1-2 case lỗi + phân quyền

[18-25 phút] Giảng viên đặt câu hỏi phản biện
             → Trả lời từng câu, không vòng vo

[25-30 phút] Nhận xét & điểm
```

## 2. Lý thuyết chính

### 2.1. Hướng dẫn trình bày

#### 2.1.1. Checklist nhanh trước khi bảo vệ

**Trước ngày bảo vệ 1-2 ngày:**

- [ ] **Test Happy Path** — mọi chức năng chính chạy đúng.
- [ ] **Test luồng dữ liệu sai** — nhập thiếu field, sai định dạng, sai kiểu.
- [ ] **Test phân quyền** — User gọi API Admin phải bị 403; không token phải bị 401.
- [ ] **Test qua cả Postman và Swagger** — phòng khi 1 trong 2 lỗi.

**Báo cáo đủ 5 phần** (xem mục 1.2):

- [ ] Mở đầu
- [ ] Cơ sở lý thuyết
- [ ] Phân tích & thiết kế (**kèm ERD + danh sách Endpoints**)
- [ ] Triển khai (**ảnh chụp** Swagger/Postman/Frontend)
- [ ] Tổng kết

**Chuẩn bị dữ liệu:**

- [ ] **Seed data đẹp** — không để dữ liệu rác (`asdf`, `test1`, `abc123`).
- [ ] Tên sản phẩm có ý nghĩa: `"iPhone 15 Pro Max"`, không phải `"sp1"`.
- [ ] Ảnh sản phẩm thật (nếu có upload).
- [ ] Tài khoản test có sẵn: `admin/123`, `user1/123`.

**Trước khi lên bảng 30 phút:**

- [ ] **Build lại project** — `dotnet build` phải pass 100%.
- [ ] **Kiểm tra Connection String** — trỏ đúng DB, DB đã có data.
- [ ] **Chạy thử app** trên máy sẽ demo (nếu được phép).
- [ ] **Backup source code** lên GitHub/GitLab — phòng máy hỏng.
- [ ] **Export Postman Collection** ra file JSON — import sẵn trên máy demo.
- [ ] **Ghi tài khoản demo ra giấy/note** — dán lên màn hình nếu được.

> **Lỗi thường gặp:** "Demo Effect" — chạy ở nhà được, lên lớp lỗi 500. Nguyên nhân thường do:
> - **Connection String** trỏ DB local nhưng máy demo không có DB đó.
> - **Port conflict** — port 5000 bị process khác chiếm.
> - **Migration chưa chạy** trên máy demo.
>
> **Cách phòng:** Trước khi lên bảng, chạy `dotnet ef database update` + `dotnet run` trên máy demo (nếu được phép), hoặc mang laptop riêng.

#### 2.1.2. Kịch bản demo chuẩn

> **Ghi chú bổ sung:** File gốc không cung cấp kịch bản mẫu.

**Kịch bản demo 8 phút (khuyến nghị):**

```text
[1] Mở Swagger UI (30 giây)
    → "Đây là Swagger, tự sinh từ code của nhóm"
    → Show tổng số endpoint: "Nhóm có 24 endpoints, chia 5 nhóm"

[2] Đăng ký tài khoản mới (1 phút)
    → POST /api/auth/register với data đẹp
    → Nhấn mạnh: validate email, password ≥ 8 ký tự

[3] Đăng nhập admin (1 phút)
    → POST /api/auth/login với admin/123
    → Copy token
    → "Đây là JWT, gồm 3 phần Header.Payload.Signature"
    → Mở jwt.io, paste token, show payload có role=Admin

[4] Test phân quyền (2 phút)
    → Gọi POST /api/products KHÔNG có token → 401
    → Gọi POST /api/products với token USER → 403
    → Gọi POST /api/products với token ADMIN → 201

[5] Test chức năng chính (2 phút)
    → GET /api/products?keyword=iphone&pageNumber=1&pageSize=5
    → Show response có PagedResponse đẹp
    → Show sorting: sortBy=price&isDescending=true

[6] Test chức năng phụ (1 phút)
    → Upload ảnh sản phẩm → nhận URL
    → Mở URL ảnh trong browser mới → show ảnh

[7] Test lỗi (30 giây)
    → Gọi POST /api/products thiếu name → 400 với message rõ ràng
    → Gọi GET /api/products/999999 → 404

[8] Kết thúc (30 giây)
    → "Đây là toàn bộ luồng. Nhóm sẵn sàng nhận câu hỏi."
```

> **Nguyên tắc vàng:** **KHÔNG mở Visual Studio** trong lúc demo trừ khi được yêu cầu. Trình bày như một **sản phẩm hoàn chỉnh** — giống cách developer thật demo sản phẩm cho khách hàng.

### 2.2. Tiêu chí đánh giá

#### 2.2.1. Bảng điểm chi tiết

| STT | Tiêu chí | Trọng số | Nội dung đánh giá |
|---|---|---|---|
| 1 | **Chất lượng sản phẩm** | **40%** | Chạy đúng, đủ chức năng, ít bug. RESTful đúng chuẩn, áp dụng Pagination, JWT. |
| 2 | **Chất lượng mã nguồn** | **20%** | Clean Code, tuân thủ Pattern (Repository, Service, DI), không hardcode. |
| 3 | **Chất lượng báo cáo** | **20%** | Trình bày rõ ràng, đủ nội dung, vẽ chuẩn sơ đồ ERD. |
| 4 | **Khả năng trình bày và phản biện** | **20%** | Trả lời trôi chảy, hiểu rõ nguyên lý, làm chủ công nghệ. |

**Tổng: 100%**

#### 2.2.2. Cách tối ưu điểm từng tiêu chí

> **Ghi chú bổ sung:** File gốc chỉ nêu tiêu chí — mình phân tích cách "ăn điểm".

**Tiêu chí 1 — Chất lượng sản phẩm (40%):**

- **Phải có:** Tất cả chức năng chính chạy được (không crash).
- **Điểm cộng:** Có thêm Pagination, Filtering, Sorting, Upload file, Refresh Token.
- **Tránh:** Bug rõ ràng (VD: tạo product trùng tên không báo lỗi), route không chuẩn REST.
- **Mẹo:** Demo luồng "khó" trước (upload, phân quyền), luồng "dễ" sau.

**Tiêu chí 2 — Chất lượng mã nguồn (20%):**

- **Phải có:** DI đầy đủ, Service/Repository tách bạch, Controller mỏng.
- **Điểm cộng:** Có Unit Test cho Service, dùng Options Pattern, xử lý exception tập trung.
- **Tránh:** `Console.WriteLine` sót, hardcode secret, try-catch rải rác, Controller > 200 dòng.
- **Mẹo:** Chuẩn bị sẵn 2-3 file để show khi được hỏi: `Program.cs`, 1 Service, 1 Repository.

**Tiêu chí 3 — Chất lượng báo cáo (20%):**

- **Phải có:** ERD đúng (có PK/FK, quan hệ), danh sách endpoint dạng bảng.
- **Điểm cộng:** Có ảnh chụp Swagger/Postman đẹp, format báo cáo chuyên nghiệp.
- **Tránh:** Copy-paste code dài dòng, ERD vẽ tay xấu, thiếu phần Tổng kết.
- **Mẹo:** Dùng công cụ vẽ ERD online (draw.io, dbdiagram.io) thay vì vẽ tay.

**Tiêu chí 4 — Trình bày & phản biện (20%):**

- **Phải có:** Nói rõ ràng, trả lời đúng câu hỏi.
- **Điểm cộng:** Trả lời có ví dụ, tự tin, thành thật khi không biết.
- **Tránh:** Đọc slide, nói lan man, cãi giảng viên, "em copy trên mạng".
- **Mẹo:** Chuẩn bị trước câu trả lời cho 10 câu hỏi phổ biến (xem mục 2.3.3).

### 2.3. Quy trình buổi bảo vệ

#### 2.3.1. Trình bày & Demo

- Mỗi nhóm: **trình bày + demo sản phẩm**, sau đó **trả lời câu hỏi phản biện**.
- Giảng viên hỏi để **xác định mức độ hiểu bài**.
- **Quy tắc:** Không mở Visual Studio đầu tiên — trình bày như **sản phẩm hoàn chỉnh** (UI hoặc Swagger/Postman).

#### 2.3.2. Phản biện

> **Ghi chú bổ sung:** File gốc không nêu cách xử lý khi bị hỏi khó.

**Nguyên tắc trả lời phản biện:**

1. **Lắng nghe hết câu hỏi** — không ngắt lời giảng viên.
2. **Nhắc lại câu hỏi** để xác nhận hiểu đúng: *"Ý thầy là tại sao nhóm chọn JWT thay vì Cookie, đúng không ạ?"*
3. **Trả lời thẳng vào ý chính** — không vòng vo.
4. **Nếu không biết:**
   - ❌ Không nên: *"Em không biết"* (cụt lủng).
   - ✅ Nên: *"Phần này nhóm em chưa tìm hiểu sâu, nhưng em đoán là... Em sẽ về kiểm tra lại."*
5. **Nếu câu hỏi ngoài phạm vi đồ án:** Thành thật: *"Nhóm em chưa làm phần này, đây là hướng phát triển trong báo cáo."*

#### 2.3.3. Câu hỏi phản biện thường gặp

> **Ghi chú bổ sung:** File gốc không liệt kê — mình tổng hợp từ kinh nghiệm.

| Câu hỏi | Gợi ý trả lời |
|---|---|
| **"Tại sao dùng JWT thay vì Session/Cookie?"** | Vì API là stateless, phù hợp mobile app, scale ngang dễ dàng. JWT không cần lưu state server-side. |
| **"JWT có nhược điểm gì?"** | Không revoke được ngay (phải đợi hết hạn), token bị lộ thì nguy hiểm → cần HTTPS + thời gian sống ngắn + Refresh Token. |
| **"Tại sao dùng Repository Pattern?"** | Tách business logic khỏi truy cập dữ liệu, dễ test (mock được), dễ đổi ORM sau này. |
| **"`AddScoped` / `AddTransient` / `AddSingleton` khác gì nhau?"** | Singleton: 1 instance suốt app. Scoped: 1 instance/request. Transient: instance mới mỗi lần inject. DbContext nên Scoped. |
| **"`[Authorize]` khác `[AllowAnonymous]` thế nào?"** | `[Authorize]` yêu cầu token hợp lệ. `[AllowAnonymous]` bỏ qua `[Authorize]` cho endpoint cụ thể. |
| **"401 khác 403 thế nào?"** | 401 = chưa xác thực (không token/token sai). 403 = đã xác thực nhưng không đủ quyền. |
| **"Tại sao cần DTO thay vì trả Entity?"** | Tránh lộ schema, tránh circular reference, dễ version API, ẩn field nhạy cảm (password hash). |
| **"CORS để làm gì?"** | Bảo vệ của browser — cho phép domain khác gọi API. Không phải của backend (Postman vẫn gọi được). |
| **"Migration khác `EnsureCreated` thế nào?"** | Migration version hóa schema, hỗ trợ update dần. `EnsureCreated` chỉ tạo DB từ đầu, không update. |
| **"Làm sao xử lý exception tập trung?"** | Global Exception Middleware — bắt mọi unhandled exception, trả JSON chuẩn, log lỗi. |
| **"Tại sao dùng `AsNoTracking()`?"** | Tắt ChangeTracker cho query chỉ đọc — giảm bộ nhớ, tăng tốc. |
| **"Pagination `Skip/Take` có cần `OrderBy` không?"** | Có — SQL Server bắt buộc `ORDER BY` với `OFFSET/FETCH`, nếu không sẽ lỗi runtime. |
| **"Tại sao dùng Options Pattern thay vì `IConfiguration`?"** | Strongly-typed, có IntelliSense, compiler kiểm tra, dễ refactor. |
| **"Làm sao bảo vệ secret trong `appsettings.json`?"** | Dùng User Secrets (dev), Environment Variables (prod), không commit lên Git. |

### 2.4. Ghi nhớ & Lỗi thường gặp

> **Ghi nhớ:**
>
> - **Thành thật:** Chưa làm được chức năng nào thì nói thẳng, nêu hướng giải quyết. Không quanh co.
> - **Bình tĩnh khi có lỗi:** Exception khi demo — mở log, xem lỗi ở đâu. **Kỹ năng debug trực tiếp được đánh giá cao**.

> **Lỗi thường gặp:**
>
> - **Demo Effect:** Chạy ở nhà được, lên lớp lỗi 500 → kiểm tra Connection String, build lại trước khi lên bảng.
> - **Quên tài khoản admin** → ghi sẵn ra file text trên màn hình.
> - **Không hiểu code mình viết** (copy từ mạng/AI) → cần nắm chắc luồng chạy các đoạn code cốt lõi.

> **Ghi chú bổ sung — Cách debug khi demo bị lỗi:**
>
> **Nếu Swagger trả 500:**
> 1. Mở browser DevTools → Network → xem Response body.
> 2. Nếu có `traceId` → mở log server (Console hoặc file) → tìm theo `traceId`.
> 3. Nói với giảng viên: *"Đây là lỗi X do Y, nhóm em sẽ sửa Z."* → **không hoảng**.
>
> **Nếu API không gọi được (CORS, 404):**
> 4. Kiểm tra URL có đúng không (port, route).
> 5. Nếu CORS → mở DevTools Console xem lỗi cụ thể.
> 6. Nếu vẫn không được → **chuyển sang Postman** để chứng minh API vẫn chạy.
>
> **Nếu sập app hoàn toàn:**
> 7. Nhìn cửa sổ Console/terminal nơi `dotnet run` — lỗi hiện ngay đó.
> 8. Nếu đã tắt → mở lại, chạy `dotnet run`, đọc lỗi.
> 9. Trung thực: *"App bị lỗi khởi động do X, nhóm em đã từng gặp và fix bằng Y."*

### 2.5. Tổng kết học phần

> **Chúc mừng hoàn thành học phần Lập trình .NET (ASP.NET Core Web API)**
>
> Từ kiến thức C# cơ bản, các bạn đã xây dựng được **Backend API hiện đại, bảo mật và chuẩn kiến trúc** — đủ tự tin bước vào dự án thực tế.
>
> Hành trang này đủ để các bạn tiếp tục nghiên cứu sâu hơn (**Microservices, gRPC, SignalR**) hoặc tham gia các dự án tại doanh nghiệp.
>
> **Chúc các bạn bảo vệ đồ án thành công!**

## 3. Ví dụ nền tảng

### 3.1. Checklist kiểm thử bằng Postman

**Tạo Collection "Final Test" với các request:**

```text
Final Test/
├── Auth/
│   ├── [01] Register - Happy Path
│   ├── [02] Register - Duplicate email (400)
│   ├── [03] Login - Admin (200)
│   ├── [04] Login - Wrong password (401)
│   ├── [05] Refresh Token (200)
│   └── [06] Refresh với token sai (401)
├── Products/
│   ├── [07] GET list - Happy Path (200)
│   ├── [08] GET list - Pagination (200)
│   ├── [09] GET list - Search + Filter (200)
│   ├── [10] GET by ID - Tồn tại (200)
│   ├── [11] GET by ID - Không tồn tại (404)
│   ├── [12] POST - Admin (201)
│   ├── [13] POST - User (403)
│   ├── [14] POST - Không token (401)
│   ├── [15] POST - Thiếu name (400)
│   ├── [16] PUT - Admin (200)
│   └── [17] DELETE - Admin (204)
└── Upload/
    ├── [18] Upload JPG (200)
    ├── [19] Upload quá 5MB (400)
    └── [20] Upload file .exe (400)
```

**Mẫu test script cho Request [03] Login:**

```javascript
// Tests tab của Postman
pm.test("Status 200", () => pm.response.to.have.status(200));

pm.test("Response có token", () => {
    const data = pm.response.json();
    pm.expect(data).to.have.property('token');
    pm.expect(data.token).to.be.a('string');
    pm.collectionVariables.set('adminToken', data.token);
});

pm.test("Token có 3 phần", () => {
    const token = pm.response.json().token;
    pm.expect(token.split('.')).to.have.lengthOf(3);
});
```

**Mẫu test script cho Request [13] POST - User (403):**

```javascript
pm.test("Status 403 Forbidden", () => pm.response.to.have.status(403));
pm.test("Body có message", () => {
    pm.expect(pm.response.json()).to.have.property('message');
});
```

### 3.2. Kịch bản demo mẫu (10 phút)

Xem chi tiết tại mục **2.1.2** — kịch bản 8 phút có thể mở rộng thành 10 phút bằng cách thêm phần Q&A nội bộ giữa các thành viên.

### 3.3. Mẫu báo cáo đồ án (tóm tắt)

**Trang bìa:**

```text
TRƯỜNG ĐẠI HỌC [TÊN TRƯỜNG]
KHOA CÔNG NGHỆ THÔNG TIN
─────────────

BÁO CÁO ĐỒ ÁN
LẬP TRÌNH .NET (ASP.NET CORE WEB API)

Đề tài: XÂY DỰNG HỆ THỐNG BÁN HÀNG E-COMMERCE

GVHD: TS. Nguyễn Văn A
Nhóm sinh viên thực hiện:
  1. Trần Văn B — 20210001
  2. Lê Thị C — 20210002
  3. Phạm Văn D — 20210003

TP. Hồ Chí Minh, tháng 12 năm 2025
```

**Mẫu bảng danh sách Endpoints (Phần 3 — Phân tích & Thiết kế):**

| STT | Method | Endpoint | Mô tả | Auth |
|---|---|---|---|---|
| 1 | POST | `/api/auth/register` | Đăng ký tài khoản | ❌ |
| 2 | POST | `/api/auth/login` | Đăng nhập, nhận JWT | ❌ |
| 3 | POST | `/api/auth/refresh` | Lấy JWT mới từ RefreshToken | ❌ |
| 4 | GET | `/api/products` | Danh sách sản phẩm (phân trang, lọc, search, sort) | ❌ |
| 5 | GET | `/api/products/{id}` | Chi tiết sản phẩm | ❌ |
| 6 | POST | `/api/products` | Tạo sản phẩm | ✅ Admin |
| 7 | PUT | `/api/products/{id}` | Cập nhật sản phẩm | ✅ Admin |
| 8 | DELETE | `/api/products/{id}` | Xóa sản phẩm | ✅ Admin |
| 9 | POST | `/api/products/upload` | Upload ảnh sản phẩm | ✅ Admin |
| 10 | GET | `/api/orders` | Danh sách đơn hàng (của user hiện tại) | ✅ User |
| 11 | POST | `/api/orders` | Tạo đơn hàng | ✅ User |
| 12 | GET | `/api/orders/{id}` | Chi tiết đơn hàng | ✅ User |
| 13 | PUT | `/api/orders/{id}/status` | Cập nhật trạng thái đơn | ✅ Admin |

**Mẫu phần ERD (mô tả bằng text):**

```text
Users (1) ──── (n) Orders (1) ──── (n) OrderItems (n) ──── (1) Products (n) ──── (1) Categories
  │                                                                                      │
  │                                                                                      │
  └──── (n) RefreshTokens                                                     
```

**Mẫu phần Triển khai (ảnh chụp):**

```text
[Hình 4.1]: Giao diện Swagger UI với 13 endpoints
[Hình 4.2]: Test Login trả về JWT token
[Hình 4.3]: Test phân quyền — User gọi API Admin bị 403
[Hình 4.4]: Test phân trang — pageNumber=2, pageSize=5
[Hình 4.5]: Upload ảnh thành công, trả về URL
[Hình 4.6]: Giao diện Frontend (nếu có)
```

## 4. Thực hành / Lab cơ bản

### 4.1. Demo

**Chuẩn bị:**

- Mở sẵn Swagger UI và Postman Collection.
- Đóng các tab không liên quan (Facebook, YouTube...).
- Tắt thông báo (Notifications) — `Win + A` → Focus Assist.
- Chuẩn bị nước uống, giấy note tài khoản demo.

**Thực hiện:** Theo kịch bản mục 2.1.2.

### 4.2. Bảo vệ dự án

**Trước khi vào phòng:**

- [ ] Máy đã sạc đầy / cắm sạc.
- [ ] Đã build lại project.
- [ ] DB đã có seed data.
- [ ] Đã test login `admin/123` thành công.
- [ ] Đã export Postman Collection.
- [ ] Đã mở sẵn file báo cáo PDF.

**Trong phòng:**

- [ ] Bình tĩnh, tự tin.
- [ ] Trả lời thẳng, không vòng vo.
- [ ] Nếu không biết → thừa nhận + nêu hướng.
- [ ] Ghi chú lại nhận xét của giảng viên.

### 4.3. Phản biện

**Chiến thuật trả lời:**

1. **Chủ động chuẩn bị** — ôn lại 14 câu hỏi ở mục 2.3.3.
2. **Chia người trả lời** — ai làm phần nào trả lời phần đó.
3. **Hỗ trợ đồng đội** — nếu bạn cùng nhóm bí, người khác có thể bổ sung (nhưng không giành).
4. **Không tranh cãi** — nếu giảng viên nói sai, nhẹ nhàng: *"Dạ, nhóm em hiểu là X, có thể nhóm em chưa đúng, thầy góp ý thêm giúp em."*

## 5. Bài tập mở rộng

### 5.1. Bài chuẩn bị

---

#### Bài 1: Kiểm thử Happy Path + Negative + Authorization

**Yêu cầu:** Test Happy Path, luồng dữ liệu sai, và phân quyền (User gọi API Admin) qua Postman/Swagger.

**Giải thích chi tiết:**

**Happy Path (luồng thành công):**

- Đăng ký → Login → Lấy token → CRUD product → Nhận response 200/201.
- **Mục đích:** Chứng minh chức năng chính chạy được.

**Negative Test (luồng dữ liệu sai):**

| Case | Input | Kỳ vọng |
|---|---|---|
| Thiếu field bắt buộc | `{ "price": 100 }` (thiếu `name`) | 400 + message rõ |
| Email sai format | `{ "email": "abc" }` | 400 |
| Password yếu | `{ "password": "123" }` | 400 |
| Giá âm | `{ "price": -10 }` | 400 |
| ID không tồn tại | `GET /api/products/999999` | 404 |
| Sai password | `admin/wrongpass` | 401 |
| Token hết hạn | Token cũ > 60 phút | 401 |

**Authorization Test (phân quyền):**

| Case | Token | Kỳ vọng |
|---|---|---|
| Gọi API bảo mật, không token | Không | 401 |
| Gọi API bảo mật, token sai | `"Bearer abc"` | 401 |
| Gọi API Admin bằng token User | User token | 403 |
| Gọi API Admin bằng token Admin | Admin token | 200 |
| Gọi API public | Không | 200 |

**Cách làm:**

1. Tạo Postman Collection có sẵn các request (xem mục 3.1).
2. Chạy từng request, ghi lại kết quả.
3. **Nếu case nào fail** → ghi vào file `bugs.md`, sửa trước khi bảo vệ.

---

#### Bài 2: Seed data đẹp

**Yêu cầu:** Seed data đẹp, không để dữ liệu rác (`asdf`, `test1`).

**Giải thích chi tiết:**

**Cách seed data chuẩn — dùng `DbInitializer`:**

```csharp
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        // Categories
        var categories = new[]
        {
            new Category { Name = "Điện thoại", Description = "Smartphone các loại" },
            new Category { Name = "Laptop", Description = "Máy tính xách tay" },
            new Category { Name = "Phụ kiện", Description = "Tai nghe, sạc, ốp lưng" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Products
        var products = new[]
        {
            new Product { Name = "iPhone 15 Pro Max 256GB", Price = 34990000m, CategoryId = categories[0].Id, Stock = 50 },
            new Product { Name = "Samsung Galaxy S24 Ultra", Price = 31990000m, CategoryId = categories[0].Id, Stock = 30 },
            new Product { Name = "MacBook Pro M3 14 inch", Price = 49990000m, CategoryId = categories[1].Id, Stock = 15 },
            new Product { Name = "Dell XPS 13 Plus", Price = 35990000m, CategoryId = categories[1].Id, Stock = 20 },
            new Product { Name = "AirPods Pro 2", Price = 5990000m, CategoryId = categories[2].Id, Stock = 100 },
            new Product { Name = "Sạc Anker 65W", Price = 590000m, CategoryId = categories[2].Id, Stock = 200 }
        };
        context.Products.AddRange(products);

        // Users (password đã hash)
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            context.Users.AddRange(
                new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Admin",
                    Email = "admin@shop.com"
                },
                new User
                {
                    Username = "user1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "User",
                    Email = "user1@gmail.com"
                }
            );
        }
        await context.SaveChangesAsync();
    }
}
```

**Gọi trong `Program.cs`:**

```csharp
var app = builder.Build();

// Seed data khi khởi động
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);
}

app.Run();
```

**Nguyên tắc seed data đẹp:**

- **Tên thật, có ý nghĩa** — `"iPhone 15 Pro Max"` chứ không phải `"sp1"`.
- **Giá hợp lý** — `34990000` chứ không phải `1` hoặc `999999999`.
- **Số lượng vừa phải** — 20-50 bản ghi (đủ để test phân trang).
- **Có quan hệ** — sản phẩm thuộc category, order có order items.
- **Có ảnh** — nếu có upload, seed sẵn vài ảnh đẹp.

---

#### Bài 3: Kiểm tra Connection String + Build lại project

**Yêu cầu:** Build lại project, kiểm tra Connection String trước khi lên bảng (tránh Demo Effect).

**Giải thích chi tiết:**

**Bước 1 — Kiểm tra Connection String:**

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Checklist:**

- [ ] `Server` đúng (localhost hay máy khác).
- [ ] `Database` tồn tại (đã chạy `update-database`).
- [ ] `Trusted_Connection` hoặc `User Id/Password` đúng.
- [ ] `TrustServerCertificate=True` (nếu SQL Server local).
- [ ] **Không có** connection string thật trong `appsettings.json` (chỉ dev).

**Bước 2 — Build lại:**

```bash
# Xóa bin/obj cũ để build sạch
dotnet clean
dotnet build -c Release
```

**Kỳ vọng:** `Build succeeded. 0 Error(s), 0 Warning(s)`.

**Bước 3 — Test trước khi lên bảng:**

```bash
# Chạy thử
dotnet run

# Mở browser
# http://localhost:5000/swagger
# Login thử: admin/123
```

**Bước 4 — Backup:**

```bash
git add .
git commit -m "chore: chuẩn bị bảo vệ"
git push origin main
```

**Nếu phát hiện lỗi trước khi bảo vệ:**

```bash
# Xem log chi tiết
dotnet run --verbosity detailed

# Nếu lỗi connection → sửa appsettings rồi chạy lại
```

---

#### Bài 4: Backup source code + Export Postman Collection

**Yêu cầu:** Backup source code lên Github/GitLab, export sẵn Postman Collection.

**Giải thích chi tiết:**

**Backup source code:**

```bash
# Kiểm tra trạng thái
git status

# Nếu còn file chưa commit → commit hết
git add .
git commit -m "chore: final version trước bảo vệ"

# Push lên remote
git push origin main

# Tạo tag để đánh dấu phiên bản bảo vệ
git tag -a v1.0-defense -m "Phiên bản bảo vệ đồ án"
git push origin v1.0-defense
```

**Export Postman Collection:**

1. Mở Postman → Collections → chọn Collection của bạn.
2. Click chuột phải → **Export**.
3. Chọn **Collection v2.1** → **Export**.
4. Lưu file `ECommerce-API.postman_collection.json` vào thư mục `docs/` trong repo.
5. Commit:

```bash
git add docs/ECommerce-API.postman_collection.json
git commit -m "docs: export Postman collection"
git push
```

**Export Environment (nếu có dùng):**

1. Postman → Environments → chọn environment → **Export**.
2. Lưu file `ECommerce-API.postman_environment.json`.

> **Lợi ích:** Nếu máy demo lỗi, có thể import Collection trên máy khác, chỉ cần đổi `baseUrl` là chạy được.

---

#### Bài 5: Chuẩn bị câu trả lời phản biện

**Yêu cầu:** Chuẩn bị trước câu trả lời cho 14 câu hỏi phổ biến (mục 2.3.3).

**Giải thích chi tiết:**

**Cách chuẩn bị hiệu quả:**

1. **Mỗi thành viên tự viết câu trả lời** cho 5 câu hỏi liên quan đến phần mình làm.
2. **Họp nhóm** — mỗi người trình bày, người khác góp ý.
3. **Ghi ra file `Q&A.md`** trong repo:

```markdown
# Câu hỏi phản biện — Chuẩn bị

## Q1: Tại sao dùng JWT thay vì Session/Cookie?
**Trả lời:** API là stateless, phù hợp mobile app. JWT lưu ở client, server không cần lưu state. Scale ngang dễ (nhiều server không cần share session store).

**Nếu hỏi thêm:** "Vậy JWT có nhược điểm gì?"
→ Không revoke được ngay, phải đợi hết hạn. Giải pháp: Refresh Token + thời gian sống ngắn (15-60 phút).

## Q2: AddScoped khác AddSingleton thế nào?
**Trả lời:** 
- Singleton: 1 instance suốt app.
- Scoped: 1 instance/request.
- Transient: instance mới mỗi lần inject.
DbContext nên Scoped vì không thread-safe.

...
```

**Kỹ thuật trả lời phản biện:**

| Tình huống | Cách xử lý |
|---|---|
| **Biết chắc** | Trả lời ngay, ngắn gọn, có ví dụ. |
| **Biết nhưng chưa chắc** | *"Theo em hiểu là X, nhưng em chưa chắc lắm. Thầy có thể góp ý thêm?"* |
| **Không biết** | *"Phần này nhóm em chưa tìm hiểu, đây là hướng phát triển. Em sẽ về tìm hiểu thêm ạ."* |
| **Câu hỏi mẹo/bẫy** | Bình tĩnh, hỏi lại để chắc: *"Ý thầy là X, đúng không ạ?"* |
| **Bị chỉ ra lỗi** | *"Dạ, đúng là nhóm em chưa xử lý case này. Em cảm ơn thầy đã góp ý."* |

---

## 6. Bảng thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| **Happy Path** | Luồng thành công — mọi input đúng, output đúng kỳ vọng. |
| **Negative Test** | Test với dữ liệu sai — kiểm tra app xử lý lỗi đúng. |
| **Boundary Test** | Test giá trị biên (0, max, max+1). |
| **Authorization Test** | Test phân quyền — User có gọi được API Admin không. |
| **Authentication Test** | Test xác thực — không token có bị 401 không. |
| **Demo Effect** | Hiện tượng app chạy ở nhà được nhưng lỗi khi demo. |
| **Seed Data** | Dữ liệu mẫu được nạp sẵn khi khởi động app. |
| **Migration** | Công cụ EF Core version hóa schema CSDL. |
| **Connection String** | Chuỗi kết nối tới CSDL — chứa server, database, auth. |
| **ERD** | Entity Relationship Diagram — sơ đồ quan hệ các bảng. |
| **Endpoint** | Điểm cuối API — 1 route + 1 HTTP method. |
| **JWT** | JSON Web Token — token 3 phần `Header.Payload.Signature`. |
| **401 Unauthorized** | Chưa xác thực — không token hoặc token sai. |
| **403 Forbidden** | Đã xác thực nhưng không đủ quyền. |
| **CORS** | Cross-Origin Resource Sharing — cơ chế bảo vệ của browser. |
| **Pagination** | Phân trang — chia dữ liệu thành nhiều trang. |
| **Repository Pattern** | Lớp trung gian truy cập dữ liệu. |
| **Service Pattern** | Lớp chứa business logic. |
| **DI (Dependency Injection)** | Tiêm phụ thuộc — container tự khởi tạo service. |
| **Clean Code** | Code dễ đọc, dễ hiểu, dễ bảo trì. |
| **Hardcode** | Giá trị cố định trong code, không nên cấu hình động. |
| **Refactor** | Tái cấu trúc code — cải thiện chất lượng mà không đổi hành vi. |
| **Bug** | Lỗi trong code. |
| **Debug** | Gỡ lỗi — tìm nguyên nhân bug. |
| **Postman Collection** | Tập hợp request Postman — chia sẻ được. |
| **Environment (Postman)** | Bộ biến môi trường của Postman (`baseUrl`, `token`). |
| **Swagger UI** | Giao diện web test API tự sinh từ code. |
| **Tag (Git)** | Đánh dấu phiên bản trong Git (v1.0, v1.1...). |
| **Release** | Chế độ build tối ưu (không debug symbol). |
| **Production** | Môi trường chạy thật (cho user). |
| **Staging** | Môi trường kiểm thử giống production. |
| **Development** | Môi trường dev cục bộ. |

## 7. Cheatsheet

### 7.1. Timeline chuẩn bị bảo vệ

```text
Trước 1 TUẦN:
  ├─ Hoàn thiện tất cả chức năng
  ├─ Chạy toàn bộ checklist kiểm thử
  ├─ Fix bug
  └─ Seed data đẹp

Trước 3 NGÀY:
  ├─ Hoàn thiện báo cáo (5 phần)
  ├─ Chuẩn bị slide
  ├─ Viết Q&A.md
  └─ Họp nhóm tổng duyệt

Trước 1 NGÀY:
  ├─ Build lại -c Release
  ├─ Test login + 3 API chính
  ├─ Export Postman Collection
  ├─ Backup source code
  └─ Sạc laptop

Sáng ngày bảo vệ:
  ├─ Chạy thử app
  ├─ Mở sẵn Swagger + Postman
  ├─ Tắt thông báo
  ├─ Mang theo giấy note tài khoản
  └─ Tâm lý thoải mái
```

### 7.2. Checklist 1 trang trước khi lên bảng

**Chức năng:**

- [ ] Login `admin/123` thành công.
- [ ] Login `user1/123` thành công.
- [ ] Test 1 API bảo mật bằng token Admin → 200.
- [ ] Test 1 API bảo mật bằng token User → 403.
- [ ] Test 1 API bảo mật không token → 401.
- [ ] Test 1 API lỗi (thiếu field) → 400.
- [ ] Test 1 API phân trang → 200 + `PagedResponse`.

**Chuẩn bị:**

- [ ] DB có seed data.
- [ ] Swagger load được.
- [ ] Postman Collection đã import.
- [ ] Báo cáo PDF sẵn sàng.
- [ ] Laptop đã sạc / cắm sạc.

### 7.3. Bảng điểm — tự đánh giá trước khi nộp

| Tiêu chí | Trọng số | Tự đánh giá (0-10) | Điểm dự kiến |
|---|---|---|---|
| Chất lượng sản phẩm | 40% | ___/10 | ___ |
| Chất lượng mã nguồn | 20% | ___/10 | ___ |
| Chất lượng báo cáo | 20% | ___/10 | ___ |
| Trình bày & phản biện | 20% | ___/10 | ___ |
| **TỔNG** | **100%** | | **___/10** |

> **Mục tiêu:** ≥ 8.0/10 → Xuất sắc. 7.0-7.9 → Tốt. 6.0-6.9 → Khá.

### 7.4. 14 câu hỏi phản biện — đáp án nhanh

| #   | Câu hỏi                              | Đáp án ngắn                                             |
| --- | ------------------------------------ | ------------------------------------------------------- |
| 1   | JWT vs Session?                      | Stateless, scale ngang, phù hợp mobile.                 |
| 2   | Nhược điểm JWT?                      | Không revoke ngay, cần Refresh Token.                   |
| 3   | Tại sao Repository?                  | Tách data access, dễ test, dễ đổi ORM.                  |
| 4   | 3 loại DI lifetime?                  | Singleton (app), Scoped (request), Transient (mỗi lần). |
| 5   | `[Authorize]` vs `[AllowAnonymous]`? | Yêu cầu token vs bỏ qua auth.                           |
| 6   | 401 vs 403?                          | Chưa xác thực vs đã xác thực nhưng thiếu quyền.         |
| 7   | Tại sao DTO?                         | Tránh lộ schema, tránh circular ref, ẩn field nhạy cảm. |
| 8   | CORS là gì?                          | Bảo vệ của browser, cho phép cross-origin.              |
| 9   | Migration vs EnsureCreated?          | Version hóa vs chỉ tạo từ đầu.                          |
| 10  | Global Exception Middleware?         | Bắt mọi lỗi, trả JSON chuẩn.                            |
| 11  | `AsNoTracking()`?                    | Tắt ChangeTracker cho query chỉ đọc.                    |
| 12  | Pagination cần `OrderBy` không?      | Có — SQL Server bắt buộc với OFFSET/FETCH.              |
| 13  | Options Pattern vs IConfiguration?   | Strongly-typed, IntelliSense, dễ refactor.              |
| 14  | Bảo vệ secret?                       | User Secrets (dev), Env Vars (prod).                    |

### 7.5. Xử lý sự cố khi demo

| Sự cố                     | Xử lý nhanh                                                                  |
| ------------------------- | ---------------------------------------------------------------------------- |
| **App không start**       | Xem log terminal → đọc lỗi → nói: *"Lỗi X do Y, nhóm em đã từng fix bằng Z"* |
| **Swagger 500**           | Mở DevTools → xem response → tìm theo `traceId` trong log                    |
| **CORS lỗi**              | Chuyển sang Postman để chứng minh API chạy được                              |
| **DB không kết nối**      | Kiểm tra Connection String, chạy `update-database`                           |
| **Port bị chiếm**         | `netstat -ano \| findstr :5000` → `taskkill /PID x /F`                       |
| **Token hết hạn**         | Login lại lấy token mới                                                      |
| **Ảnh upload không hiện** | Kiểm tra `UseStaticFiles()` + đường dẫn `wwwroot`                            |
| **Quên tài khoản demo**   | Đọc từ giấy note đã chuẩn bị                                                 |
| **Laptop sập nguồn**      | Cắm sạc, khởi động lại, chạy lại app (đã build sẵn)                          |
| **Máy demo khác máy dev** | Import Postman Collection, đổi `baseUrl`                                     |

### 7.6. Câu thần chú khi bảo vệ

```text
1. "Đây là sản phẩm của nhóm em."
   → Tự tin, không rụt rè.

2. "Swagger tự sinh từ code, không phải hardcode."
   → Nhấn mạnh chất lượng code.

3. "Nhóm em áp dụng JWT để bảo mật, phân quyền Admin/User."
   → Nhấn mạnh bảo mật.

4. "Đây là kiến trúc N-tier, DI đầy đủ."
   → Nhấn mạnh kiến trúc.

5. "Phần này nhóm em chưa làm, đây là hướng phát triển."
   → Thành thật khi không biết.

6. "Em cảm ơn thầy/cô đã góp ý."
   → Khiêm tốn khi bị chỉ lỗi.
```

