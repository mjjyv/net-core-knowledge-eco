# Đặc Tả Giao Diện API & Thời Gian Thực (RESTful & SignalR)

Tài liệu này đặc tả toàn bộ các điểm cuối **RESTful Web API** và các kênh kết nối thời gian thực **ASP.NET Core SignalR Hubs** của hệ thống The Ecosystem.

---

## 1. Chuẩn Quy Tắc Thiết Kế RESTful Web API

1. **Định dạng dữ liệu:** Chuẩn `application/json; charset=utf-8`.
2. **Tiêu chuẩn mã trạng thái HTTP (HTTP Status Codes):**
   - `200 OK`: Yêu cầu GET/PUT thành công.
   - `201 Created`: Tạo mới tài nguyên thành công (kèm header `Location`).
   - `204 No Content`: Xóa tài nguyên thành công.
   - `400 Bad Request`: Dữ liệu đầu vào vi phạm validation.
   - `401 Unauthorized`: Chưa cung cấp Bearer token hoặc token không hợp lệ.
   - `403 Forbidden`: Người dùng không đủ quyền hạn (Role/Policy) truy cập tài nguyên.
   - `404 Not Found`: Không tìm thấy tài nguyên yêu cầu.
   - `409 Conflict`: Xung đột dữ liệu (ví dụ: trùng lặp Slug hoặc Email).
   - `500 Internal Server Error`: Lỗi máy chủ chưa được xử lý.
3. **Chuẩn báo lỗi đồng nhất (RFC 7807 Problem Details):**
   Mọi lỗi trả về đều tuân thủ cấu trúc:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
     "title": "Validation Error",
     "status": 400,
     "detail": "Một hoặc nhiều trường dữ liệu không hợp lệ.",
     "instance": "/api/v1/articles",
     "errors": {
       "Title": ["Tiêu đề không được để trống."]
     }
   }
   ```

---

## 2. Danh Mục Các Điểm Cuối RESTful API Cốt Lõi

### 2.1. Nhóm Xác Thực & Tài Khoản (`/api/v1/auth`)

| Phương thức | Đường dẫn | Phân quyền | Mô tả chức năng |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Anonymous | Đăng ký tài khoản người dùng mới. |
| `POST` | `/api/v1/auth/login` | Anonymous | Đăng nhập bằng Email/Password, nhận JWT và Refresh Token. |
| `POST` | `/api/v1/auth/refresh-token` | Anonymous | Làm mới phiên đăng nhập bằng Refresh Token. |
| `GET` | `/api/v1/auth/me` | `[Authorize]` | Lấy thông tin tài khoản hiện tại từ Token Claims. |

---

### 2.2. Nhóm Cây Tri Thức (`/api/v1/taxonomy`)

| Phương thức | Đường dẫn | Phân quyền | Mô tả chức năng |
|---|---|---|---|
| `GET` | `/api/v1/taxonomy/topics` | Anonymous | Lấy toàn bộ cây Topics kèm Aspects và Categories. |
| `POST` | `/api/v1/taxonomy/topics` | `[Authorize(Roles = "Host")]` | Tạo Topic mới. |
| `POST` | `/api/v1/taxonomy/topics/{topicId}/aspects` | `[Authorize(Roles = "Host")]` | Thêm Aspect vào Topic. |
| `POST` | `/api/v1/taxonomy/aspects/{aspectId}/categories` | `[Authorize(Roles = "Host")]` | Thêm Category vào Aspect. |

---

### 2.3. Nhóm Quản Trị Bài Viết Tri Thức (`/api/v1/articles`)

| Phương thức | Đường dẫn | Phân quyền | Mô tả chức năng |
|---|---|---|---|
| `GET` | `/api/v1/articles` | Anonymous | Lấy danh sách bài viết có phân trang, tìm kiếm, lọc và sắp xếp. |
| `GET` | `/api/v1/articles/{id}` | Anonymous | Lấy chi tiết bài viết theo ID hoặc Slug. |
| `POST` | `/api/v1/articles` | `[Authorize(Roles = "Presenter,Host")]` | Tạo mới bài viết Markdown. |
| `PUT` | `/api/v1/articles/{id}` | `[Authorize(Roles = "Presenter,Host")]` | Cập nhật nội dung và siêu dữ liệu bài viết. |
| `DELETE` | `/api/v1/articles/{id}` | `[Authorize(Roles = "Host")]` | Xóa mềm bài viết. |
| `GET` | `/api/v1/articles/{id}/bundle` | Anonymous | Xuất bản gói đồng bộ liên thông sâu (`ecosystem://bundle`). |

---

### 2.4. Nhóm Tải Tệp Đính Kèm (`/api/v1/files`)

| Phương thức | Đường dẫn | Phân quyền | Mô tả chức năng |
|---|---|---|---|
| `POST` | `/api/v1/files/upload` | `[Authorize]` | Tải file đính kèm (ảnh minh họa, file `.md`) với giới hạn 5MB. |
| `GET` | `/api/v1/files/{fileName}` | Anonymous | Tải về file đính kèm với MIME Type thích hợp. |

---

## 3. Cơ Chế Liên Kết Sâu Hệ Điều Hành (`ecosystem://`)

Điểm cuối `/api/v1/articles/{id}/bundle` trả về gói JSON tự chứa đầy đủ nội dung bài viết, mã Markdown thô, danh sách ảnh đính kèm và siêu dữ liệu để ứng dụng Desktop PKM hoặc Script tự động hóa đồng bộ về máy cục bộ:

```json
{
  "articleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Kiến trúc ASP.NET Core Clean Architecture",
  "slug": "kien-truc-asp-net-core-clean-architecture",
  "markdownContent": "# Kiến trúc...\n\nNội dung chi tiết...",
  "categoryPath": "DotNetCore/BackendArchitecture/WebAPI",
  "tags": ["dotnet", "clean-architecture", "csharp"],
  "attachments": [
    {
      "fileName": "clean-architecture-diagram.png",
      "downloadUrl": "https://api.ecosystem.local/api/v1/files/clean-architecture-diagram.png"
    }
  ],
  "exportedAt": "2026-10-27T05:00:00Z"
}
```

---

## 4. Đặc Tả Kênh Cộng Tác Thời Gian Thực (SignalR Hubs)

### 4.1. `CollaborationHub` (`/hubs/collaboration`)
Phục vụ tính năng nhận diện trạng thái đa người dùng trên tài liệu:

- **Kết nối & Nhóm:**
  - `JoinDocument(string articleId)`: Thêm connection hiện tại vào Group theo `articleId`.
  - `LeaveDocument(string articleId)`: Rời khỏi Group tài liệu.
- **Phát tán trạng thái con trỏ (Presence):**
  - Client gửi: `SendCursorPosition(CursorPositionDto dto)` (Tọa độ X, Y, Tỷ lệ phần trăm trên DOM node).
  - Hub phân phối tới các client khác trong Group: `ReceiveCursorPosition(UserCursorDto dto)`.
- **Bong bóng chat ngữ cảnh (Inline Floating Message):**
  - Client gửi: `SendFloatingMessage(FloatingMessageDto dto)` (Nội dung, Tọa độ neo theo khối).
  - Hub phân phối: `ReceiveFloatingMessage(FloatingMessageDto dto)`.

### 4.2. `ClassroomHub` (`/hubs/classroom`)
Phục vụ điều phối phiên học tập trực tuyến theo thời gian thực:

- `JoinClassroom(string roomCode)`: Tham gia phòng học theo vai trò (`Host`, `Presenter`, `Audience`).
- `BroadcastSlideChange(SlideStateDto state)`: Giảng viên chuyển trang trình chiếu, toàn bộ học viên lập tức đồng bộ theo thời gian thực.
- `HandRaise(string userId)`: Học viên giơ tay phát biểu.
- Quản lý Heartbeat và tự động dọn dẹp kết nối rớt mạng sau 30 giây (Eviction Policy).
