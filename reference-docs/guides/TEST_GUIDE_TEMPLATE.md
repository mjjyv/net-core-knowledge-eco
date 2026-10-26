# Mẫu Hướng Dẫn Kiểm Thử Backend Dành Cho Dev (Backend Testing Guide Template)

*Tài liệu này được lập và lưu tại `reference-docs/guides/PHASE-XX-TEST-GUIDE.md` sau khi hoàn thành từng phase.*

---

## 1. Mục Tiêu Kiểm Thử Của Phase

- **Phase:** Phase [Số Phase]: [Tên Phase]
- **Mục tiêu:** Kiểm thử và xác nhận toàn diện các tính năng Backend đã được triển khai, đảm bảo đáp ứng đúng các tiêu chí trong checklist của bài học tương ứng.
- **Yêu cầu môi trường:**
  - .NET 8 SDK đã cài đặt (`dotnet --version` $\ge 8.0$)
  - Cơ sở dữ liệu đang chạy (SQL Server / PostgreSQL / SQLite)
  - Ứng dụng Backend đang chạy tại `https://localhost:5001` (hoặc `http://localhost:5000`)

---

## 2. Chuẩn Bị Môi Trường & Khởi Chạy Backend

```bash
# Di chuyển vào thư mục WebAPI và khởi chạy
dotnet run --project src/Ecosystem.WebAPI
```

Đảm bảo Kestrel báo lắng nghe thành công tại `https://localhost:5001`.

---

## 3. Kịch Bản Kiểm Thử Đường Thuận (Happy Path)

### Ca kiểm thử 3.1: [Tên ca kiểm thử thuận]
- **Mục đích:** Kiểm tra luồng xử lý chuẩn với dữ liệu hợp lệ.
- **Lệnh thực thi cURL:**
  ```bash
  curl -X POST "https://localhost:5001/api/v1/..." \
    -H "Content-Type: application/json" \
    -d '{
      "field1": "value1",
      "field2": "value2"
    }'
  ```
- **Kết quả kỳ vọng (Expected Response):**
  - **HTTP Status:** `200 OK` hoặc `201 Created`
  - **Header `Location`:** (nếu có)
  - **Payload phản hồi:**
    ```json
    {
      "id": "...",
      "field1": "value1"
    }
    ```

---

## 4. Kịch Bản Kiểm Thử Đường Nghịch & Phòng Thủ (Negative Path)

### Ca kiểm thử 4.1: Kiểm tra dữ liệu rỗng hoặc sai định dạng (400 Bad Request)
- **Lệnh thực thi cURL:**
  ```bash
  curl -X POST "https://localhost:5001/api/v1/..." \
    -H "Content-Type: application/json" \
    -d '{}'
  ```
- **Kết quả kỳ vọng:**
  - **HTTP Status:** `400 Bad Request`
  - **Cấu trúc lỗi RFC 7807:**
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "Field1": ["Trường này là bắt buộc."]
      }
    }
    ```

### Ca kiểm thử 4.2: Kiểm tra truy cập tài nguyên không tồn tại (404 Not Found)
- **Lệnh cURL:**
  ```bash
  curl -X GET "https://localhost:5001/api/v1/.../00000000-0000-0000-0000-000000000000"
  ```
- **Kết quả kỳ vọng:** `404 Not Found`.

---

## 5. Kịch Bản Kiểm Thử Phân Quyền & Bảo Mật (Nếu Có)

### Ca kiểm thử 5.1: Truy cập API cần bảo mật khi chưa đăng nhập (401 Unauthorized)
- **Lệnh cURL:**
  ```bash
  curl -X POST "https://localhost:5001/api/v1/protected-endpoint"
  ```
- **Kết quả kỳ vọng:** `401 Unauthorized`.

### Ca kiểm thử 5.2: Truy cập API với Token sai quyền (403 Forbidden)
- **Lệnh cURL:**
  ```bash
  curl -X DELETE "https://localhost:5001/api/v1/protected-endpoint/..." \
    -H "Authorization: Bearer <Audience_Token>"
  ```
- **Kết quả kỳ vọng:** `403 Forbidden`.

---

## 6. Kiểm Tra Trực Tiếp Trong Cơ Sở Dữ Liệu (Database Verification)

Nếu phase có liên quan đến việc ghi dữ liệu vào CSDL, chạy câu truy vấn SQL sau để kiểm tra dữ liệu thực tế:

```sql
SELECT TOP 5 Id, Title, CreatedAt, IsDeleted FROM Articles ORDER BY CreatedAt DESC;
```

---

## 7. Hướng Dẫn Xử Lý Sự Cố Thường Gặp (Troubleshooting)

| Vấn đề / Lỗi gặp phải | Nguyên nhân có thể | Cách khắc phục |
|---|---|---|
| Lỗi SSL / HTTPS Certificate | Chứng chỉ phát triển chưa được tin cậy | Chạy lệnh `dotnet dev-certs https --trust` |
| Lỗi 500 Internal Server Error | Connection string sai hoặc chưa update migration | Kiểm tra file `appsettings.json` và chạy `dotnet ef database update` |
| Swagger không hiển thị endpoint | Quên thêm attribute `[ApiController]` hoặc route bị thiếu | Kiểm tra lại controller và restart ứng dụng |
