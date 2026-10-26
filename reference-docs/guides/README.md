# Trung Tâm Hướng Dẫn Kiểm Thử Backend Dành Cho Developer (`reference-docs/guides`)

Thư mục này là nơi lưu trữ **toàn bộ tài liệu hướng dẫn kiểm thử chức năng Backend** được tạo ra sau mỗi khi hoàn thành một phase trong lộ trình 15 phase nền tảng của dự án **The Ecosystem**.

---

## 1. Mục Đích & Nguyên Tắc Hướng Dẫn Kiểm Thử

1. **Tập trung tuyệt đối vào Backend:** Mọi hướng dẫn đều hướng tới việc kiểm thử trực tiếp các chức năng phía máy chủ thông qua giao diện dòng lệnh (CLI), cURL, Swagger UI, và HTTP REST Client (.http / Postman).
2. **Toàn diện & Thực tế (Comprehensive & Reproducible):** Mỗi tài liệu kiểm thử đều phải có:
   - Các lệnh cURL cụ thể kèm payload mẫu để copy-paste chạy ngay.
   - Kịch bản kiểm thử đường thuận (Happy Path - dữ liệu chuẩn, mã 200/201).
   - Kịch bản kiểm thử đường nghịch & phòng thủ (Negative Path - dữ liệu sai, rỗng, mã 400/404/409).
   - Kịch bản kiểm thử bảo mật & phân quyền (401 Unauthorized khi thiếu token, 403 Forbidden khi sai role).
   - Truy vấn SQL hoặc bước kiểm tra trực tiếp trạng thái dữ liệu trong CSDL.
   - Hướng dẫn xử lý sự cố thường gặp (Troubleshooting Guide).

---

## 2. Quy Cách Đặt Tên & Cấu Trúc Tài Liệu Từng Phase

Mỗi phase khi hoàn thành sẽ có tài liệu kiểm thử tương ứng được tạo tại đây:

- `PHASE-01-TEST-GUIDE.md`: Hướng dẫn kiểm tra biên dịch, Project References và Domain Entities.
- `PHASE-02-TEST-GUIDE.md`: Hướng dẫn kiểm tra Kestrel Web API, Dependency Injection Lifetimes, `/health`.
- `PHASE-03-TEST-GUIDE.md`: Hướng dẫn kiểm thử Controllers Cây tri thức và RESTful Routing.
- `PHASE-04-TEST-GUIDE.md`: Hướng dẫn kiểm thử DTOs, Model Validation (400 Bad Request) & Swagger.
- `PHASE-05-TEST-GUIDE.md`: Hướng dẫn kiểm tra kết nối CSDL, EF Core Migrations và Schema tables.
- `PHASE-06-TEST-GUIDE.md`: Hướng dẫn kiểm thử CRUD Bài viết Markdown, quan hệ phân cấp và LINQ queries.
- `PHASE-07-TEST-GUIDE.md`: Hướng dẫn chạy Unit Tests tầng Service, Repository và Unit of Work.
- `PHASE-08-TEST-GUIDE.md`: Hướng dẫn kiểm thử Xác thực JWT (Register, Login, Token, Refresh Token).
- `PHASE-09-TEST-GUIDE.md`: Hướng dẫn kiểm thử Phân quyền (Roles `Host`/`Presenter`/`Audience`, Policies).
- `PHASE-10-TEST-GUIDE.md`: Hướng dẫn kiểm thử Upload/Download file và Options Pattern.
- `PHASE-11-TEST-GUIDE.md`: Hướng dẫn kiểm thử Phân trang `PagedResult<T>`, Tìm kiếm & Sắp xếp động.
- `PHASE-12-TEST-GUIDE.md`: Hướng dẫn kiểm thử Global Exception Handling (RFC 7807) & Request Logging.
- `PHASE-13-TEST-GUIDE.md`: Hướng dẫn kiểm thử Swagger XML, CORS Policies và chạy ứng dụng trong Docker.
- `PHASE-14-TEST-GUIDE.md`: Hướng dẫn kiểm thử SignalR Hubs thời gian thực và API gói `bundle`.
- `PHASE-15-TEST-GUIDE.md`: Hướng dẫn chạy trọn bộ kiểm thử E2E Postman, nạp Seed Data và kịch bản demo bảo vệ.

---

## 3. Mẫu Chuẩn Cho Tài Liệu Kiểm Thử (Xem Template)

Vui lòng tham khảo chi tiết tại [`TEST_GUIDE_TEMPLATE.md`](file:///home/debv/Documents/life_it_antigravity/acad_net-core/theEcosystem/reference-docs/guides/TEST_GUIDE_TEMPLATE.md) để tuân thủ định dạng chuẩn khi viết hướng dẫn kiểm thử cho từng phase.
