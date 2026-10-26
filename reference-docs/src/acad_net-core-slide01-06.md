---
role: acad
topic: it
---
# 📘 Lập trình .NET (ASP.NET Core Web API)

---

# Buổi 1: Giới thiệu Học Phần & Ôn Tập C#

---

### 📌 1. Lý thuyết

#### 1.1. Giới thiệu học phần

**Mục tiêu:**  
Xây dựng được một ứng dụng Web API hoàn chỉnh bằng **ASP.NET Core**.

**Công nghệ sử dụng:**

| Thành phần | Công nghệ |
|------------|-----------|
| Framework  | .NET 8 (LTS) |
| IDE        | Visual Studio 2022 |
| Database   | SQL Server 2022 |
| API Testing| Postman / Swagger |
| ORM        | Entity Framework Core |

---

#### 1.2. Tổng quan .NET, CLR, BCL, .NET SDK

**🔹 .NET là gì?**

- Nền tảng phát triển phần mềm **đa nền tảng** — Windows, Linux, macOS
- Hỗ trợ nhiều loại ứng dụng: Web API, Console, Desktop, Mobile, Cloud
- **Mã nguồn mở**, được phát triển bởi Microsoft và cộng đồng

**🔹 Các thành phần chính:**

| Thành phần | Vai trò |
|------------|---------|
| **CLR** (Common Language Runtime) | Môi trường thực thi mã .NET, quản lý bộ nhớ qua Garbage Collector, bảo mật, xử lý ngoại lệ runtime |
| **BCL** (Base Class Library) | Tập hợp thư viện dựng sẵn: `System.IO`, `System.Collections`, `System.Linq`, ... |
| **.NET SDK** (Software Development Kit) | Bộ công cụ xây dựng & chạy ứng dụng: Roslyn compiler, `dotnet CLI`, build tools và templates |

> ⚡ **Ghi nhớ:** SDK = Tools để **BUILD** | Runtime = Môi trường để **CHẠY**

---

#### 1.3. Ôn tập C#

##### 📌 OOP trong C#

| Khái niệm | Ứng dụng trong Web API |
|-----------|------------------------|
| Class / Object | Model, Controller, Service |
| Interface | Contract cho Repository, Service |
| Inheritance | Base Controller, Base Entity |
| Encapsulation | Property với getter/setter |

**Ví dụ:**

```csharp
public interface IStudentService
{
    List<Student> GetAll();
}

public class StudentService : IStudentService
{
    public List<Student> GetAll() => _students;
}
```

✅ **Best Practice:** Lập trình theo Interface – không phụ thuộc vào class cụ thể.

---

##### 📌 Collections & Generic

| Collection | Dùng khi |
|------------|----------|
| `List<T>` | Danh sách có thứ tự, cần thêm/xóa |
| `Dictionary<K, V>` | Tra cứu nhanh theo key |
| `IEnumerable<T>` | Trả dữ liệu từ Repository/Service |
| `IQueryable<T>` | Build query động, chưa thực thi |
| `HashSet<T>` | Tập hợp phần tử duy nhất |

**Ví dụ Generic – Response Wrapper:**

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
}
```

💡 **Ghi nhớ:** `IEnumerable<T>` dùng để trả dữ liệu. `List<T>` dùng khi cần thao tác.

---

##### 📌 LINQ & Lambda

**LINQ là gì?**  
Language Integrated Query – truy vấn dữ liệu ngay trong C#.
Dùng để lọc, sắp xếp, biến đổi danh sách.

**Ví dụ:**

```csharp
var students = new List<Student>
{
    new Student { Id = 1, Name = "An", GPA = 3.5 },
    new Student { Id = 2, Name = "Bình", GPA = 2.8 },
    new Student { Id = 3, Name = "Chi", GPA = 3.9 },
};

var result = students
    .Where(s => s.GPA >= 3.0)
    .OrderByDescending(s => s.GPA)
    .Select(s => new { s.Name, s.GPA })
    .ToList();
```

---

##### 📌 Exception Handling

**Tại sao cần xử lý ngoại lệ?**

- Ứng dụng không crash khi gặp lỗi bất ngờ
- Trả thông báo lỗi rõ ràng cho người dùng API

**Cấu trúc:**

```csharp
try
{
    var student = GetStudentById(id);
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"Không tìm thấy: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
}
finally
{
    Console.WriteLine("Hoàn thành.");
}
```

⚠️ **Lỗi thường gặp:** Bắt `Exception` quá chung → mất thông tin lỗi cụ thể.

---

##### 📌 async / await

**Tại sao cần trong Web API?**  
Web API thường gọi database, file, external API → tốn thời gian. `async/await` giúp **không chặn thread** → xử lý được nhiều request hơn.

```csharp
// ❌ KHÔNG nên (blocking)
var data = GetDataFromDb();

// ✅ NÊN (non-blocking)
var data = await GetDataFromDbAsync();

// Method async phải return Task hoặc Task<T>
public async Task<List<Student>> GetAllAsync()
{
    return await _dbContext.Students.ToListAsync();
}
```

⚠️ **Lỗi thường gặp:** Dùng `async` nhưng quên `await` → không thực sự bất đồng bộ.

---

### 🛠️ 2. Thực hành

#### 2.1. Cài đặt môi trường

| Công cụ | Link tải |
|---------|----------|
| .NET 8 SDK | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) |
| Visual Studio 2022 Community | Chọn workload: **ASP.NET and web development** |
| SQL Server 2022 Express | [miễn phí] |
| Postman | [postman.com/downloads](https://www.postman.com/downloads) |

**Kiểm tra cài đặt:**

```bash
dotnet --version
# Output mong đợi: 8.x.x
```

💡 **Mẹo:** Nếu máy yếu, có thể dùng VS Code + C# Dev Kit extension (nhẹ hơn nhiều).

---

#### 2.2. Tạo Console Application

**Demo: Console App Quản Lý Student**

**Bước 1:** Tạo project

```bash
dotnet new console -n StudentConsoleApp
cd StudentConsoleApp
```

**Bước 2:** Định nghĩa Model

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double GPA { get; set; }
}
```

**Bước 3:** Quản lý bằng LINQ

```csharp
var students = new List<Student>
{
    new Student { Id = 1, Name = "An Nguyễn", GPA = 3.5 },
    new Student { Id = 2, Name = "Bình Trần", GPA = 2.8 },
    new Student { Id = 3, Name = "Chi Lê", GPA = 3.9 },
};

var topStudents = students
    .Where(s => s.GPA >= 3.5)
    .OrderByDescending(s => s.GPA);

foreach (var s in topStudents)
    Console.WriteLine($"{s.Name} - GPA: {s.GPA}");
```

---

### 📝 2.3. Bài thực hành

**Yêu cầu:** Xây dựng Console App quản lý danh sách sinh viên.

1. Tạo `List<Student>` với ít nhất 5 sinh viên. In toàn bộ danh sách ra console.

2. Thêm thuộc tính `Major` (ngành học) vào `Student`.  
   Lọc sinh viên ngành **CNTT** có GPA ≥ 3.0. Sắp xếp theo GPA giảm dần.

3. Viết method `FindById(int id)`.  
   Nếu không tìm thấy → throw `KeyNotFoundException`.  
   Bắt exception và in thông báo lỗi thân thiện.

4. Viết class `StudentManager` với các method:
   - `AddStudent(Student s)`
   - `RemoveStudent(int id)`
   - `GetTopStudents(int n)` - trả n sinh viên GPA cao nhất

   Tất cả sử dụng LINQ.

5. Chuyển `StudentManager` sang dùng `IStudentManager` interface.  
   Viết `async Task Main()`, mô phỏng delay 1 giây khi "tải dữ liệu".  
   In kết quả dạng bảng căn lề đẹp.

---

✅ **Hoàn thành nội dung Buổi 1.**

# Buổi 2: Giới thiệu ASP.NET Core Web API, Kiến trúc ứng dụng, Middleware, Dependency Injection

---

## 📌 1. Lý thuyết

### 1.1. ASP.NET Core

#### ASP.NET Core là gì?

**ASP.NET Core** là framework xây dựng ứng dụng web hiện đại trên nền .NET, được thiết kế lại từ ASP.NET truyền thống với nhiều cải tiến vượt trội.

**Đặc điểm nổi bật:**

| Đặc điểm                 | Mô tả                                    |
| ------------------------ | ---------------------------------------- |
| **Đa nền tảng**          | Chạy trên Windows, Linux, macOS          |
| **Hiệu năng cao**        | Nhanh hơn ASP.NET truyền thống 10-20 lần |
| **Mã nguồn mở**          | Phát triển công khai trên GitHub         |
| **Tích hợp tốt**         | Hỗ trợ Docker, Kubernetes, Cloud         |
| **Module hóa**           | Chỉ sử dụng những gì cần thiết           |
| **Dependency Injection** | Hỗ trợ DI tích hợp sẵn                   |

**Kiến trúc đa tầng:**

```
┌─────────────────────────────────────────┐
│           Client (Browser/Mobile)       │
└─────────────────┬───────────────────────┘
                  │ HTTP Request/Response
┌─────────────────▼───────────────────────┐
│         ASP.NET Core Middleware         │
├─────────────────────────────────────────┤
│  Authentication │ Routing │ Endpoints   │
├─────────────────────────────────────────┤
│         Controllers / Minimal API       │
├─────────────────────────────────────────┤
│         Business Logic (Services)       │
├─────────────────────────────────────────┤
│         Data Access (Repository/EF)     │
└─────────────────────────────────────────┘
```

#### So sánh ASP.NET Core vs ASP.NET Framework

| Tiêu chí      | ASP.NET Core                | ASP.NET Framework      |
| ------------- | --------------------------- | ---------------------- |
| Nền tảng      | Windows, Linux, macOS       | Chỉ Windows            |
| Hiệu suất     | Rất cao                     | Trung bình             |
| MVC & Web API | Hợp nhất                    | Tách biệt              |
| DI            | Tích hợp sẵn                | Cần thư viện bên ngoài |
| Configuration | JSON, Environment           | XML (web.config)       |
| Deployment    | Self-contained, IIS, Docker | Chủ yếu IIS            |

---

### 1.2. Web API

#### Web API là gì?

**Web API (Application Programming Interface)** là dịch vụ HTTP trả về dữ liệu (JSON/XML), không trả giao diện HTML.

**Bản chất:** Web API là cầu nối giữa frontend và backend, cho phép các ứng dụng khác nhau giao tiếp qua HTTP protocol.

```
┌─────────────┐         ┌─────────────┐          ┌─────────────┐
│   React     │◄────────│  Web API    │─────────►│  Database   │
│   Angular   │  JSON   │  (Backend)  │   SQL    │             │
│   Vue.js    │────────►│             │◄─────────│             │
│   Mobile    │         │             │          │             │
└─────────────┘         └─────────────┘          └─────────────┘
```

**Các thành phần chính của Web API:**

| Thành phần | Vai trò |
|------------|---------|
| **Controller** | Nhận và xử lý HTTP request |
| **Action** | Phương thức xử lý request cụ thể |
| **Model** | Định nghĩa cấu trúc dữ liệu |
| **Route** | Định tuyến URL đến Action |
| **Middleware** | Xử lý request/response pipeline |

**HTTP Methods trong Web API:**

| Method | Mục đích | Trạng thái |
|--------|----------|------------|
| GET | Lấy dữ liệu | Idempotent |
| POST | Tạo mới dữ liệu | Không idempotent |
| PUT | Cập nhật toàn bộ | Idempotent |
| PATCH | Cập nhật một phần | Không idempotent |
| DELETE | Xóa dữ liệu | Idempotent |

**HTTP Status Codes thường dùng:**

| Code | Ý nghĩa | Sử dụng |
|------|---------|---------|
| 200 | OK | Thành công |
| 201 | Created | Tạo mới thành công |
| 204 | No Content | Xóa thành công |
| 400 | Bad Request | Lỗi validation |
| 401 | Unauthorized | Chưa xác thực |
| 403 | Forbidden | Không có quyền |
| 404 | Not Found | Không tìm thấy |
| 500 | Internal Server Error | Lỗi server |

💡 **Khác biệt Web API vs MVC:** Web API trả JSON, MVC trả HTML.

---

### 1.3. Project Structure

#### Tạo project với CLI

```bash
dotnet new webapi -n BookApi
cd BookApi
```

#### Cấu trúc thư mục tiêu chuẩn

```
BookApi/
├── Controllers/
│   └── WeatherForecastController.cs   # Xử lý request HTTP
├── Models/                              # Định nghĩa dữ liệu
├── Properties/
│   └── launchSettings.json            # Cấu hình launch
├── bin/                                # Build output
├── obj/                                # Build temp files
├── appsettings.json                    # Cấu hình ứng dụng
├── appsettings.Development.json       # Cấu hình Development
└── Program.cs                          # Điểm khởi động (✅ .NET 8)
```

**Phân tích các file quan trọng:**

#### Program.cs - Điểm khởi động duy nhất

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### appsettings.json - Cấu hình ứng dụng

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookDb;Trusted_Connection=True;"
  }
}
```

#### launchSettings.json - Cấu hình môi trường

```json
{
  "profiles": {
    "BookApi": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

📌 **Ghi nhớ:**
- `Controllers/` là nơi nhận và xử lý request
- `Models/` là nơi định nghĩa dữ liệu
- `Program.cs` là điểm khởi động duy nhất trong .NET 8 Minimal Hosting Model

---

### 1.4. Middleware

#### Middleware là gì?

**Middleware** là các thành phần xử lý HTTP request/response theo dạng pipeline. Mỗi middleware có thể:
1. Xử lý request
2. Gọi middleware tiếp theo
3. Dừng pipeline

#### Hoạt động của Middleware Pipeline

```
REQUEST →
┌────────────────────────────────────────────────────────────┐
│  Middleware 1 (Logging)                                    │
│    └─► Middleware 2 (Authentication)                       │
│          └─► Middleware 3 (Authorization)                  │
│                └─► Middleware 4 (Routing)                  │
│                      └─► Controller Action                 │
│                      ◄─┘                                   │
│                ◄─┘                                         │
│          ◄─┘                                               │
│    ◄─┘                                                     │
└────────────────────────────────────────────────────────────┘
← RESPONSE
```

#### Các Middleware phổ biến

| Middleware            | Nhiệm vụ                   | Thứ tự ưu tiên |
| --------------------- | -------------------------- | -------------- |
| `UseExceptionHandler` | Xử lý exception            | 1              |
| `UseHttpsRedirection` | Chuyển HTTP → HTTPS        | 2              |
| `UseStaticFiles`      | Phục vụ file tĩnh          | 3              |
| `UseRouting`          | Xác định route cho request | 4              |
| `UseAuthentication`   | Kiểm tra xác thực          | 5              |
| `UseAuthorization`    | Kiểm tra phân quyền        | 6              |
| `UseEndpoints`        | Thực thi endpoint          | 7              |

#### Ví dụ Middleware Custom

```csharp
// Middleware Logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"[LOG] Request: {context.Request.Method} {context.Request.Path}");
    await next(); // Gọi middleware tiếp theo
    Console.WriteLine($"[LOG] Response: {context.Response.StatusCode}");
});

// Hoặc tạo class riêng
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}

// Đăng ký
app.UseMiddleware<RequestLoggingMiddleware>();
```

⚠️ **Lỗi thường gặp:** Đăng ký sai thứ tự middleware. `UseAuthentication()` phải đặt TRƯỚC `UseAuthorization()`. Nếu sai → request chưa xác thực đã bị kiểm tra phân quyền → 403/401 sai.

✅ **Best Practice:** Luôn đặt Exception Handling ở đầu pipeline.

---

### 1.5. Dependency Injection

#### Dependency Injection (DI) là gì?

**Dependency Injection** là một design pattern cho phép chuyển việc tạo dependency từ bên trong class ra bên ngoài, giúp giảm coupling và tăng testability.

#### Vấn đề (Without DI)

```csharp
// ❌ Không nên: Tạo dependency trực tiếp bên trong class
public class BookController
{
    private BookService _service = new BookService(); // Phụ thuộc cứng
    // Khó test, khó thay đổi
}
```

#### Giải pháp (With DI)

```csharp
// ✅ Nên: Nhận dependency qua constructor
public class BookController : ControllerBase
{
    private readonly IBookService _service;
    
    public BookController(IBookService service)
    {
        _service = service; // DI Container tự inject
    }
}
```

#### Lợi ích của DI

| Lợi ích | Mô tả |
|---------|-------|
| **Loose Coupling** | Các thành phần không phụ thuộc trực tiếp |
| **Testability** | Dễ dàng mock dependency để unit test |
| **Maintainability** | Dễ thay đổi implementation |
| **Reusability** | Service có thể tái sử dụng |
| **Lifecycle Management** | Container quản lý lifecycle |

#### Service Lifetime

| Lifetime | Tạo instance khi | Dùng cho |
|----------|------------------|----------|
| **Singleton** | Lần đầu tiên, dùng mãi | Config, Cache, Stateless services |
| **Scoped** | Mỗi HTTP request | DbContext, Repository, Business Services |
| **Transient** | Mỗi lần inject | Stateless service, Utility classes |

```csharp
// Đăng ký trong Program.cs
builder.Services.AddSingleton<IMyService, MyService>();   // 1 instance cho toàn app
builder.Services.AddScoped<IMyService, MyService>();      // 1 instance mỗi request
builder.Services.AddTransient<IMyService, MyService>();   // 1 instance mỗi lần request

// AddDbContext mặc định là Scoped
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

#### Ví dụ thực tế về Lifetime

```csharp
public class TestService : ITestService
{
    public Guid Id { get; } = Guid.NewGuid();
}

// Đăng ký
builder.Services.AddTransient<ITestService, TestService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddSingleton<ITestService, TestService>();

// Trong Controller
public class TestController : ControllerBase
{
    private readonly ITestService _service1;
    private readonly ITestService _service2;
    
    public TestController(ITestService service1, ITestService service2)
    {
        _service1 = service1;
        _service2 = service2;
        // Transient: service1.Id ≠ service2.Id
        // Scoped: service1.Id == service2.Id (trong cùng request)
        // Singleton: service1.Id == service2.Id (toàn app)
    }
}
```

⚠️ **Lỗi thường gặp:** Inject `Scoped` vào `Singleton` → lỗi runtime.

✅ **Best Practice:** `DbContext` luôn dùng `AddDbContext()` (tương đương Scoped).

---

### 1.6. Program.cs

#### .NET 8 Minimal Hosting Model

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký services (DI Container)
builder.Services.AddControllers();                    // API Controllers
builder.Services.AddEndpointsApiExplorer();          // API Explorer cho Swagger
builder.Services.AddSwaggerGen();                    // Swagger Generator
builder.Services.AddScoped<IBookService, BookService>(); // Custom Service
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Cấu hình Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();     // Chuyển HTTP → HTTPS
app.UseAuthentication();       // Xác thực
app.UseAuthorization();        // Phân quyền
app.MapControllers();          // Map API Endpoints

app.Run();                     // Khởi chạy
```

#### Phân tích các phương thức

| Phương thức | Vai trò |
|-------------|---------|
| `WebApplication.CreateBuilder()` | Tạo builder để cấu hình |
| `builder.Services.*` | Đăng ký services vào DI Container |
| `builder.Build()` | Build ứng dụng |
| `app.Use*()` | Thêm middleware vào pipeline |
| `app.MapControllers()` | Map các controller |
| `app.Run()` | Chạy ứng dụng |

📌 **Ghi nhớ:** 
- `builder` để đăng ký services
- `app` để cấu hình middleware
- `Run()` để khởi chạy

---

## 🛠️ 2. Thực hành

### 2.1. Tạo Web API

#### Bước 1: Tạo Project

```bash
# Dùng CLI
dotnet new webapi -n BookApi
cd BookApi
```

**Hoặc dùng Visual Studio:** New Project → ASP.NET Core Web API

#### Bước 2: Chạy ứng dụng

```bash
dotnet run
# App lắng nghe ở https://localhost:5001
```

#### Bước 3: Mở Swagger

```
https://localhost:5001/swagger
```

#### Bước 4: Thêm BookController

Tạo file `Controllers/BookController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var books = new List<string> { "Clean Code", "DDD", "C# in Depth" };
        return Ok(books);
    }
}
```

#### Bước 5: Test trên Swagger UI

1. Click "Try it out"
2. Click "Execute"
3. Xem response JSON

💡 **Mẹo:** 
- Đừng tắt app khi test
- Chạy `dotnet run` trong terminal
- Mở Swagger ở browser khác
- Hot reload: `dotnet watch run`

---

### 2.2. Chạy Swagger

#### Swagger – Tài Liệu API Tự Động

**Swagger là gì?**
- Công cụ sinh tài liệu API từ code (OpenAPI spec)
- Giao diện web để test API ngay trên browser
- Đã được tích hợp sẵn trong template Web API (.NET 8)

**Truy cập Swagger:**
```
https://localhost:{port}/swagger
```

**Tùy chỉnh Swagger:**

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Book API",
        Version = "v1",
        Description = "API quản lý sách",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@bookapi.com"
        }
    });
    
    // Thêm security definition nếu cần
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});
```

✅ **Best Practice:** Luôn bật Swagger trong môi trường Development để test nhanh. Tắt hoặc bảo vệ trong Production.

---

### 2.3. Viết Controller đầu tiên

#### Tạo Controllers/BookController.cs

```csharp
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var books = new List<string> { "Clean Code", "DDD", "C# in Depth" };
        return Ok(books);
    }
}
```

#### Giải thích các thành phần

| Thành phần | Ý nghĩa |
|------------|---------|
| `[ApiController]` | Tự động validate model, trả lỗi 400 |
| `[Route("api/[controller]")]` | Route: `/api/book` (lấy tên controller) |
| `ControllerBase` | Base class cho API (không có View) |
| `Ok(books)` | Trả 200 OK kèm dữ liệu |
| `[HttpGet]` | Xác định phương thức HTTP |

#### Các ActionResult thường dùng

```csharp
// Trả về 200 OK
return Ok(data);

// Trả về 201 Created
return CreatedAtAction(nameof(GetById), new { id = newId }, newData);

// Trả về 204 No Content
return NoContent();

// Trả về 400 Bad Request
return BadRequest("Invalid data");

// Trả về 404 Not Found
return NotFound("Id not found");

// Trả về 401 Unauthorized
return Unauthorized();

// Trả về 403 Forbidden
return Forbid();
```

---

### 2.4. Thêm Service với DI

#### Bước 1: Tạo Interface và Service

```csharp
// Interfaces/IBookService.cs
public interface IBookService
{
    List<string> GetAll();
}

// Services/BookService.cs
public class BookService : IBookService
{
    public List<string> GetAll()
    {
        return new List<string> { "Clean Code", "DDD", "C# in Depth" };
    }
}
```

#### Bước 2: Đăng ký trong Program.cs

```csharp
builder.Services.AddScoped<IBookService, BookService>();
```

#### Bước 3: Inject vào Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_bookService.GetAll());
    }
}
```

---

### 2.5. Bài thực hành

#### Bài tập 1: Tạo Product API

**Yêu cầu:** ASP.NET Core Web API tên `ProductApi`. Chạy thành công và mở Swagger UI trên browser.

**Hướng dẫn:**

```bash
dotnet new webapi -n ProductApi
cd ProductApi
dotnet run
# Mở https://localhost:5001/swagger
```

---

#### Bài tập 2: Endpoint GET /api/product

**Yêu cầu:** Endpoint GET `/api/product`. Trả về `List<string>` 3 tên sản phẩm. Kiểm tra bằng Swagger.

**Đáp án:**

```csharp
// Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = new List<string> 
        { 
            "Laptop Dell XPS", 
            "iPhone 15 Pro", 
            "Samsung Galaxy S24" 
        };
        return Ok(products);
    }
}
```

---

#### Bài tập 3: Tạo Service và Inject

**Yêu cầu:** Interface với method `GetAll()`. `ProductService` implement interface. Đăng ký `AddScoped`, inject vào `ProductController`.

**Đáp án:**

```csharp
// Interfaces/IProductService.cs
public interface IProductService
{
    List<string> GetAll();
}

// Services/ProductService.cs
public class ProductService : IProductService
{
    public List<string> GetAll()
    {
        return new List<string> 
        { 
            "Laptop Dell XPS", 
            "iPhone 15 Pro", 
            "Samsung Galaxy S24" 
        };
    }
}

// Program.cs
builder.Services.AddScoped<IProductService, ProductService>();

// Controllers/ProductController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_productService.GetAll());
    }
}
```

---

#### Bài tập 4: Custom Middleware Logging

**Yêu cầu:** In console: `[LOG] Request: {Method} {Path}` cho mỗi request. Đăng ký bằng `app.Use()` trong Program.cs.

**Đáp án:**

```csharp
// Program.cs
var app = builder.Build();

// Custom Middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[LOG] Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

// Hoặc tạo class riêng
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"[LOG] Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
    }
}

// Đăng ký
app.UseMiddleware<RequestLoggingMiddleware>();
```

---

#### Bài tập 5: Kiểm tra Service Lifetime

**Yêu cầu:** Tạo 3 service (Singleton/Scoped/Transient). Mỗi service có property `Id = Guid.NewGuid()`. Gọi 2 lần trong cùng request, in Id → quan sát khác nhau.

**Đáp án:**

```csharp
// Interfaces/ITestService.cs
public interface ITestService
{
    Guid Id { get; }
}

// Services/SingletonService.cs
public class SingletonService : ITestService
{
    public Guid Id { get; } = Guid.NewGuid();
}

// Services/ScopedService.cs
public class ScopedService : ITestService
{
    public Guid Id { get; } = Guid.NewGuid();
}

// Services/TransientService.cs
public class TransientService : ITestService
{
    public Guid Id { get; } = Guid.NewGuid();
}

// Program.cs
builder.Services.AddSingleton<ITestService, SingletonService>();
builder.Services.AddScoped<ITestService, ScopedService>();
builder.Services.AddTransient<ITestService, TransientService>();

// Controllers/TestController.cs
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ITestService _singleton1;
    private readonly ITestService _singleton2;
    private readonly ITestService _scoped1;
    private readonly ITestService _scoped2;
    private readonly ITestService _transient1;
    private readonly ITestService _transient2;

    public TestController(
        [FromServices] ITestService singleton1,
        [FromServices] ITestService singleton2,
        [FromServices] ITestService scoped1,
        [FromServices] ITestService scoped2,
        [FromServices] ITestService transient1,
        [FromServices] ITestService transient2)
    {
        _singleton1 = singleton1;
        _singleton2 = singleton2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _transient1 = transient1;
        _transient2 = transient2;
    }

    [HttpGet("lifetime")]
    public IActionResult TestLifetime()
    {
        var result = new
        {
            Singleton = new { Instance1 = _singleton1.Id, Instance2 = _singleton2.Id, Equal = _singleton1.Id == _singleton2.Id },
            Scoped = new { Instance1 = _scoped1.Id, Instance2 = _scoped2.Id, Equal = _scoped1.Id == _scoped2.Id },
            Transient = new { Instance1 = _transient1.Id, Instance2 = _transient2.Id, Equal = _transient1.Id == _transient2.Id }
        };
        
        return Ok(result);
    }
}
```

**Kết quả dự kiến:**
- Singleton: `Equal = true` (cùng instance)
- Scoped: `Equal = true` (cùng request)
- Transient: `Equal = false` (khác instance)

---

## 📝 Bài tập nâng cao

### Bài tập 6: CRUD API cho Book

**Yêu cầu:** Xây dựng CRUD API cho Book với các endpoint:

```
GET    /api/book         - Lấy tất cả sách
GET    /api/book/{id}    - Lấy sách theo id
POST   /api/book         - Tạo sách mới
PUT    /api/book/{id}    - Cập nhật sách
DELETE /api/book/{id}    - Xóa sách
```

**Đáp án:**

```csharp
// Models/Book.cs
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
}

// Services/IBookService.cs
public interface IBookService
{
    List<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    Book Update(Book book);
    bool Delete(int id);
}

// Services/BookService.cs
public class BookService : IBookService
{
    private static List<Book> _books = new List<Book>
    {
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Year = 2008, Price = 45.99m },
        new Book { Id = 2, Title = "DDD", Author = "Eric Evans", Year = 2003, Price = 55.50m },
        new Book { Id = 3, Title = "C# in Depth", Author = "Jon Skeet", Year = 2019, Price = 49.99m }
    };
    private static int _nextId = 4;

    public List<Book> GetAll() => _books;

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(book);
        return book;
    }

    public Book Update(Book book)
    {
        var existing = _books.FirstOrDefault(b => b.Id == book.Id);
        if (existing == null) throw new KeyNotFoundException($"Book with ID {book.Id} not found");
        
        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Year = book.Year;
        existing.Price = book.Price;
        
        return existing;
    }

    public bool Delete(int id)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        if (book == null) return false;
        return _books.Remove(book);
    }
}

// Controllers/BookController.cs
[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_bookService.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var book = _bookService.GetById(id);
        if (book == null)
            return NotFound($"Book with ID {id} not found");
        return Ok(book);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Book book)
    {
        var newBook = _bookService.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Book book)
    {
        if (id != book.Id)
            return BadRequest("ID mismatch");
        
        try
        {
            var updated = _bookService.Update(book);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _bookService.Delete(id);
        if (!deleted)
            return NotFound($"Book with ID {id} not found");
        return NoContent();
    }
}

// Program.cs
builder.Services.AddScoped<IBookService, BookService>();
```

---

### Bài tập 7: Middleware xử lý Exception

**Yêu cầu:** Tạo middleware toàn cục để bắt và xử lý exception, trả về response dạng JSON.

**Đáp án:**

```csharp
// Middlewares/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## 📊 Tổng kết Buổi 2

| Chủ đề | Đã học |
|--------|--------|
| ASP.NET Core | Framework đa nền tảng, hiệu năng cao |
| Web API | HTTP Service trả JSON |
| Project Structure | Controllers, Models, Program.cs |
| Middleware | Pipeline xử lý request/response |
| Dependency Injection | Singleton, Scoped, Transient |
| Program.cs | Minimal Hosting Model |

**Key Takeaways:**
1. ✅ ASP.NET Core là framework web hiện đại, đa nền tảng
2. ✅ Web API trả về JSON, không trả HTML
3. ✅ Middleware chạy theo pipeline, thứ tự quan trọng
4. ✅ DI giúp loose coupling và dễ test
5. ✅ .NET 8 dùng Minimal Hosting Model (Program.cs duy nhất)

---

✅ **Hoàn thành nội dung Buổi 2.**


# Buổi 3: Controller, Routing, HTTP Methods, RESTful API, Model Binding, Action Result

---

## 📌 1. Lý thuyết

### 1.1. Controller

#### Controller Trong ASP.NET Core

**Controller** là lớp tiếp nhận và xử lý các HTTP request đến. Nó nhóm các endpoint (action) liên quan đến cùng một resource.

**Phân biệt ControllerBase và Controller:**

| Đặc điểm | `ControllerBase` | `Controller` (MVC) |
|----------|------------------|-------------------|
| **Dùng cho** | Web API | MVC (có View) |
| **Trả về** | Dữ liệu (JSON/XML) | View + Dữ liệu |
| **Hỗ trợ View** | ✕ Không | ✓ Có |
| **Tính năng** | Tối giản cho API | Đầy đủ cho MVC |

**Ví dụ:**

```csharp
// ✅ Đúng cho Web API
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new List<string> { "An", "Bình", "Chi" });
    }
}

// ❌ Không dùng Controller cho Web API (vì kế thừa thêm View features)
public class StudentsController : Controller
{
    // Không cần View trong API
}
```

#### [ApiController] Attribute

`[ApiController]` bật các hành vi chuẩn cho Web API:

| Tính năng | Mô tả |
|-----------|-------|
| **Tự động validate Model** | Tự động trả `400 Bad Request` khi `ModelState` không hợp lệ |
| **Tự suy luận binding source** | Tự động xác định `[FromBody]`, `[FromRoute]`... |
| **Attribute Routing bắt buộc** | Yêu cầu dùng Attribute Routing thay vì Conventional |
| **Problem Details** | Trả về lỗi theo chuẩn RFC 7807 |

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    // Khi có [ApiController], không cần kiểm tra ModelState.IsValid
    {
        // Tự động trả về 400 nếu ModelState không hợp lệ
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```

📌 **Ghi nhớ:** Web API dùng `ControllerBase`, không cần View. Luôn gắn `[ApiController]`.

---

### 1.2. Routing

#### Routing Trong ASP.NET Core

Routing là cơ chế ánh xạ URL đến các action method trong controller.

#### 2 loại Routing:

**1. Conventional Routing (ít dùng trong API):**

```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");
```

Ví dụ: `/Students/GetAll` → `StudentsController.GetAll()`

**2. Attribute Routing (chuẩn cho Web API):**

```csharp
[ApiController]
[Route("api/[controller]")] // → /api/students
public class StudentsController : ControllerBase
{
    [HttpGet]                    // → GET /api/students
    public IActionResult GetAll() { ... }

    [HttpGet("{id}")]            // → GET /api/students/{id}
    public IActionResult GetById(int id) { ... }

    [HttpPost]                   // → POST /api/students
    public IActionResult Create([FromBody] Student student) { ... }

    [HttpPut("{id}")]            // → PUT /api/students/{id}
    public IActionResult Update(int id, [FromBody] Student student) { ... }

    [HttpDelete("{id}")]         // → DELETE /api/students/{id}
    public IActionResult Delete(int id) { ... }
}
```

#### Route Tokens

| Token | Mô tả | Ví dụ |
|-------|-------|-------|
| `[controller]` | Tên controller (bỏ "Controller") | `StudentsController` → `students` |
| `[action]` | Tên action method | `GetAll` → `GetAll` |

#### Route Constraints

```csharp
[HttpGet("{id:int}")]        // id phải là int
[HttpGet("{id:min(1)}")]     // id >= 1
[HttpGet("{name:alpha}")]    // name chỉ chứa chữ cái
[HttpGet("{id:range(1,100)}")] // id trong khoảng 1-100
[HttpGet("{id:regex(\\d+)}")]   // id chỉ chứa số
```

#### Route với Optional Parameters

```csharp
[HttpGet("{id:int?}")]
public IActionResult Get(int? id)
{
    if (id.HasValue)
        return Ok($"Lấy student với id = {id}");
    return Ok("Lấy tất cả students");
}
```

✅ **Best Practice:** Luôn dùng Attribute Routing trong Web API. `[controller]` sẽ tự thay bằng tên controller (bỏ hậu tố "Controller").

---

### 1.3. HTTP Methods

#### HTTP Methods & Ý Nghĩa

| Method | Mục đích | Idempotent? | Safe? |
|--------|----------|-------------|-------|
| **GET** | Lấy dữ liệu | ✓ Yes | ✓ Yes |
| **POST** | Tạo mới resource | ✕ No | ✕ No |
| **PUT** | Cập nhật toàn bộ resource | ✓ Yes | ✕ No |
| **PATCH** | Cập nhật một phần resource | ✕ No | ✕ No |
| **DELETE** | Xóa resource | ✓ Yes | ✕ No |
| **HEAD** | Lấy headers (không có body) | ✓ Yes | ✓ Yes |
| **OPTIONS** | Lấy thông tin về methods hỗ trợ | ✓ Yes | ✓ Yes |

**Idempotent:** Gọi nhiều lần cho kết quả (trạng thái server) giống như gọi một lần.

**Safe:** Không thay đổi trạng thái server (chỉ đọc).

#### Bảng ánh xạ CRUD sang HTTP Methods

| Operation | HTTP Method | URI | Status Code |
|-----------|-------------|-----|-------------|
| **C**reate | POST | `/api/students` | 201 Created |
| **R**ead All | GET | `/api/students` | 200 OK |
| **R**ead One | GET | `/api/students/{id}` | 200 OK / 404 |
| **U**pdate | PUT/PATCH | `/api/students/{id}` | 200/204 / 404 |
| **D**elete | DELETE | `/api/students/{id}` | 204 / 404 |

**Ví dụ với Student API:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    // GET /api/students - Lấy tất cả sinh viên
    [HttpGet]
    public IActionResult GetAll() => Ok(_students);

    // GET /api/students/{id} - Lấy một sinh viên
    [HttpGet("{id}")]
    public IActionResult GetById(int id) { ... }

    // POST /api/students - Tạo sinh viên mới
    [HttpPost]
    public IActionResult Create([FromBody] Student student) { ... }

    // PUT /api/students/{id} - Cập nhật toàn bộ
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Student student) { ... }

    // DELETE /api/students/{id} - Xóa sinh viên
    [HttpDelete("{id}")]
    public IActionResult Delete(int id) { ... }
}
```

---

### 1.4. RESTful API

#### RESTful API là gì?

**REST** (Representational State Transfer) là một kiến trúc thiết kế API theo chuẩn HTTP. Mỗi resource (tài nguyên) được định danh bằng URI, và thao tác với resource qua HTTP Methods.

**Bản chất:** REST là tập hợp các nguyên tắc và quy ước, không phải giao thức hay framework.

#### 6 nguyên tắc REST

| Nguyên tắc | Ý nghĩa | Ví dụ |
|------------|---------|-------|
| **Stateless** | Server không lưu trạng thái client | Mỗi request chứa đủ thông tin |
| **Client-Server** | Tách biệt giao diện và dữ liệu | Frontend riêng, Backend riêng |
| **Uniform Interface** | Giao diện nhất quán qua URI + Method | `/api/students/{id}` + GET/PUT/DELETE |
| **Cacheable** | Response có thể được cache | Cache dữ liệu ít thay đổi |
| **Layered System** | Client không cần biết server gì | Proxy, Load Balancer |
| **Code on Demand** | (Tùy chọn) Server gửi code về client | JavaScript |

#### RESTful URI Design (Best Practices)

```csharp
// ✅ Đúng
GET    /api/students
GET    /api/students/123
POST   /api/students
PUT    /api/students/123
DELETE /api/students/123

// ❌ Sai
GET    /api/getStudents
GET    /api/studentById?id=123
POST   /api/createStudent
GET    /api/studentDetails/123
```

**Quy tắc đặt tên URI:**

| Quy tắc | Đúng | Sai |
|---------|------|-----|
| **Dùng danh từ số nhiều** | `/products`, `/orders` | `/product`, `/getOrder` |
| **Dùng lowercase** | `/users/123` | `/Users/123` |
| **Kebab-case** | `/order-items` | `/orderItems` |
| **Không dùng verb** | `/products` | `/getProducts` |
| **Nested resources** | `/orders/123/items` | `/orderItems?orderId=123` |

#### Nested Resources

```csharp
// Lấy tất cả items của order 123
GET /api/orders/123/items

// Lấy item 456 của order 123
GET /api/orders/123/items/456

// Tạo item cho order 123
POST /api/orders/123/items

// Cập nhật item 456 của order 123
PUT /api/orders/123/items/456

// Xóa item 456 của order 123
DELETE /api/orders/123/items/456
```

#### Model Binding – Nhận Dữ Liệu Từ Request

Model Binding là cách ASP.NET Core ánh xạ dữ liệu từ request đến các tham số của action method.

| Attribute | Nguồn | Ví dụ Request |
|-----------|-------|---------------|
| `[FromRoute]` | URL path | `/api/students/5` → `id = 5` |
| `[FromQuery]` | Query string | `?page=1&size=10` → `page=1, size=10` |
| `[FromBody]` | Request body (JSON) | `{ "name": "An" }` |
| `[FromHeader]` | HTTP header | `Authorization: Bearer token` |
| `[FromForm]` | Form data | `name=An&age=20` |
| `[FromServices]` | DI Container | Lấy service từ DI |

**Ví dụ thực tế:**

```csharp
// GET /api/students/5?includeGrades=true&page=1
[HttpGet("{id}")]
public IActionResult Get(
    [FromRoute] int id,
    [FromQuery] bool includeGrades = false,
    [FromQuery] int page = 1)
{
    // id = 5, includeGrades = true, page = 1
    return Ok();
}

// POST /api/students
[HttpPost]
public IActionResult Create([FromBody] CreateStudentDto dto)
{
    // DTO từ JSON body
    return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
}

// Lấy service từ DI
[HttpGet]
public IActionResult GetAll([FromServices] IStudentService service)
{
    return Ok(service.GetAll());
}
```

📌 **Ghi nhớ:** Với `[ApiController]`, ASP.NET Core tự suy luận nguồn binding:
- `int id` → `[FromRoute]`
- `[FromBody]` cho complex types
- `[FromQuery]` cho các tham số còn lại

---

### 1.5. Action Result & HTTP Status Codes

#### IActionResult – Kiểu trả về của Action

`IActionResult` là interface cho mọi loại response. Các helper methods tạo ra các response tương ứng.

#### Các ActionResult phổ biến

| Method                 | Status Code               | Khi nào dùng                 |
| ---------------------- | ------------------------- | ---------------------------- |
| `Ok(data)`             | 200 OK                    | Thành công, có dữ liệu       |
| `CreatedAtAction(...)` | 201 Created               | Tạo mới thành công           |
| `Created(...)`         | 201 Created               | Tạo mới (đơn giản)           |
| `NoContent()`          | 204 No Content            | Thành công, không có dữ liệu |
| `BadRequest(error)`    | 400 Bad Request           | Dữ liệu đầu vào sai          |
| `Unauthorized()`       | 401 Unauthorized          | Chưa xác thực                |
| `Forbid()`             | 403 Forbidden             | Không có quyền               |
| `NotFound(error)`      | 404 Not Found             | Không tìm thấy resource      |
| `Conflict(error)`      | 409 Conflict              | Xung đột dữ liệu             |
| `StatusCode(500)`      | 500 Internal Server Error | Lỗi server                   |

#### Ví dụ chi tiết

```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var students = _service.GetAll();
        return Ok(students); // 200 OK
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var student = _service.GetById(id);
        if (student == null)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" }); // 404
        
        return Ok(student); // 200
    }

    [HttpPost]
    public IActionResult Create([FromBody] Student student)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // 400
            
        var created = _service.Add(student);
        return CreatedAtAction(
            nameof(GetById), 
            new { id = created.Id }, 
            created); // 201 Created
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Student student)
    {
        if (id != student.Id)
            return BadRequest(new { message = "ID không khớp" }); // 400
            
        try
        {
            _service.Update(student);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" }); // 404
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" }); // 404
            
        return NoContent(); // 204
    }
}
```

#### Problem Details (RFC 7807)

```csharp
// Tự động trả về với [ApiController]
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Bad Request",
    "status": 400,
    "traceId": "00-0f8c2b9f7e8a4d6b9c7e8a4d6b9c7e8a-..."
}

// Custom Problem Details
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var student = _service.GetById(id);
    if (student == null)
    {
        return Problem(
            detail: $"Không tìm thấy sinh viên với ID {id}",
            statusCode: 404,
            title: "Resource Not Found"
        );
    }
    return Ok(student);
}
```

⚠️ **Lỗi thường gặp:** Trả `Ok(null)` khi không tìm thấy thay vì `NotFound()`.

Ghi chú: Có thể thêm DELETE, FILTER

VD UsersController:
```C#
namespace ProjectAPIdemo.Controllers
{
    // [ApiController]: Bật tính năng Model Binding tự động và validation dữ liệu đầu vào
    [ApiController]

    // ROUTING: Định tuyến URL cấp Controller -> "api/users"
    [Route("api/[controller]")]

    // CONTROLLER: Lớp kế thừa từ ControllerBase để xử lý các request
    public class UsersController : ControllerBase
    {
        // DỮ LIỆU GIẢ (Mock Data): Dùng static list để lưu tạm dữ liệu trong bộ nhớ
        private static List<UserResponseDto> _fakeUsers = new List<UserResponseDto>
        {
            new UserResponseDto { Id = 1, Username = "nguyenvana", Email = "ana@gmail.com" },
            new UserResponseDto { Id = 2, Username = "tranvanb", Email = "banb@gmail.com" }
        };

        // 1. HTTP METHOD: [HttpGet]
        // ROUTING: GET api/users
        // ACTION RESULT: ActionResult<List<User>>
        [HttpGet]
        public ActionResult<List<UserResponseDto>> GetAll()
        {
            // Trả về HTTP Status Code 200 OK kèm danh sách user
            return Ok(_fakeUsers);
        }

        // 2. HTTP METHOD: [HttpGet("{id}")]
        // ROUTING: GET api/users/1
        // MODEL BINDING: Lấy dữ liệu id từ URL (Route) truyền vào tham số `int id`
        [HttpGet("{id:int}")]
        public ActionResult<UserResponseDto> GetById(int id)
        {
            var user = _fakeUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                // ACTION RESULT: HTTP 404 Not Found
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            // ACTION RESULT: HTTP 200 OK kèm đối tượng user tìm thấy
            return Ok(user);
        }

        // 3. RESTFUL API & HTTP METHOD: [HttpPost]
        // ROUTING: POST api/users/register
        // MODEL BINDING: Attributes [FromBody] đọc dữ liệu JSON gửi lên ép thành đối tượng `User`
        [HttpPost("register")]
        public ActionResult<UserResponseDto> Register([FromBody] UserResponseDto request)
        {
            // Giả lập logic tạo ID tự tăng
            request.Id = _fakeUsers.Max(u => u.Id) + 1;
            _fakeUsers.Add(request);

            // RESTful Standard: Trả về HTTP 201 Created cùng đường dẫn lấy tài nguyên vừa tạo
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        // 4. RESTFUL API & HTTP METHOD: [HttpPost]
        // ROUTING: POST api/users/login
        [HttpPost("login")]
        public ActionResult<string> Login([FromBody] UserLoginDto request)
        {
            // Logic giả lập kiểm tra tài khoản
            if (request.Username == "admin" && request.Password == "123456")
            {
                // ACTION RESULT: HTTP 200 OK kèm chuỗi Token giả
```

```C#
return Ok(new { token = "fake-jwt-token-xyz-123" });
            }

            // ACTION RESULT: HTTP 400 Bad Request
            return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }
    }
   
}
```

---

## 🛠️ 2. Thực hành

### 2.1. Xây dựng CRUD với dữ liệu giả

#### Step 1: Tạo Project

```bash
dotnet new webapi -n StudentApi
cd StudentApi
```

#### Step 2: Định nghĩa Model

```csharp
// Models/Student.cs
namespace StudentApi.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
}
```

#### Step 3: Tạo DTO (Data Transfer Object)

```csharp
// DTOs/StudentDto.cs
namespace StudentApi.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class CreateStudentDto
{
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentDto
{
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; }
    public DateTime DateOfBirth { get; set; }
}
```

#### Step 4: Tạo Service

```csharp
// Services/IStudentService.cs
namespace StudentApi.Services;

public interface IStudentService
{
    List<Student> GetAll();
    Student? GetById(int id);
    Student Add(CreateStudentDto dto);
    Student Update(int id, UpdateStudentDto dto);
    bool Delete(int id);
}

// Services/StudentService.cs
using StudentApi.Models;
using StudentApi.DTOs;

namespace StudentApi.Services;

public class StudentService : IStudentService
{
    private static List<Student> _students = new()
    {
        new Student { Id = 1, Name = "An Nguyễn", Major = "CNTT", GPA = 3.5, DateOfBirth = new DateTime(2002, 5, 15) },
        new Student { Id = 2, Name = "Bình Trần", Major = "CNTT", GPA = 2.8, DateOfBirth = new DateTime(2001, 8, 20) },
        new Student { Id = 3, Name = "Chi Lê", Major = "HTTT", GPA = 3.9, DateOfBirth = new DateTime(2003, 3, 10) },
        new Student { Id = 4, Name = "Dũng Phạm", Major = "KHDL", GPA = 3.2, DateOfBirth = new DateTime(2002, 11, 5) },
        new Student { Id = 5, Name = "Em Ngô", Major = "CNTT", GPA = 3.7, DateOfBirth = new DateTime(2001, 7, 25) }
    };
    private static int _nextId = 6;

    public List<Student> GetAll() => _students;

    public Student? GetById(int id) => _students.FirstOrDefault(s => s.Id == id);

    public Student Add(CreateStudentDto dto)
    {
        var student = new Student
        {
            Id = _nextId++,
            Name = dto.Name,
            Major = dto.Major,
            GPA = dto.GPA,
            DateOfBirth = dto.DateOfBirth
        };
        _students.Add(student);
        return student;
    }

    public Student Update(int id, UpdateStudentDto dto)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null)
            throw new KeyNotFoundException($"Không tìm thấy sinh viên với ID {id}");

        student.Name = dto.Name;
        student.Major = dto.Major;
        student.GPA = dto.GPA;
        student.DateOfBirth = dto.DateOfBirth;
        return student;
    }

    public bool Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null) return false;
        return _students.Remove(student);
    }
}
```

#### Step 5: Tạo Controller

```csharp
// Controllers/StudentsController.cs
using Microsoft.AspNetCore.Mvc;
using StudentApi.Models;
using StudentApi.DTOs;
using StudentApi.Services;

namespace StudentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    // GET /api/students
    [HttpGet]
    public IActionResult GetAll()
    {
        var students = _service.GetAll();
        var result = students.Select(s => new StudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Major = s.Major,
            GPA = s.GPA,
            DateOfBirth = s.DateOfBirth
        });
        return Ok(result);
    }

    // GET /api/students/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var student = _service.GetById(id);
        if (student == null)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });

        var result = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Major = student.Major,
            GPA = student.GPA,
            DateOfBirth = student.DateOfBirth
        };
        return Ok(result);
    }

    // GET /api/students/search?major=CNTT&minGPA=3.0
    [HttpGet("search")]
    public IActionResult Search(
        [FromQuery] string? major,
        [FromQuery] double? minGPA)
    {
        var query = _service.GetAll().AsQueryable();

        if (!string.IsNullOrEmpty(major))
            query = query.Where(s => s.Major.Equals(major, StringComparison.OrdinalIgnoreCase));

        if (minGPA.HasValue)
            query = query.Where(s => s.GPA >= minGPA.Value);

        var result = query.Select(s => new StudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Major = s.Major,
            GPA = s.GPA,
            DateOfBirth = s.DateOfBirth
        });

        return Ok(result);
    }

    // POST /api/students
    [HttpPost]
    public IActionResult Create([FromBody] CreateStudentDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        try
        {
            var student = _service.Add(dto);
            var result = new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Major = student.Major,
                GPA = student.GPA,
                DateOfBirth = student.DateOfBirth
            };
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo sinh viên", error = ex.Message });
        }
    }

    // PUT /api/students/{id}
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateStudentDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        try
        {
            var student = _service.Update(id, dto);
            var result = new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Major = student.Major,
                GPA = student.GPA,
                DateOfBirth = student.DateOfBirth
            };
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật sinh viên", error = ex.Message });
        }
    }

    // DELETE /api/students/{id}
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });

        return NoContent();
    }
}
```

#### Step 6: Đăng ký Service trong Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### Step 7: Test với Swagger

```
https://localhost:5001/swagger
```

**Test các endpoints:**

| Operation | Method | URI | Body |
|-----------|--------|-----|------|
| GetAll | GET | `/api/students` | - |
| GetById | GET | `/api/students/1` | - |
| Search | GET | `/api/students/search?major=CNTT&minGPA=3.0` | - |
| Create | POST | `/api/students` | JSON body |
| Update | PUT | `/api/students/1` | JSON body |
| Delete | DELETE | `/api/students/1` | - |

#### Step 8: Test với Postman

**Create Collection "StudentApi" và thêm requests:**

```
POST /api/students
Content-Type: application/json
{
    "name": "Hoa Vũ",
    "major": "CNTT",
    "gpa": 3.8,
    "dateOfBirth": "2003-04-12"
}

GET /api/students
GET /api/students/6
GET /api/students/search?major=CNTT&minGPA=3.5
PUT /api/students/6
{
    "name": "Hoa Vũ",
    "major": "KHDL",
    "gpa": 3.9,
    "dateOfBirth": "2003-04-12"
}
DELETE /api/students/6
```

---

### 2.2. Bài thực hành

#### Bài tập 1: Products API cơ bản

**Yêu cầu:** Tạo `ProductsController` với 2 endpoint:
- `GET /api/products` → trả danh sách 3 sản phẩm
- `GET /api/products/{id}` → trả 1 sản phẩm hoặc 404

**Đáp án:**

```csharp
// Models/Product.cs
namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop Dell XPS", Category = "Electronics", Price = 1500, Stock = 10 },
        new Product { Id = 2, Name = "iPhone 15 Pro", Category = "Electronics", Price = 1200, Stock = 5 },
        new Product { Id = 3, Name = "Samsung Galaxy S24", Category = "Electronics", Price = 1000, Stock = 8 }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Không tìm thấy sản phẩm với ID {id}" });
        return Ok(product);
    }
}
```

**Giải thích:**
- `[HttpGet]` không có tham số → mặc định route `/api/products`
- `[HttpGet("{id:int}")]` → route `/api/products/{id}` với ràng buộc id là int
- `NotFound()` trả về 404 khi không tìm thấy
- `Ok()` trả về 200 với dữ liệu

---

#### Bài tập 2: POST endpoint

**Yêu cầu:** Thêm endpoint `POST /api/products` để tạo sản phẩm mới. Trả về `201 Created` kèm dữ liệu sản phẩm vừa tạo. Test bằng Swagger và Postman.

**Đáp án:**

```csharp
[HttpPost]
public IActionResult Create([FromBody] Product product)
{
    if (product == null)
        return BadRequest(new { message = "Dữ liệu không hợp lệ" });

    // Validate
    if (string.IsNullOrEmpty(product.Name))
        return BadRequest(new { message = "Tên sản phẩm là bắt buộc" });

    if (product.Price <= 0)
        return BadRequest(new { message = "Giá sản phẩm phải lớn hơn 0" });

    // Gán ID mới
    product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
    _products.Add(product);

    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}
```

**Giải thích:**
- `[FromBody]` lấy dữ liệu từ JSON body
- `CreatedAtAction` trả về 201 Created với Location header
- `nameof(GetById)` xác định action để tạo Location header

**Test với Postman:**

```
POST /api/products
Content-Type: application/json
{
    "name": "MacBook Pro 14",
    "category": "Electronics",
    "price": 2500,
    "stock": 3
}
```

---

#### Bài tập 3: PUT và DELETE

**Yêu cầu:** Thêm `PUT /api/products/{id}` và `DELETE /api/products/{id}`. Đảm bảo trả đúng status code: 204, 404. Xử lý trường hợp `id` không tồn tại.

**Đáp án:**

```csharp
// PUT /api/products/{id}
[HttpPut("{id:int}")]
public IActionResult Update(int id, [FromBody] Product updated)
{
    if (updated == null)
        return BadRequest(new { message = "Dữ liệu không hợp lệ" });

    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
        return NotFound(new { message = $"Không tìm thấy sản phẩm với ID {id}" });

    // Validate
    if (string.IsNullOrEmpty(updated.Name))
        return BadRequest(new { message = "Tên sản phẩm là bắt buộc" });

    if (updated.Price <= 0)
        return BadRequest(new { message = "Giá sản phẩm phải lớn hơn 0" });

    // Cập nhật
    product.Name = updated.Name;
    product.Category = updated.Category;
    product.Price = updated.Price;
    product.Stock = updated.Stock;

    return NoContent(); // 204 - Thành công, không trả dữ liệu
}

// DELETE /api/products/{id}
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
        return NotFound(new { message = $"Không tìm thấy sản phẩm với ID {id}" });

    _products.Remove(product);
    return NoContent(); // 204 - Xóa thành công
}
```

**Giải thích:**
- `NoContent()` trả về 204 - thành công nhưng không có dữ liệu
- `NotFound()` trả về 404 khi không tìm thấy resource
- PUT và DELETE đều là idempotent operations

---

#### Bài tập 4: Query parameters

**Yêu cầu:** Thêm `GET /api/products?category=Electronics&minPrice=100`. Dùng `[FromQuery]` để lọc sản phẩm theo `category` và `minPrice`. Dùng LINQ để thực hiện lọc.

**Đáp án:**

```csharp
[HttpGet]
public IActionResult GetAll(
    [FromQuery] string? category,
    [FromQuery] decimal? minPrice)
{
    var query = _products.AsQueryable();

    if (!string.IsNullOrEmpty(category))
    {
        query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    if (minPrice.HasValue)
    {
        query = query.Where(p => p.Price >= minPrice.Value);
    }

    return Ok(query.ToList());
}
```

**Test các request:**

```bash
# Lấy tất cả sản phẩm
GET /api/products

# Lọc theo category
GET /api/products?category=Electronics

# Lọc theo price
GET /api/products?minPrice=1000

# Kết hợp cả hai
GET /api/products?category=Electronics&minPrice=1000
```

**Giải thích:**
- `[FromQuery]` lấy tham số từ query string
- `AsQueryable()` cho phép xây dựng query động
- `StringComparison.OrdinalIgnoreCase` để so sánh không phân biệt hoa thường

---

#### Bài tập 5: RESTful URI Design

**Yêu cầu:** Thiết kế URI chuẩn REST cho hệ thống quản lý đơn hàng:
- Đơn hàng (Order), chi tiết đơn hàng (OrderItem)
- Mỗi đơn hàng có nhiều OrderItem
- Thiết kế 8 endpoints: CRUD cho Order + CRUD cho OrderItem (nested route)

**Đáp án:**

```csharp
// Models/Order.cs
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled
    public List<OrderItem> Items { get; set; } = new();
}

// Models/OrderItem.cs
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

// Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static List<Order> _orders = new();
    private static int _nextOrderId = 1;
    private static int _nextItemId = 1;

    // GET /api/orders - Lấy tất cả đơn hàng
    [HttpGet]
    public IActionResult GetAll() => Ok(_orders);

    // GET /api/orders/{id} - Lấy một đơn hàng
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });
        return Ok(order);
    }

    // POST /api/orders - Tạo đơn hàng mới
    [HttpPost]
    public IActionResult Create([FromBody] Order order)
    {
        order.Id = _nextOrderId++;
        order.OrderDate = DateTime.UtcNow;
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        
        // Gán ID cho các items
        foreach (var item in order.Items)
        {
            item.Id = _nextItemId++;
            item.OrderId = order.Id;
        }
        
        _orders.Add(order);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PUT /api/orders/{id} - Cập nhật đơn hàng
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Order updated)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });

        order.CustomerName = updated.CustomerName;
        order.Status = updated.Status;
        order.TotalAmount = updated.Items.Sum(i => i.Quantity * i.UnitPrice);
        
        return NoContent();
    }

    // DELETE /api/orders/{id} - Xóa đơn hàng
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });

        _orders.Remove(order);
        return NoContent();
    }
}

// Controllers/OrderItemsController.cs (Nested Controller)
[ApiController]
[Route("api/orders/{orderId}/[controller]")]
public class OrderItemsController : ControllerBase
{
    private static List<Order> _orders = OrdersController._orders; // Thực tế dùng service

    // GET /api/orders/1/orderitems
    [HttpGet]
    public IActionResult GetByOrderId(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}" });
        return Ok(order.Items);
    }

    // GET /api/orders/1/orderitems/5
    [HttpGet("{id:int}")]
    public IActionResult GetById(int orderId, int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}" });

        var item = order.Items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = $"Không tìm thấy item với ID {id}" });

        return Ok(item);
    }

    // POST /api/orders/1/orderitems
    [HttpPost]
    public IActionResult Create(int orderId, [FromBody] OrderItem item)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}" });

        item.Id = _nextItemId++;
        item.OrderId = orderId;
        order.Items.Add(item);
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        return CreatedAtAction(nameof(GetById), new { orderId, id = item.Id }, item);
    }

    // PUT /api/orders/1/orderitems/5
    [HttpPut("{id:int}")]
    public IActionResult Update(int orderId, int id, [FromBody] OrderItem updated)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}" });

        var item = order.Items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = $"Không tìm thấy item với ID {id}" });

        item.ProductName = updated.ProductName;
        item.Quantity = updated.Quantity;
        item.UnitPrice = updated.UnitPrice;
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        return NoContent();
    }

    // DELETE /api/orders/1/orderitems/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int orderId, int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}" });

        var item = order.Items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = $"Không tìm thấy item với ID {id}" });

        order.Items.Remove(item);
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        return NoContent();
    }
}
```

**Tổng kết các endpoints:**

| Resource  | Operation                  | Method | URI                                     |
| --------- | -------------------------- | ------ | --------------------------------------- |
| Order     | Lấy tất cả                 | GET    | `/api/orders`                           |
| Order     | Lấy một                    | GET    | `/api/orders/{id}`                      |
| Order     | Tạo                        | POST   | `/api/orders`                           |
| Order     | Cập nhật                   | PUT    | `/api/orders/{id}`                      |
| Order     | Xóa                        | DELETE | `/api/orders/{id}`                      |
| OrderItem | Lấy tất cả items của order | GET    | `/api/orders/{orderId}/orderitems`      |
| OrderItem | Lấy một item               | GET    | `/api/orders/{orderId}/orderitems/{id}` |
| OrderItem | Tạo item                   | POST   | `/api/orders/{orderId}/orderitems`      |
| OrderItem | Cập nhật item              | PUT    | `/api/orders/{orderId}/orderitems/{id}` |
| OrderItem | Xóa item                   | DELETE | `/api/orders/{orderId}/orderitems/{id}` |

**Giải thích thiết kế:**
- Sử dụng nested route cho OrderItem (`orders/{orderId}/orderitems`)
- OrderItem luôn thuộc về một Order
- URIs thể hiện rõ mối quan hệ parent-child
- HTTP Methods xác định hành động

---

## 📊 Tổng kết Buổi 3

| Chủ đề | Đã học |
|--------|--------|
| **Controller** | `ControllerBase` + `[ApiController]` |
| **Routing** | Attribute Routing, Route Constraints, Route Tokens |
| **HTTP Methods** | GET, POST, PUT, DELETE, PATCH |
| **RESTful API** | 6 nguyên tắc, URI Design, Nested Resources |
| **Model Binding** | `[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromHeader]` |
| **Action Result** | `Ok()`, `NotFound()`, `BadRequest()`, `NoContent()`, `CreatedAtAction()` |
| **Status Codes** | 200, 201, 204, 400, 404, 500 |

**Key Takeaways:**
1. ✅ Web API dùng `ControllerBase` (không phải `Controller`)
2. ✅ Luôn dùng Attribute Routing với `[Route("api/[controller]")]`
3. ✅ RESTful API sử dụng danh từ số nhiều cho URI, HTTP Methods cho hành động
4. ✅ `[ApiController]` tự động validate và binding
5. ✅ Trả về đúng status code cho từng scenario
6. ✅ Dùng DTO để tách biệt model và API contract

---

✅ **Hoàn thành nội dung Buổi 3.**


# Buổi 4: DTO, Validation, Swagger và kiểm thử API bằng Postman

---

## 📌 1. Lý thuyết

### 1.1. DTO (Data Transfer Object)

#### DTO là gì?

**DTO (Data Transfer Object)** là một object đơn giản chỉ chứa dữ liệu, không chứa bất kỳ logic nghiệp vụ nào. DTO được sử dụng để vận chuyển dữ liệu giữa các tầng của ứng dụng hoặc giữa client và server.

**Bản chất:** DTO là một "container" dữ liệu thuần túy, không có hành vi (methods), chỉ có các property.

#### Tại sao cần DTO?

| Vấn đề | Mô tả | Giải pháp với DTO |
|--------|-------|-------------------|
| **Bảo mật thông tin** | Entity chứa thông tin nhạy cảm (PasswordHash, Salt, Token) | DTO chỉ chọn lọc các field cần thiết |
| **Circular Reference** | Entity có navigation properties (Student ↔ Class) | DTO không chứa navigation properties |
| **Over-posting** (Mass Assignment) | Hacker gửi thêm field không mong muốn (isAdmin) | DTO chỉ chứa field cho phép client thay đổi |
| **Performance** | Entity có nhiều field không cần thiết | DTO giảm kích thước payload |
| **Separation of Concerns** | Entity gắn với database schema | DTO tách biệt API contract khỏi database |

#### Vấn đề khi trả về trực tiếp Entity

```csharp
// ❌ KHÔNG NÊN - Trả về Entity trực tiếp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }    // ⚠️ Lộ thông tin nhạy cảm
    public string Email { get; set; }
    public Class Class { get; set; }            // ⚠️ Nguy cơ Circular Reference
    public bool IsAdmin { get; set; }           // ⚠️ Risk of over-posting
}

[HttpGet("{id}")]
public IActionResult GetStudent(int id)
{
    var student = _dbContext.Students.Find(id);
    return Ok(student); // ❌ Trả về Entity -> Lộ PasswordHash
}
```

**Hậu quả:**
1. **Lộ dữ liệu nhạy cảm:** `PasswordHash` sẽ được serialize thành JSON
2. **Circular Reference:** `Student.Class.Students.Class...` gây lỗi khi serialize
3. **Over-posting:** Client có thể gửi thêm `IsAdmin: true` khi tạo mới

#### Tạo DTO riêng cho từng ngữ cảnh

```csharp
// ✅ DTO cho tạo mới (Input/Request)
public class StudentCreateDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty; // Chỉ nhận password, không lưu hash

    [Range(18, 100)]
    public int Age { get; set; }

    public string Major { get; set; } = string.Empty;
}

// ✅ DTO cho cập nhật (Input)
public class StudentUpdateDto
{
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(18, 100)]
    public int Age { get; set; }

    public string Major { get; set; } = string.Empty;
}

// ✅ DTO cho trả về (Output/Response)
public class StudentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Major { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty; // Chỉ lấy tên, không lấy cả object
}

// ✅ DTO cho danh sách (List)
public class StudentListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

#### Over-posting (Mass Assignment) - Lỗ hổng bảo mật

**Vấn đề:** Client gửi thêm các field không mong đợi:

```json
// Client gửi thêm field isAdmin
POST /api/students
{
    "name": "Hacker",
    "email": "hacker@evil.com",
    "password": "123456",
    "isAdmin": true  // ⚠️ Field nguy hiểm
}
```

**Khắc phục với DTO:**

```csharp
// ✅ DTO chỉ chứa field cho phép client thay đổi
public class StudentCreateDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    // ❌ Không có IsAdmin
}

[HttpPost]
public IActionResult Create([FromBody] StudentCreateDto dto)
{
    // ASP.NET Core tự động bỏ qua các field không có trong DTO
    var student = new Student
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = Hash(dto.Password),
        IsAdmin = false // Giá trị mặc định, không lấy từ client
    };
    // ...
}
```

#### AutoMapper - Tự động mapping Entity ↔ DTO

```csharp
// Install: dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

// AutoMapper Profile
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentResponseDto>()
            .ForMember(dest => dest.ClassName, 
                opt => opt.MapFrom(src => src.Class.Name));
        
        CreateMap<StudentCreateDto, Student>()
            .ForMember(dest => dest.PasswordHash, 
                opt => opt.Ignore()); // Không map PasswordHash
        
        CreateMap<StudentUpdateDto, Student>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

// Program.cs
builder.Services.AddAutoMapper(typeof(Program));

// Trong Controller
public class StudentsController : ControllerBase
{
    private readonly IMapper _mapper;
    
    public StudentsController(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    [HttpGet]
    public IActionResult GetAll()
    {
        var students = _service.GetAll();
        var dtos = _mapper.Map<List<StudentResponseDto>>(students);
        return Ok(dtos);
    }
}
```

---

### 1.2. Data Annotation

#### Data Annotation là gì?

**Data Annotation** là các attribute (thuộc tính) trong C# được sử dụng để khai báo quy tắc validation một cách declarative (khai báo).

#### Các Data Annotation phổ biến

| Attribute | Mô tả | Ví dụ |
|-----------|-------|-------|
| `[Required]` | Bắt buộc nhập | `[Required(ErrorMessage = "Tên không được để trống")]` |
| `[StringLength(max)]` | Độ dài chuỗi tối đa | `[StringLength(50, MinimumLength = 3)]` |
| `[MinLength(min)]` | Độ dài chuỗi tối thiểu | `[MinLength(3)]` |
| `[MaxLength(max)]` | Độ dài chuỗi tối đa | `[MaxLength(50)]` |
| `[Range(min, max)]` | Giá trị số trong khoảng | `[Range(18, 100)]` |
| `[EmailAddress]` | Phải là định dạng Email | `[EmailAddress]` |
| `[RegularExpression(pattern)]` | Phải khớp với Regex | `[RegularExpression(@"^[a-zA-Z]+$")]` |
| `[Compare(otherProperty)]` | So sánh với property khác | `[Compare(nameof(Password))]` |
| `[Phone]` | Phải là số điện thoại | `[Phone]` |
| `[Url]` | Phải là URL hợp lệ | `[Url]` |
| `[CreditCard]` | Phải là số thẻ tín dụng | `[CreditCard]` |

#### Ví dụ chi tiết

```csharp
public class UserRegisterDto
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username phải từ 3-20 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ chứa chữ cái, số và dấu gạch dưới")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Password phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPassword là bắt buộc")]
    [Compare(nameof(Password), ErrorMessage = "Password và ConfirmPassword không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age là bắt buộc")]
    [Range(18, 60, ErrorMessage = "Age phải từ 18 đến 60")]
    public int Age { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;
}
```

#### Custom Error Messages

```csharp
public class ProductCreateDto
{
    [Required(ErrorMessage = "{0} là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} không được vượt quá {1} ký tự")]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "{0} phải lớn hơn hoặc bằng {1}")]
    [Display(Name = "Giá")]
    public decimal Price { get; set; }
}
```

---

### 1.3. Model Validation

#### Model Validation là gì?

**Model Validation** là quá trình kiểm tra dữ liệu đầu vào (input) từ client TRƯỚC khi xử lý trong business logic.

#### Luồng xử lý Validation

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ HTTP POST với JSON body
       ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  REQUEST    │────▶│  VALIDATE   │────▶│  REJECTED   │
│ JSON body   │     │ Model State │     │ 400 Bad     │
└─────────────┘     └──────┬──────┘     │ Request     │
                           │ Valid      └─────────────┘
                           ▼
                    ┌─────────────┐
                    │  ACCEPTED   │
                    │ 200 / 201   │
                    └─────────────┘
```

#### Xử lý Validation trong Controller

**Với `[ApiController]` (tự động):**

```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] StudentCreateDto dto)
    {
        // ✅ Không cần kiểm tra ModelState.IsValid
        // Framework tự động trả về 400 Bad Request nếu invalid
        // response bao gồm chi tiết lỗi validation

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }
}
```

**Nếu không dùng `[ApiController]` (cần kiểm tra thủ công):**

```csharp
public class StudentsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] StudentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Trả về lỗi validation chi tiết
        }

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }
}
```

#### Response Validation Error mặc định (từ `[ApiController]`)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-0f8c2b9f7e8a4d6b9c7e8a4d6b9c7e8a-...",
  "errors": {
    "Username": [
      "Username là bắt buộc"
    ],
    "Email": [
      "Email không hợp lệ"
    ],
    "Age": [
      "Age phải từ 18 đến 60"
    ]
  }
}
```

#### Custom Validation - Implement IValidatableObject

```csharp
public class OrderCreateDto : IValidatableObject
{
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Discount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Items.Any())
        {
            yield return new ValidationResult("Đơn hàng phải có ít nhất 1 sản phẩm", 
                new[] { nameof(Items) });
        }

        if (Discount < 0 || Discount > 100)
        {
            yield return new ValidationResult("Discount phải từ 0 đến 100", 
                new[] { nameof(Discount) });
        }

        var total = Items.Sum(i => i.Quantity * i.UnitPrice);
        if (Discount > 0 && total < 1000000)
        {
            yield return new ValidationResult(
                "Áp dụng discount yêu cầu tổng giá trị đơn hàng >= 1,000,000 VND",
                new[] { nameof(Discount) });
        }
    }
}
```

#### Custom Validation Attribute

```csharp
// Tạo Custom Attribute
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MustBeEvenAttribute : ValidationAttribute
{
    public MustBeEvenAttribute()
    {
        ErrorMessage = "{0} phải là số chẵn";
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is int intValue)
        {
            return intValue % 2 == 0;
        }

        return false;
    }
}

// Sử dụng
public class GameDto
{
    [MustBeEven]
    public int Level { get; set; }
}
```

#### Custom Validation với Dependency Injection

```csharp
public class UniqueUsernameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var dbContext = validationContext.GetService<ApplicationDbContext>();
        var username = value.ToString();

        if (dbContext.Users.Any(u => u.Username == username))
        {
            return new ValidationResult($"Username '{username}' đã tồn tại");
        }

        return ValidationResult.Success;
    }
}
```

---

### 1.4. Swagger/OpenAPI

#### Swagger/OpenAPI là gì?

| Thuật ngữ | Mô tả |
|-----------|-------|
| **OpenAPI Specification** (OAS) | Tiêu chuẩn để mô tả các RESTful API (định dạng JSON/YAML) |
| **Swagger** | Bộ công cụ xây dựng xung quanh OpenAPI Specification |
| **Swagger UI** | Giao diện web hiển thị và test API |

**Lợi ích:**
- ✅ Tự động sinh tài liệu API
- ✅ Giao diện test API trực tiếp trên trình duyệt
- ✅ Hỗ trợ Frontend team dễ dàng tích hợp
- ✅ Tạo client SDK tự động (OpenAPI Generator)

#### Cấu hình Swagger trong .NET 8

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Cấu hình thông tin API
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Management API",
        Version = "v1",
        Description = "API quản lý sinh viên - Học phần Lập trình .NET",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@university.edu",
            Url = new Uri("https://university.edu/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // 2. Đọc XML Comments (nếu bật trong project)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // 3. Thêm security definition (JWT)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào đây",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 4. Cấu hình schema cho DTO
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

var app = builder.Build();

// Enable Swagger chỉ trong Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.RoutePrefix = "swagger"; // hoặc để string.Empty để swagger ở root
    });
}
```

#### XML Comments cho Swagger

**Bật XML Documentation trong project:**
```xml
<!-- StudentApi.csproj -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**Viết XML Comments:**

```csharp
/// <summary>
/// Controller quản lý sinh viên
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    /// <summary>
    /// Lấy danh sách tất cả sinh viên
    /// </summary>
    /// <returns>Danh sách sinh viên (StudentResponseDto)</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    /// <response code="401">Chưa xác thực</response>
    /// <response code="403">Không có quyền</response>
    [HttpGet]
    public IActionResult GetAll()
    {
        // ...
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một sinh viên
    /// </summary>
    /// <param name="id">ID của sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        // ...
    }

    /// <summary>
    /// Tạo sinh viên mới
    /// </summary>
    /// <param name="dto">Dữ liệu sinh viên cần tạo</param>
    /// <returns>Sinh viên đã tạo</returns>
    /// <response code="201">Tạo sinh viên thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost]
    public IActionResult Create([FromBody] StudentCreateDto dto)
    {
        // ...
    }
}
```

#### Data Annotation hiển thị trên Swagger

```csharp
public class StudentCreateDto
{
    /// <summary>
    /// Tên đầy đủ của sinh viên
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    [Required(ErrorMessage = "Tên không được để trống")]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email sinh viên (dùng để đăng nhập)
    /// </summary>
    /// <example>student@university.edu</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu (tối thiểu 6 ký tự)
    /// </summary>
    /// <example>Password123</example>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ngành học (Major)
    /// </summary>
    /// <example>Công nghệ thông tin</example>
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Tuổi sinh viên (18-60)
    /// </summary>
    /// <example>20</example>
    [Range(18, 60)]
    public int Age { get; set; }
}
```

---

## 🛠️ 2. Thực hành

### 2.1. Demo CRUD với DTO và Validation

#### Step 1: Tạo Project

```bash
dotnet new webapi -n StudentApi
cd StudentApi
```

#### Step 2: Định nghĩa Models

```csharp
// Models/Student.cs
namespace StudentApi.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### Step 3: Định nghĩa DTOs

```csharp
// DTOs/StudentCreateDto.cs
namespace StudentApi.DTOs;

/// <summary>
/// DTO cho việc tạo mới sinh viên
/// </summary>
public class StudentCreateDto
{
    /// <summary>
    /// Tên đầy đủ của sinh viên
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    [Required(ErrorMessage = "Tên không được để trống")]
    [StringLength(50, MinimumLength = 3, 
        ErrorMessage = "Tên phải từ 3-50 ký tự")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email sinh viên
    /// </summary>
    /// <example>student@university.edu</example>
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu (tối thiểu 6 ký tự)
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu
    /// </summary>
    [Required]
    [Compare(nameof(Password), ErrorMessage = "Password và ConfirmPassword không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Ngành học
    /// </summary>
    /// <example>Công nghệ thông tin</example>
    [StringLength(50)]
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Tuổi sinh viên (18-60)
    /// </summary>
    /// <example>20</example>
    [Range(18, 60, ErrorMessage = "Tuổi phải từ 18 đến 60")]
    public int Age { get; set; }
}

// DTOs/StudentUpdateDto.cs
public class StudentUpdateDto
{
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string Major { get; set; } = string.Empty;

    [Range(18, 60)]
    public int? Age { get; set; }
}

// DTOs/StudentResponseDto.cs
public class StudentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### Step 4: Tạo Service

```csharp
// Services/IStudentService.cs
public interface IStudentService
{
    List<Student> GetAll();
    Student? GetById(int id);
    Student Create(StudentCreateDto dto);
    Student? Update(int id, StudentUpdateDto dto);
    bool Delete(int id);
}

// Services/StudentService.cs
public class StudentService : IStudentService
{
    private static List<Student> _students = new();
    private static int _nextId = 1;

    public StudentService()
    {
        // Seed data
        if (!_students.Any())
        {
            _students.Add(new Student
            {
                Id = _nextId++,
                Name = "Nguyễn Văn A",
                Email = "a@university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Major = "Công nghệ thông tin",
                Age = 20,
                CreatedAt = DateTime.UtcNow
            });
            _students.Add(new Student
            {
                Id = _nextId++,
                Name = "Trần Thị B",
                Email = "b@university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Major = "Hệ thống thông tin",
                Age = 21,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    public List<Student> GetAll() => _students;

    public Student? GetById(int id) => _students.FirstOrDefault(s => s.Id == id);

    public Student Create(StudentCreateDto dto)
    {
        var student = new Student
        {
            Id = _nextId++,
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Major = dto.Major,
            Age = dto.Age,
            CreatedAt = DateTime.UtcNow
        };
        _students.Add(student);
        return student;
    }

    public Student? Update(int id, StudentUpdateDto dto)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            student.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Email))
            student.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.Major))
            student.Major = dto.Major;

        if (dto.Age.HasValue)
            student.Age = dto.Age.Value;

        return student;
    }

    public bool Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null) return false;
        return _students.Remove(student);
    }
}
```

#### Step 5: Tạo Controller

```csharp
// Controllers/StudentsController.cs
using Microsoft.AspNetCore.Mvc;
using StudentApi.DTOs;
using StudentApi.Services;

namespace StudentApi.Controllers;

/// <summary>
/// Controller quản lý sinh viên
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách tất cả sinh viên
    /// </summary>
    /// <returns>Danh sách sinh viên</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<StudentResponseDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var students = _service.GetAll();
        var dtos = students.Select(s => new StudentResponseDto
        {
            Id = s.Id,
            Name = s.Name,
            Email = s.Email,
            Major = s.Major,
            Age = s.Age,
            CreatedAt = s.CreatedAt
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Lấy thông tin sinh viên theo ID
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        var student = _service.GetById(id);
        if (student == null)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });

        var dto = new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            Major = student.Major,
            Age = student.Age,
            CreatedAt = student.CreatedAt
        };
        return Ok(dto);
    }

    /// <summary>
    /// Tạo sinh viên mới
    /// </summary>
    /// <param name="dto">Dữ liệu sinh viên</param>
    /// <returns>Sinh viên đã tạo</returns>
    /// <response code="201">Tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] StudentCreateDto dto)
    {
        try
        {
            var student = _service.Create(dto);
            var response = new StudentResponseDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Major = student.Major,
                Age = student.Age,
                CreatedAt = student.CreatedAt
            };
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo sinh viên", error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <param name="dto">Dữ liệu cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Update(int id, [FromBody] StudentUpdateDto dto)
    {
        var student = _service.Update(id, dto);
        if (student == null)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });

        var response = new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            Major = student.Major,
            Age = student.Age,
            CreatedAt = student.CreatedAt
        };
        return Ok(response);
    }

    /// <summary>
    /// Xóa sinh viên
    /// </summary>
    /// <param name="id">ID sinh viên cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    /// <response code="204">Xóa thành công</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted)
            return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });

        return NoContent();
    }
}
```

#### Step 6: Cấu hình Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Management API",
        Version = "v1",
        Description = "API quản lý sinh viên - Học phần Lập trình .NET",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@university.edu"
        }
    });

    // Include XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

### 2.2. Kiểm thử bằng Postman

#### Cài đặt và Sử dụng Postman

**1. Cài đặt Postman:**
- Tải từ: https://www.postman.com/downloads/
- Hoặc dùng Web Version: https://web.postman.co/

**2. Tạo Environment:**

```
Name: StudentApi - Development
Variable: baseUrl = https://localhost:5001
```

**3. Tạo Collection "Student API":**

**Request 1: Create Student - POST**
```
Method: POST
URL: {{baseUrl}}/api/students
Headers: Content-Type: application/json
Body (raw JSON):
{
    "name": "Phạm Văn C",
    "email": "c@university.edu",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "major": "Khoa học dữ liệu",
    "age": 22
}
```

**Request 2: Get All Students - GET**
```
Method: GET
URL: {{baseUrl}}/api/students
```

**Request 3: Get Student By ID - GET**
```
Method: GET
URL: {{baseUrl}}/api/students/1
```

**Request 4: Update Student - PUT**
```
Method: PUT
URL: {{baseUrl}}/api/students/3
Body (raw JSON):
{
    "name": "Phạm Văn C Updated",
    "major": "AI",
    "age": 23
}
```

**Request 5: Delete Student - DELETE**
```
Method: DELETE
URL: {{baseUrl}}/api/students/3
```

#### Test Validation Errors

**Test 1: Missing Required Field**
```json
{
    "name": "Test Student",
    "email": "test@test.com"
    // Thiếu password, confirmPassword
}
```
**Expected Response:** 400 Bad Request với validation errors

**Test 2: Invalid Email**
```json
{
    "name": "Test Student",
    "email": "invalid-email",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "age": 20
}
```
**Expected Response:** 400 Bad Request - "Email không hợp lệ"

**Test 3: Password Mismatch**
```json
{
    "name": "Test Student",
    "email": "test@test.com",
    "password": "SecurePass123",
    "confirmPassword": "DifferentPassword",
    "age": 20
}
```
**Expected Response:** 400 Bad Request - "Password và ConfirmPassword không khớp"

**Test 4: Age Out of Range**
```json
{
    "name": "Test Student",
    "email": "test@test.com",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "age": 16
}
```
**Expected Response:** 400 Bad Request - "Tuổi phải từ 18 đến 60"

#### Postman Test Scripts

```javascript
// Tests tab trong Postman
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Response has student data", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('name');
    pm.expect(jsonData).to.have.property('email');
});

pm.test("Content-Type is JSON", function () {
    pm.response.to.have.header("Content-Type", "application/json; charset=utf-8");
});

// Lưu ID vào biến environment
var jsonData = pm.response.json();
pm.environment.set("studentId", jsonData.id);
```

---

### 2.3. Bài thực hành

#### Bài tập 1: UserRegisterDto với Validation

**Yêu cầu:** Tạo `UserRegisterDto` gồm các trường: Username, Password, Email, Age. Thêm Validation cho từng trường.

**Đáp án:**

```csharp
// DTOs/UserRegisterDto.cs
namespace UserApi.DTOs;

/// <summary>
/// DTO đăng ký người dùng
/// </summary>
public class UserRegisterDto
{
    /// <summary>
    /// Tên đăng nhập
    /// </summary>
    /// <example>john_doe</example>
    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username phải từ 3-20 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", 
        ErrorMessage = "Username chỉ chứa chữ cái, số và dấu gạch dưới")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Password phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu
    /// </summary>
    [Required(ErrorMessage = "ConfirmPassword là bắt buộc")]
    [Compare(nameof(Password), ErrorMessage = "Password và ConfirmPassword không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tuổi
    /// </summary>
    /// <example>25</example>
    [Required(ErrorMessage = "Age là bắt buộc")]
    [Range(18, 60, ErrorMessage = "Age phải từ 18 đến 60")]
    public int Age { get; set; }
}
```

**Giải thích:**
- `[Required]` - Bắt buộc nhập
- `[StringLength]` - Giới hạn độ dài chuỗi
- `[RegularExpression]` - Kiểm tra format username và password complexity
- `[EmailAddress]` - Kiểm tra định dạng email
- `[Range]` - Kiểm tra giá trị số
- `[Compare]` - So sánh password và confirm password

---

#### Bài tập 2: Cấu hình Swagger XML Comments

**Yêu cầu:** Cấu hình để Swagger có thể đọc được XML Comments từ các Controller và DTO. Viết summary giải thích chức năng cho một Endpoint bất kỳ.

**Đáp án:**

**Step 1: Bật XML Documentation trong .csproj**

```xml
<!-- UserApi.csproj -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**Step 2: Viết XML Comments**

```csharp
// Controllers/UsersController.cs
/// <summary>
/// Controller quản lý người dùng
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Đăng ký tài khoản người dùng mới
    /// </summary>
    /// <param name="dto">Thông tin đăng ký</param>
    /// <returns>Thông tin người dùng đã tạo</returns>
    /// <response code="200">Đăng ký thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="409">Username hoặc Email đã tồn tại</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }
}

// DTOs/UserRegisterDto.cs
/// <summary>
/// DTO đăng ký người dùng
/// </summary>
public class UserRegisterDto
{
    /// <summary>
    /// Tên đăng nhập (3-20 ký tự, chỉ chữ cái, số và dấu gạch dưới)
    /// </summary>
    /// <example>john_doe</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu (ít nhất 6 ký tự, có chữ hoa, chữ thường và số)
    /// </summary>
    /// <example>SecurePass123</example>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
```

**Step 3: Cấu hình Swagger**

```csharp
// Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "API quản lý người dùng"
    });

    // Include XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

---

#### Bài tập 3: Test với Postman

**Yêu cầu:** Gọi Endpoint đăng ký User bằng Postman. Thử gửi Body thiếu trường Email và xem kết quả lỗi trả về.

**Đáp án:**

**Test 1: Valid Request**
```
POST {{baseUrl}}/api/users/register
Content-Type: application/json

{
    "username": "john_doe",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "email": "john@example.com",
    "age": 25
}
```

**Expected Response:**
```json
{
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "age": 25,
    "createdAt": "2024-01-15T10:30:00Z"
}
```

**Test 2: Missing Email**
```
POST {{baseUrl}}/api/users/register
Content-Type: application/json

{
    "username": "john_doe",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "age": 25
}
```

**Expected Response (400 Bad Request):**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-...",
    "errors": {
        "Email": [
            "Email là bắt buộc"
        ]
    }
}
```

**Test 3: Invalid Email Format**
```json
{
    "username": "john_doe",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123",
    "email": "invalid-email",
    "age": 25
}
```

**Expected Response (400 Bad Request):**
```json
{
    "errors": {
        "Email": [
            "Email không hợp lệ"
        ]
    }
}
```

**Test 4: Password Mismatch**
```json
{
    "username": "john_doe",
    "password": "SecurePass123",
    "confirmPassword": "DifferentPassword",
    "email": "john@example.com",
    "age": 25
}
```

**Expected Response (400 Bad Request):**
```json
{
    "errors": {
        "ConfirmPassword": [
            "Password và ConfirmPassword không khớp"
        ]
    }
}
```

---

#### Bài tập 4: Custom Validation Attribute

**Yêu cầu:** Tạo Custom Validation Attribute `[MustBeEven]` để kiểm tra một property số nguyên phải là số chẵn. Áp dụng vào một DTO.

**Đáp án:**

```csharp
// Attributes/MustBeEvenAttribute.cs
using System.ComponentModel.DataAnnotations;

namespace Common.Attributes;

/// <summary>
/// Custom validation attribute kiểm tra giá trị phải là số chẵn
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class MustBeEvenAttribute : ValidationAttribute
{
    public MustBeEvenAttribute()
    {
        ErrorMessage = "{0} phải là số chẵn";
    }

    /// <summary>
    /// Kiểm tra giá trị có phải là số chẵn không
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        return value switch
        {
            int intValue => intValue % 2 == 0,
            long longValue => longValue % 2 == 0,
            short shortValue => shortValue % 2 == 0,
            _ => false
        };
    }

    /// <summary>
    /// Format error message
    /// </summary>
    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name);
    }
}

// DTOs/GameDto.cs
namespace GameApi.DTOs;

public class GameDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MustBeEven(ErrorMessage = "Level phải là số chẵn")]
    [Range(1, 100)]
    public int Level { get; set; }

    [MustBeEven]
    public int? BonusPoints { get; set; }
}

// Test the validation
public class GameController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] GameDto dto)
    {
        // Framework tự động kiểm tra [MustBeEven]
        // Nếu Level là số lẻ -> trả về 400 Bad Request
        return Ok(dto);
    }
}
```

**Test với Postman:**

**Test 1: Level là số chẵn (Valid)**
```json
{
    "name": "Super Mario",
    "level": 10
}
```
**Expected:** 200 OK

**Test 2: Level là số lẻ (Invalid)**
```json
{
    "name": "Super Mario",
    "level": 11
}
```
**Expected (400 Bad Request):**
```json
{
    "errors": {
        "Level": [
            "Level phải là số chẵn"
        ]
    }
}
```

**Nâng cao: MustBeMultipleOfAttribute**

```csharp
// Attributes/MustBeMultipleOfAttribute.cs
public class MustBeMultipleOfAttribute : ValidationAttribute
{
    private readonly int _divisor;

    public MustBeMultipleOfAttribute(int divisor)
    {
        _divisor = divisor;
        ErrorMessage = $"Giá trị phải là bội số của {divisor}";
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is int intValue)
        {
            return intValue % _divisor == 0;
        }

        return false;
    }
}

// Sử dụng
public class ProductDto
{
    [MustBeMultipleOf(5)]
    public int Quantity { get; set; } // Quantity phải chia hết cho 5
}
```

---

## 📊 Tổng kết Buổi 4

| Chủ đề | Đã học |
|--------|--------|
| **DTO** | Tách biệt Entity khỏi API Contract, Over-posting, AutoMapper |
| **Data Annotation** | `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`, `[RegularExpression]` |
| **Model Validation** | Validation pipeline, `[ApiController]` tự động validation, IValidatableObject |
| **Custom Validation** | Tạo Custom Attribute, sử dụng DI trong Validation |
| **Swagger/OpenAPI** | Cấu hình Swagger, XML Comments, API Documentation |
| **Postman** | Testing, Collections, Environments, Test Scripts |

**Key Takeaways:**
1. ✅ **DTO** bảo vệ dữ liệu nhạy cảm và ngăn chặn over-posting
2. ✅ **Data Annotation** khai báo validation một cách declarative
3. ✅ `[ApiController]` tự động xử lý validation, trả về 400 Bad Request
4. ✅ **Swagger** + XML Comments tạo tài liệu API tự động
5. ✅ **Postman** là công cụ kiểm thử API chuyên nghiệp
6. ✅ **Custom Validation** cho phép tạo quy tắc kiểm tra riêng

---

✅ **Hoàn thành nội dung Buổi 4.**

# Buổi 5: Entity Framework Core: DbContext, DbSet, Code First, Migration

---

## 📌 1. Lý thuyết

### 1.1. ORM (Object-Relational Mapping)

#### ORM là gì?

**ORM (Object-Relational Mapping)** là kỹ thuật ánh xạ các bảng trong cơ sở dữ liệu (Database) thành các đối tượng (Objects) trong mã nguồn.

```
┌──────────────────────────┐         ┌─────────────────────────┐
│   C# Classes (Objects)   │  ORM    │   Database (Tables)     │
├──────────────────────────┤◄──────► ├─────────────────────────┤
│   public class Student   │         │   Students Table        │
│   {                      │         │   ├── Id (int)          │
│     public int Id {get;} │         │   ├── Name (nvarchar)   │
│     public string Name   │         │   └── Age (int)         │
│   }                      │         │                         │
└──────────────────────────┘         └─────────────────────────┘
```

#### Tại sao cần ORM?

| Lợi ích | Mô tả |
|---------|-------|
| **Tăng tốc độ phát triển** | Lập trình viên không cần viết SQL thủ công |
| **Giảm lỗi** | Tránh lỗi cú pháp SQL, lỗi injection |
| **Làm việc với OOP** | Sử dụng C# LINQ thay vì SQL |
| **Đa nền tảng** | Hỗ trợ nhiều loại database |
| **Migration** | Tự động đồng bộ code và database |

#### Các ORM phổ biến

| ORM | Mô tả |
|-----|-------|
| **Entity Framework Core** | Microsoft, phổ biến nhất trong .NET Core |
| **Dapper** | Micro-ORM, performance tốt |
| **NHibernate** | ORM mạnh mẽ, dựa trên Hibernate |
| **SQLite-net** | ORM cho SQLite |

#### So sánh SQL và LINQ với ORM

```csharp
// ❌ SQL thủ công (dễ sai, khó bảo trì)
var sql = "SELECT * FROM Students WHERE Age > 18 AND Major = 'CNTT'";
var students = db.ExecuteQuery(sql);

// ✅ ORM với LINQ (type-safe, dễ đọc)
var students = await _context.Students
    .Where(s => s.Age > 18 && s.Major == "CNTT")
    .ToListAsync();
```

---

### 1.2. Entity Framework Core

#### EF Core là gì?

**Entity Framework Core** là framework ORM mã nguồn mở, nhẹ, đa nền tảng do Microsoft phát triển, là phiên bản kế thừa của Entity Framework 6.

**Đặc điểm nổi bật:**

| Đặc điểm | Mô tả |
|----------|-------|
| **Đa nền tảng** | Chạy trên Windows, Linux, macOS |
| **Lightweight** | Tối ưu hơn EF6, ít feature hơn |
| **Performance** | Nhanh hơn EF6 |
| **LINQ Support** | Query mạnh mẽ với LINQ |
| **Migration** | Code-first migration |
| **Đa database** | SQL Server, MySQL, PostgreSQL, SQLite, In-Memory |

#### Phương pháp tiếp cận

**1. Code First** (Quan trọng nhất)
```
C# Classes → Migration → Database
```
Viết code (Entity Classes) trước, tự động sinh database.

**2. Database First**
```
Database → Scaffold → C# Classes
```
Có database trước, tự động sinh code.

#### Kiến trúc EF Core

```
┌─────────────────────────────────────────────────────────────┐
│                    Ứng dụng của bạn                         │
│                         │                                   │
│                         ▼                                   │
│                 ┌──────────────┐                            │
│                 │   DbContext  │   ◄─── Lớp trung tâm       │
│                 └──────┬───────┘                            │
│                        │                                    │
│              ┌─────────┴─────────┐                          │
│              │                   │                          │
│              ▼                   ▼                          │
│        ┌──────────┐      ┌──────────────┐                   │
│        │  LINQ    │      │  Change      │                   │
│        │  Queries │      │  Tracking    │                   │
│        └────┬─────┘      └──────┬───────┘                   │
│             │                   │                           │
│             └────────┬──────────┘                           │
│                      │                                      │
│                      ▼                                      │
│              ┌──────────────┐                               │
│              │  Provider    │  ◄─── Database Provider       │
│              │  (SQL, My)   │                               │
│              └──────┬───────┘                               │
│                     │                                       │
└─────────────────────┼───────────────────────────────────────┘
                      │
                      ▼
              ┌──────────────┐
              │   Database   │
              │  (SQL / My)  │
              └──────────────┘
```

---

### 1.3. DbContext

#### DbContext là gì?

**DbContext** là lớp trung tâm, đại diện cho một phiên làm việc (session) với cơ sở dữ liệu.

**Nhiệm vụ chính:**

| Nhiệm vụ | Mô tả |
|----------|-------|
| **Connection Management** | Quản lý kết nối database |
| **Entity Tracking** | Theo dõi thay đổi của entities |
| **Query Execution** | Thực thi LINQ queries |
| **Save Changes** | Lưu thay đổi vào database |
| **Transaction Management** | Quản lý transactions |

#### Tạo DbContext

```csharp
using Microsoft.EntityFrameworkCore;

namespace StudentApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet properties
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    // Override OnConfiguring (nếu không dùng DI)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=MyDb;Trusted_Connection=True;");
        }
    }

    // Override OnModelCreating để cấu hình relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cấu hình relationships, constraints, indexes
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.StudentId, e.CourseId });
    }
}
```

#### Vòng đời của DbContext trong ASP.NET Core

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

| Lifetime | Mô tả | Khi dùng |
|----------|-------|----------|
| **Scoped** (Mặc định) | 1 instance cho mỗi HTTP request | Web API, MVC |
| **Transient** | 1 instance mỗi lần request | Hiếm khi |
| **Singleton** | 1 instance cho toàn app | ⚠️ KHÔNG DÙNG |

⚠️ **Lưu ý quan trọng:** DbContext KHÔNG thread-safe. Không dùng Singleton cho DbContext.

```csharp
// ✅ Đúng - Scoped
builder.Services.AddDbContext<ApplicationDbContext>(options => ...);

// ❌ Sai - Singleton (gây lỗi)
builder.Services.AddSingleton<ApplicationDbContext>();
```

---

### 1.4. DbSet

#### DbSet là gì?

**DbSet\<T\>** đại diện cho một bảng trong cơ sở dữ liệu. Được khai báo như các thuộc tính (Properties) bên trong `DbContext`.

```csharp
public class ApplicationDbContext : DbContext
{
    // Đại diện cho bảng Students trong Database
    public DbSet<Student> Students { get; set; }
    
    // Đại diện cho bảng Courses
    public DbSet<Course> Courses { get; set; }
    
    // Đại diện cho bảng Enrollments (Many-to-Many)
    public DbSet<Enrollment> Enrollments { get; set; }
}
```

#### Các thao tác với DbSet

```csharp
// CRUD Operations với DbSet

// 1. CREATE - Thêm mới
var student = new Student { Name = "Nguyễn Văn A", Age = 20 };
_context.Students.Add(student);
await _context.SaveChangesAsync();

// 2. READ - Lấy dữ liệu
// Lấy tất cả
var allStudents = await _context.Students.ToListAsync();

// Lấy theo điều kiện
var activeStudents = await _context.Students
    .Where(s => s.IsActive)
    .ToListAsync();

// Lấy một
var student = await _context.Students.FindAsync(1);
var student2 = await _context.Students.FirstOrDefaultAsync(s => s.Email == "a@test.com");

// 3. UPDATE - Cập nhật
var student = await _context.Students.FindAsync(1);
student.Name = "Nguyễn Văn B";
await _context.SaveChangesAsync();

// 4. DELETE - Xóa
var student = await _context.Students.FindAsync(1);
_context.Students.Remove(student);
await _context.SaveChangesAsync();
```

---

### 1.5. Migration

#### Migration là gì?

**Migration** là tính năng giúp theo dõi các thay đổi của mô hình (Entity Classes) và cập nhật cơ sở dữ liệu cho đồng bộ.

```
Code Changes → Migration File → Database Update

┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Change     │     │  Migration  │     │  Database   │
│  Model      │────▶│  (C# file)  │────▶│  Updated    │
│  (Entity)   │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
```

#### Luồng làm việc với Migration

```
1. Thay đổi Entity Classes
          ↓
2. dotnet ef migrations add MigrationName
          ↓
3. Kiểm tra Migration files
          ↓
4. dotnet ef database update
          ↓
5. Database được cập nhật
```

#### Các lệnh Migration

**Sử dụng .NET CLI (Khuyến nghị):**

```bash
# 1. Cài đặt EF Core Tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# 2. Thêm migration mới
dotnet ef migrations add InitialCreate

# 3. Cập nhật database
dotnet ef database update

# 4. Xóa migration mới nhất (chưa apply)
dotnet ef migrations remove

# 5. Xem danh sách migrations
dotnet ef migrations list

# 6. Script migration (sinh SQL script)
dotnet ef migrations script

# 7. Rollback (quay lại migration cụ thể)
dotnet ef database update PreviousMigrationName

# 8. Revert to empty database
dotnet ef database update 0
```

**Sử dụng Package Manager Console (Visual Studio):**

```powershell
# 1. Thêm migration
Add-Migration InitialCreate

# 2. Cập nhật database
Update-Database

# 3. Xóa migration
Remove-Migration

# 4. Xem migrations
Get-Migration

# 5. Script migration
Script-Migration
```

#### Migration Files

Khi chạy `add-migration`, EF Core tạo ra 3 file:

```
Migrations/
├── 20240115073000_InitialCreate.cs          # Main migration
├── 20240115073000_InitialCreate.Designer.cs # Migration snapshot
└── ApplicationDbContextModelSnapshot.cs     # Current model snapshot
```

**File migration (20240115073000_InitialCreate.cs):**

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Age = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Students_Email",
            table: "Students",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Students");
    }
}
```

#### Các tình huống với Migration

**1. Thêm column mới**

```csharp
// Entity Class
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string PhoneNumber { get; set; } // NEW
    public bool IsActive { get; set; }
}

// Migration
dotnet ef migrations add AddPhoneNumber
dotnet ef database update
```

**2. Thay đổi Data Type**

```csharp
// Entity
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; } // từ string sang int
}

// Migration
dotnet ef migrations add ChangeAgeToInt
dotnet ef database update
```

**3. Thêm Relationship**

```csharp
// Entity
public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? InstructorId { get; set; } // NEW Foreign Key
    public Instructor Instructor { get; set; } // NEW Navigation
}

public class Instructor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Course> Courses { get; set; }
}

// Migration
dotnet ef migrations add AddInstructorRelation
dotnet ef database update
```

#### Lỗi thường gặp với Migration

| Lỗi | Nguyên nhân | Cách khắc phục |
|-----|-------------|----------------|
| **"Invalid column name"** | Quên chạy migration | Chạy `dotnet ef database update` |
| **"Cannot add new entity"** | Chưa có DbSet trong DbContext | Thêm `public DbSet<Entity> Entity { get; set; }` |
| **"Migration already exists"** | Migration đã tồn tại | Dùng tên khác hoặc remove |
| **"Pending model changes"** | Model thay đổi chưa có migration | Chạy `dotnet ef migrations add` |
| **"Cannot drop column"** | Có dữ liệu phụ thuộc | Xóa dữ liệu hoặc thêm logic migration |

---

## 🛠️ 2. Thực hành

### 2.1. Demo: Code First với EF Core

#### Step 1: Tạo Project

```bash
dotnet new webapi -n ProductApi
cd ProductApi
```

#### Step 2: Cài đặt EF Core Packages

```bash
# Core EF Core package
dotnet add package Microsoft.EntityFrameworkCore

# SQL Server provider
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# EF Tools (cho migration)
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Design-time (cần cho migration)
dotnet add package Microsoft.EntityFrameworkCore.Design
```

#### Step 3: Tạo Entity Models

```csharp
// Models/Product.cs
namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

// Models/Category.cs
namespace ProductApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

// Models/Order.cs
namespace ProductApi.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public List<OrderItem> OrderItems { get; set; } = new();
}

// Models/OrderItem.cs
namespace ProductApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
    
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
```

#### Step 4: Tạo DbContext

```csharp
// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(p => p.Description)
                .HasMaxLength(1000);
            entity.Property(p => p.Price)
                .HasPrecision(18, 2);
            entity.Property(p => p.Category)
                .HasMaxLength(50);
            entity.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");
        });

        // Cấu hình Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");
        });

        // Cấu hình Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(o => o.CustomerEmail)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
            entity.Property(o => o.Status)
                .IsRequired()
                .HasMaxLength(20);
        });

        // Cấu hình OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
            
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
            new Category { Id = 2, Name = "Books", Description = "Reading materials" },
            new Category { Id = 3, Name = "Clothing", Description = "Fashion items" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Laptop Dell XPS", 
                Description = "High performance laptop",
                Price = 1500.00m,
                Stock = 10,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Product 
            { 
                Id = 2, 
                Name = "iPhone 15 Pro", 
                Description = "Latest smartphone",
                Price = 1200.00m,
                Stock = 5,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }
}
```

#### Step 5: Cấu hình appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

#### Step 6: Đăng ký DbContext trong Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository/Service
// builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations automatically on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Automatically apply migrations
}

app.Run();
```

#### Step 7: Chạy Migration

```bash
# 1. Thêm migration
dotnet ef migrations add InitialCreate

# 2. Cập nhật database
dotnet ef database update
```

#### Step 8: Kiểm tra Database

```sql
-- Mở SSMS và kiểm tra
USE ProductDb;
SELECT * FROM Categories;
SELECT * FROM Products;
SELECT * FROM Orders;
SELECT * FROM OrderItems;

-- Kiểm tra relationships
SELECT 
    p.Name AS ProductName,
    c.Name AS CategoryName
FROM Products p
LEFT JOIN Categories c ON p.Category = c.Name;
```

---

### 2.2. Demo: Adding New Column

#### Step 1: Sửa Entity

```csharp
// Models/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    
    // NEW COLUMN
    public decimal? DiscountRate { get; set; } // Thêm DiscountRate
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

#### Step 2: Tạo Migration mới

```bash
# Thêm migration
dotnet ef migrations add AddDiscountRateToProduct

# Cập nhật database
dotnet ef database update
```

#### Step 3: Kiểm tra Migration File

```csharp
// 20240115080000_AddDiscountRateToProduct.cs
public partial class AddDiscountRateToProduct : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "DiscountRate",
            table: "Products",
            type: "decimal(18,2)",
            nullable: true,
            precision: 18,
            scale: 2);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DiscountRate",
            table: "Products");
    }
}
```

---

### 2.3. Demo: Rename Column

#### Step 1: Sửa Entity

```csharp
// Models/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal? DiscountRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // RENAME FROM Category TO CategoryName
    // Category → CategoryName
}
```

#### Step 2: Sử dụng Migration Builder

```csharp
// Migration
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Category",
        table: "Products",
        newName: "CategoryName");
}
```

---

### 2.4. Demo: Add Relationship

#### Step 1: Thêm Entity

```csharp
// Models/Supplier.cs
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

// Models/Product.cs (update)
public class Product
{
    // ... existing properties ...
    public int? SupplierId { get; set; } // Foreign Key
    public Supplier Supplier { get; set; } // Navigation Property
}
```

#### Step 2: Cấu hình Relationship

```csharp
// Data/ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing configurations ...
    
    modelBuilder.Entity<Product>()
        .HasOne(p => p.Supplier)
        .WithMany(s => s.Products)
        .HasForeignKey(p => p.SupplierId)
        .OnDelete(DeleteBehavior.SetNull);
}
```

#### Step 3: Migration

```bash
dotnet ef migrations add AddSupplierEntity
dotnet ef database update
```

---

## 📝 2.5. Bài thực hành

### Bài tập 1: Tạo Database Structure

**Yêu cầu:** Khởi tạo cấu trúc Database bằng EF Core Code First.

**Chi tiết:**
1. Cài đặt các gói NuGet cần thiết cho EF Core (SqlServer, Tools)
2. Tạo cấu trúc thư mục chứa Data/DbContext và cấu hình chuỗi kết nối đến SQL Server trong `appsettings.json`
3. Tạo 2 Entity: `Category` (Id, Name) và `Product` (Id, Name, Price, CategoryId)
4. Khai báo chúng vào DbContext
5. Sử dụng lệnh tạo Migration (tên: "Init") và Update-Database để sinh CSDL. Kiểm tra bằng SSMS

**Đáp án:**

```csharp
// Models/Category.cs
namespace StoreApi.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public List<Product> Products { get; set; } = new();
}

// Models/Product.cs
namespace StoreApi.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Precision(18, 2)]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; } // Foreign Key
    
    public Category Category { get; set; } = null!; // Navigation Property
}

// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;

namespace StoreApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(p => p.Price)
                .HasPrecision(18, 2);
            
            // Relationship
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StoreDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Migration Commands
// dotnet ef migrations add Init
// dotnet ef database update
```

**Kiểm tra bằng SQL:**
```sql
USE StoreDb;
SELECT * FROM Categories;
SELECT * FROM Products;

-- Kiểm tra foreign key constraint
SELECT 
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    fk.name AS FKName
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('Products');
```

---

### Bài tập 2: Thêm Column

**Yêu cầu:** Cập nhật Entity `Product`, thêm trường `Description` (có thể null). Sinh một Migration mới (tên: "AddDescription") và cập nhật Database mà không làm mất dữ liệu cũ.

**Đáp án:**

```csharp
// Models/Product.cs (Updated)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // NEW - Nullable Description
    public string? Description { get; set; } // Nên là string? cho nullable
    
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

// Migration Commands
// dotnet ef migrations add AddDescription
// dotnet ef database update

// Generated Migration
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Description",
        table: "Products",
        type: "nvarchar(1000)",
        maxLength: 1000,
        nullable: true); // nullable: true vì là string? trong C#
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "Description",
        table: "Products");
}

// Kiểm tra thêm dữ liệu
// INSERT INTO Products (Name, Price, CategoryId, Description) 
// VALUES ('Test Product', 100, 1, 'This is a test description');
```

---

### Bài tập 3: Add Relationship (Many-to-Many)

**Yêu cầu:** Tạo mối quan hệ Many-to-Many giữa `Product` và `Order` thông qua bảng `OrderItem`.

**Đáp án:**

```csharp
// Models/Order.cs
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}

// Models/OrderItem.cs (Junction Table)
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

// Models/Product.cs (Updated)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public List<OrderItem> OrderItems { get; set; } = new();
}

// Data/ApplicationDbContext.cs (Configure Many-to-Many)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing configurations ...

    // Order configuration
    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(o => o.Id);
        entity.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(100);
    });

    // OrderItem configuration (Junction Table)
    modelBuilder.Entity<OrderItem>(entity =>
    {
        entity.HasKey(oi => oi.Id);
        entity.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);
        
        entity.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        entity.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    });
}

// Migration
// dotnet ef migrations add AddOrderAndOrderItems
// dotnet ef database update
```

---

### Bài tập 4: Seed Data

**Yêu cầu:** Thêm seed data cho Category và Product.

**Đáp án:**

```csharp
// Data/ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ... existing configurations ...

    // Seed Categories
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "Electronics" },
        new Category { Id = 2, Name = "Books" },
        new Category { Id = 3, Name = "Clothing" },
        new Category { Id = 4, Name = "Food" }
    );

    // Seed Products
    modelBuilder.Entity<Product>().HasData(
        new Product { Id = 1, Name = "Laptop", Price = 1500, CategoryId = 1, Description = "High performance laptop" },
        new Product { Id = 2, Name = "Smartphone", Price = 1200, CategoryId = 1, Description = "Latest smartphone" },
        new Product { Id = 3, Name = "C# Book", Price = 50, CategoryId = 2, Description = "C# programming guide" },
        new Product { Id = 4, Name = "T-Shirt", Price = 30, CategoryId = 3, Description = "Cotton t-shirt" }
    );
}

// Migration with seed data
// dotnet ef migrations add AddSeedData
// dotnet ef database update

// Kiểm tra
// SELECT * FROM Categories;
// SELECT * FROM Products;
```

---

### Bài tập 5: Apply Migrations on Startup

**Yêu cầu:** Tự động áp dụng migrations khi khởi động ứng dụng.

**Đáp án:**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... services configuration ...

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Only in Development environment
    if (app.Environment.IsDevelopment())
    {
        // 1. Ensure database is created and migrations are applied
        dbContext.Database.Migrate();
        
        // 2. Or ensure created (without migrations)
        // dbContext.Database.EnsureCreated();
    }
    else
    {
        // For production, use safer approach
        // dbContext.Database.Migrate();
    }
}

// ... rest of app configuration ...
app.Run();
```

---

## 📊 Tổng kết Buổi 5

| Chủ đề            | Đã học                                              |
| ----------------- | --------------------------------------------------- |
| **ORM**           | Object-Relational Mapping, ánh xạ Object ↔ Database |
| **EF Core**       | ORM của Microsoft, Code First, Database First       |
| **DbContext**     | Lớp trung tâm, session với database                 |
| **DbSet**         | Đại diện cho bảng trong database                    |
| **Migration**     | Version control cho database                        |
| **Relationships** | One-to-Many, Many-to-Many configuration             |

**Key Takeaways:**
1. ✅ **ORM** giúp lập trình viên không cần viết SQL thủ công
2. ✅ **DbContext** là lớp trung tâm quản lý kết nối và tracking
3. ✅ **Migration** là version control cho database
4. ✅ **Code First**: Viết C# code → Sinh database
5. ✅ Quên chạy `Update-Database` là lỗi thường gặp nhất
6. ✅ DbContext có vòng đời **Scoped** trong Web API

---

✅ **Hoàn thành nội dung Buổi 5.**


```
Lập trình .NET (ASP.NET Core Web API)
```
# Buổi 6: CRUD với Entity Framework Core, LINQ và quan hệ dữ liệu

**Khoa Công nghệ thông tin – Lập trình .NET**

> **Mục tiêu buổi học:**
> - Hiểu và sử dụng được LINQ trong EF Core.
> - Thực hiện thành thạo CRUD với Entity Framework Core.
> - Cấu hình quan hệ dữ liệu 1-N giữa `Class` và `Student`.
> - Biết dùng `Include` để tải dữ liệu liên quan.
> - Nhận diện và xử lý lỗi Object Cycle bằng DTO.
> - Hoàn thiện các API CRUD cơ bản và API lấy sinh viên theo lớp.

---

## Mục lục

1. [Lý thuyết nền tảng](#1-lý-thuyết-nền-tảng)
2. [Lý thuyết chính](#2-lý-thuyết-chính)
   - [2.1. LINQ](#21-linq)
   - [2.2. CRUD](#22-crud)
   - [2.3. Quan hệ dữ liệu](#23-quan-hệ-dữ-liệu)
   - [2.4. Include](#24-include)
3. [Ví dụ nền tảng](#3-ví-dụ-nền-tảng)
4. [Thực hành / Lab cơ bản](#4-thực-hành--lab-cơ-bản)
5. [Bài tập mở rộng](#5-bài-tập-mở-rộng)

---

## 1. Lý thuyết nền tảng

### 1.1. ASP.NET Core Web API là gì?

**ASP.NET Core Web API** là một framework dùng để xây dựng các dịch vụ HTTP/RESTful API trên nền tảng .NET. API nhận request từ client như Postman, Swagger, ứng dụng web, mobile, sau đó trả về dữ liệu thường ở định dạng JSON.

Một số khái niệm nền tảng:

| Khái niệm | Vai trò |
|---|---|
| **Controller** | Tiếp nhận request, xử lý logic và trả về response. |
| **Routing** | Ánh xạ URL tới controller/action tương ứng. |
| **HTTP Method** | `GET`, `POST`, `PUT`, `DELETE` tương ứng đọc, thêm, sửa, xóa. |
| **Status Code** | `200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`. |
| **DTO** | Data Transfer Object – đối tượng dùng để truyền dữ liệu ra/vào API. |

### 1.2. Entity Framework Core là gì?

**Entity Framework Core (EF Core)** là ORM – Object Relational Mapping của .NET. Nó giúp lập trình viên thao tác với database thông qua đối tượng C# thay vì viết SQL thủ công.

Các thành phần chính:

| Thành phần | Ý nghĩa |
|---|---|
| **Entity** | Lớp C# ánh xạ tới bảng trong database, ví dụ `Student`, `Class`. |
| **DbContext** | Đối tượng quản lý phiên làm việc với database. |
| **DbSet<T>** | Đại diện cho một bảng, ví dụ `DbSet<Student> Students`. |
| **Migration** | Công cụ sinh và cập nhật schema database từ model. |
| **Change Tracking** | EF Core theo dõi thay đổi của entity để sinh `UPDATE`, `DELETE`. |
| **LINQ** | Cú pháp truy vấn dữ liệu trong C#, được EF Core dịch sang SQL. |

Quy trình cơ bản:

```text
Model/Entity → DbContext → Migration → Database → Controller/API
```

---

## 2. Lý thuyết chính

## 2.1. LINQ

### 2.1.1. LINQ là gì?

**Language-Integrated Query (LINQ)** cung cấp cú pháp truy vấn dữ liệu trực tiếp trong C#.

Trong EF Core, LINQ được biên dịch tự động thành các câu lệnh SQL như `SELECT`, `WHERE`, `ORDER BY` để chạy dưới CSDL.

Hỗ trợ **Methods Syntax** – dùng nhiều nhất:

```csharp
.Where()
.Select()
.OrderBy()
.FirstOrDefault()
.ToListAsync()
```

Ví dụ:

```csharp
var adults = await _context.Students
    .Where(s => s.Age >= 18)
    .OrderBy(s => s.Name)
    .Select(s => new { s.Id, s.Name, s.Age })
    .ToListAsync();
```

### 2.1.2. Query Syntax và Method Syntax

**Method Syntax:**

```csharp
var students = await _context.Students
    .Where(s => s.Age >= 18)
    .OrderBy(s => s.Name)
    .ToListAsync();
```

**Query Syntax:**

```csharp
var students = await (
    from s in _context.Students
    where s.Age >= 18
    orderby s.Name
    select s
).ToListAsync();
```

> **Ghi nhớ:** Trong EF Core, Method Syntax thường được dùng phổ biến hơn vì dễ kết hợp với `Include`, `Select`, `FirstOrDefaultAsync`.

### 2.1.3. IQueryable và IEnumerable

| Tiêu chí | `IQueryable<T>` | `IEnumerable<T>` |
|---|---|---|
| Nơi thực thi | Database | Trong bộ nhớ |
| Dịch sang SQL | Có | Không |
| Hiệu năng | Tốt nếu truy vấn đúng | Dễ tải thừa dữ liệu |
| Dùng với EF Core | Nên dùng trước `ToListAsync()` | Chỉ dùng sau khi đã lấy dữ liệu |

Ví dụ nên làm:

```csharp
var result = await _context.Students
    .Where(s => s.Age >= 18)
    .ToListAsync();
```

Ví dụ không nên làm:

```csharp
var all = await _context.Students.ToListAsync();
var result = all.Where(s => s.Age >= 18).ToList();
```

Cách thứ hai tải toàn bộ sinh viên lên bộ nhớ rồi mới lọc.

### 2.1.4. Một số phương thức LINQ thường dùng

| Phương thức | Công dụng | Ví dụ |
|---|---|---|
| `Where` | Lọc dữ liệu | `.Where(s => s.Age >= 18)` |
| `Select` | Chọn cột / ánh xạ DTO | `.Select(s => new StudentDto { ... })` |
| `OrderBy` | Sắp xếp tăng | `.OrderBy(s => s.Name)` |
| `OrderByDescending` | Sắp xếp giảm | `.OrderByDescending(s => s.Age)` |
| `FirstOrDefaultAsync` | Lấy phần tử đầu hoặc null | `.FirstOrDefaultAsync(s => s.Id == id)` |
| `FindAsync` | Tìm theo khóa chính | `_context.Students.FindAsync(id)` |
| `ToListAsync` | Lấy danh sách bất đồng bộ | `.ToListAsync()` |
| `Include` | Tải dữ liệu liên quan | `.Include(s => s.Class)` |
| `AnyAsync` | Kiểm tra tồn tại | `.AnyAsync(s => s.Id == id)` |
| `CountAsync` | Đếm số lượng | `.CountAsync()` |

---

## 2.2. CRUD

CRUD là viết tắt của:

- **C**reate – Thêm mới
- **R**ead – Đọc dữ liệu
- **U**pdate – Cập nhật
- **D**elete – Xóa

### 2.2.1. Create – Thêm mới

Sử dụng `Add()` và bắt buộc phải gọi `SaveChanges()` hoặc `SaveChangesAsync()` để đẩy câu lệnh `INSERT` xuống CSDL.

```csharp
var student = new Student
{
    Name = "Nguyen Van A",
    Age = 20
};

_context.Students.Add(student);
await _context.SaveChangesAsync(); // Lưu xuống SQL
```

### 2.2.2. Read – Đọc dữ liệu

Sử dụng LINQ để truy vấn, EF Core sinh câu lệnh `SELECT`.

```csharp
// Lấy toàn bộ
var allStudents = await _context.Students.ToListAsync();

// Lấy theo ID
var student = await _context.Students.FindAsync(id);

// Lọc dữ liệu có điều kiện (WHERE)
var adults = await _context.Students
    .Where(s => s.Age >= 18)
    .ToListAsync();
```

### 2.2.3. Update – Cập nhật

Nguyên tắc:

1. Truy vấn dữ liệu ra.
2. Thay đổi thuộc tính.
3. Lưu lại.

EF Core có cơ chế **Change Tracking** để sinh câu lệnh `UPDATE`.

```csharp
var student = await _context.Students.FindAsync(id);

if (student != null)
{
    student.Age = 21; // EF Core tự nhận biết trường Age bị thay đổi
    await _context.SaveChangesAsync();
}
```

### 2.2.4. Delete – Xóa

Nguyên tắc:

1. Truy vấn dữ liệu ra.
2. Gọi hàm `Remove()`.
3. Lưu lại.

```csharp
var student = await _context.Students.FindAsync(id);

if (student != null)
{
    _context.Students.Remove(student);
    await _context.SaveChangesAsync();
}
```

### 2.2.5. Best Practice: Luôn dùng async/await với DB

- Các thao tác I/O như đọc/ghi Database rất chậm so với tốc độ CPU.
- Luôn sử dụng các hàm bất đồng bộ như `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync` để tránh block luồng xử lý của Web Server.
- Điều này giúp ứng dụng chịu tải tốt hơn.

```csharp
// Nên
var students = await _context.Students.ToListAsync();

// Hạn chế
var students = _context.Students.ToList();
```

> **Lưu ý:** Với truy vấn chỉ đọc, có thể dùng `AsNoTracking()` để tăng hiệu năng:
> ```csharp
> var students = await _context.Students
>     .AsNoTracking()
>     .ToListAsync();
> ```

---

## 2.3. Quan hệ dữ liệu

### 2.3.1. Quan hệ One-to-Many (1-N)

Mô hình phổ biến nhất: **One-to-Many (1-N)**.

Ví dụ: 1 Lớp (`Class`) có nhiều Sinh viên (`Student`).

```csharp
public class Class
{
    public int Id { get; set; }
    public string? ClassName { get; set; }

    // Navigation Property
    public ICollection<Student> Students { get; set; } = new List<Student>();
}

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Foreign Key
    public int ClassId { get; set; }

    // Navigation Property
    public Class? Class { get; set; }
}
```

Sơ đồ:

```text
Class 1 ──── N Student
```

- `Class.Students`: danh sách sinh viên thuộc lớp.
- `Student.ClassId`: khóa ngoại trỏ tới `Class.Id`.
- `Student.Class`: navigation property trỏ tới lớp của sinh viên.

### 2.3.2. Cấu hình quan hệ bằng Fluent API

Trong `DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Class>()
        .HasMany(c => c.Students)
        .WithOne(s => s.Class)
        .HasForeignKey(s => s.ClassId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

---

## 2.4. Include

### 2.4.1. Eager Loading với Include

Mặc định EF Core **KHÔNG tải dữ liệu bảng liên quan**. Cần `Include()` để JOIN.

Ví dụ không dùng `Include`:

```csharp
// Chỉ lấy thông tin Student, Class sẽ bị null
var students = await _context.Students.ToListAsync();
```

Kết quả JSON:

```json
{
  "id": 1,
  "name": "Nguyen Van A",
  "age": 20,
  "classId": 5,
  "class": null
}
```

Ví dụ dùng `Include`:

```csharp
// Lấy Student kèm theo thông tin của Class
var studentsWithClass = await _context.Students
    .Include(s => s.Class)
    .ToListAsync();
```

Kết quả JSON:

```json
{
  "id": 1,
  "name": "Nguyen Van A",
  "age": 20,
  "classId": 5,
  "class": {
    "id": 5,
    "className": "CNTT-K6A"
  }
}
```

Tương đương SQL:

```sql
SELECT s.*, c.*
FROM Students s
INNER JOIN Classes c ON s.ClassId = c.Id;
```

### 2.4.2. Lỗi thường gặp: Object Cycle

**Vấn đề:**

Khi sử dụng `Include`, đối tượng `Class` chứa danh sách `Students`, mỗi `Student` lại trỏ ngược về `Class`… gây ra vòng lặp vô tận khi serialize ra JSON.

```text
Class → Students[] → Student.Class → Students[] → ...
```

**Cách khắc phục:**

- Dùng **DTO** để cắt vòng lặp.
- Hoặc cấu hình bỏ qua vòng lặp tham chiếu trong thư viện JSON.
- Khuyến khích dùng **DTO** thay vì cấu hình JSON toàn cục.

Ví dụ DTO:

```csharp
public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int ClassId { get; set; }
    public string? ClassName { get; set; }
}
```

### 2.4.3. Theo dõi truy vấn SQL

Trong quá trình phát triển, hãy luôn quan sát cửa sổ Console — EF Core in ra các câu lệnh SQL thật mà nó sinh ra.

- **Kiểm soát hiệu suất:** Phát hiện ngay khi EF Core sinh ra câu SQL quá phức tạp.
- **Tránh lỗi N+1 Query:** N+1 xảy ra khi load dữ liệu liên quan trong vòng lặp — 1 query cha + N query con.
- **Debug nhanh:** Biết chính xác câu SQL sinh ra là gì — copy sang SQL Server Management Studio để test/tune.

Cách bật logging trong `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

Hoặc dùng `LogTo()` trong `OnConfiguring`:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
}
```

---

## 3. Ví dụ nền tảng

### 3.1. Cài đặt package

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### 3.2. Entity models

```csharp
public class Class
{
    public int Id { get; set; }
    public string? ClassName { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }

    public int ClassId { get; set; }
    public Class? Class { get; set; }
}
```

### 3.3. DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Class> Classes => Set<Class>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>()
            .HasMany(c => c.Students)
            .WithOne(s => s.Class)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 3.4. Connection string

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SchoolDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3.5. Đăng ký DbContext trong Program.cs

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
```

### 3.6. Migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3.7. DTO

```csharp
public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int ClassId { get; set; }
    public string? ClassName { get; set; }
}

public class StudentCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int ClassId { get; set; }
}

public class StudentUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int ClassId { get; set; }
}

public class ClassWithStudentsDto
{
    public int Id { get; set; }
    public string? ClassName { get; set; }
    public List<StudentDto> Students { get; set; } = new();
}
```

### 3.8. Controller Student

```csharp
[Route("api/[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/students
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents()
    {
        var students = await _context.Students
            .Include(s => s.Class)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                Age = s.Age,
                ClassId = s.ClassId,
                ClassName = s.Class != null ? s.Class.ClassName : null
            })
            .ToListAsync();

        return Ok(students);
    }

    // GET: /api/students/5
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        var student = await _context.Students
            .Include(s => s.Class)
            .Where(s => s.Id == id)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                Age = s.Age,
                ClassId = s.ClassId,
                ClassName = s.Class != null ? s.Class.ClassName : null
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    // POST: /api/students
    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent(StudentCreateDto dto)
    {
        var student = new Student
        {
            Name = dto.Name,
            Age = dto.Age,
            ClassId = dto.ClassId
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var result = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Age = student.Age,
            ClassId = student.ClassId
        };

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, result);
    }

    // PUT: /api/students/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, StudentUpdateDto dto)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound();
        }

        student.Name = dto.Name;
        student.Age = dto.Age;
        student.ClassId = dto.ClassId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: /api/students/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound();
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

### 3.9. Controller Class – lấy sinh viên theo lớp

```csharp
[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClassesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/classes/5/students
    [HttpGet("{id}/students")]
    public async Task<ActionResult<ClassWithStudentsDto>> GetClassStudents(int id)
    {
        var classDto = await _context.Classes
            .Where(c => c.Id == id)
            .Select(c => new ClassWithStudentsDto
            {
                Id = c.Id,
                ClassName = c.ClassName,
                Students = c.Students.Select(s => new StudentDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Age = s.Age,
                    ClassId = s.ClassId,
                    ClassName = c.ClassName
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (classDto == null)
        {
            return NotFound();
        }

        return Ok(classDto);
    }
}
```

---

## 4. Thực hành / Lab cơ bản

### Lab 1: CRUD Student

1. Tạo project ASP.NET Core Web API.
2. Cài các package EF Core SQL Server.
3. Tạo entity `Student`.
4. Tạo `AppDbContext`.
5. Cấu hình connection string và đăng ký DbContext.
6. Chạy migration để tạo database.
7. Viết controller `StudentsController` với các endpoint:
   - `GET /api/students`
   - `GET /api/students/{id}`
   - `POST /api/students`
   - `PUT /api/students/{id}`
   - `DELETE /api/students/{id}`
8. Kiểm thử bằng Swagger hoặc Postman.

### Lab 2: Quan hệ Student – Class

1. Thêm entity `Class`.
2. Cấu hình quan hệ 1-N giữa `Class` và `Student`.
3. Chạy migration:

```bash
dotnet ef migrations add AddClassRelation
dotnet ef database update
```

4. Thêm dữ liệu mẫu cho `Class`.
5. Kiểm tra `Student.ClassId` trỏ đúng tới `Class.Id`.

### Lab 3: Include và không Include

1. Viết API `GET /api/students` không dùng `Include`.
2. Quan sát JSON: `class` bị `null`.
3. Sửa API dùng `Include(s => s.Class)`.
4. Quan sát JSON: có đầy đủ thông tin lớp.
5. So sánh câu SQL sinh ra trong Console.

### Lab 4: DTO và Object Cycle

1. Tạo `StudentDto`, `ClassWithStudentsDto`.
2. Chuyển các API sang trả về DTO.
3. Kiểm tra không còn lỗi vòng lặp JSON.
4. Đảm bảo API gọn dữ liệu, không trả thừa các navigation property.

### Lab 5: API lấy sinh viên theo lớp

1. Viết endpoint:

```http
GET /api/classes/{id}/students
```

2. Trả về thông tin lớp và danh sách sinh viên thuộc lớp đó.
3. Dùng DTO để tránh Object Cycle.
4. Kiểm thử với lớp có nhiều sinh viên.

---

## 5. Bài tập mở rộng

1. Thêm chức năng tìm kiếm sinh viên theo tên:
   ```http
   GET /api/students?name=Nguyen
   ```

2. Thêm phân trang cho danh sách sinh viên:
   ```http
   GET /api/students?page=1&pageSize=10
   ```

3. Thêm entity `Subject` và cấu hình quan hệ nhiều-nhiều với `Student`.

4. Sử dụng `AsNoTracking()` cho các API chỉ đọc.

5. Bật logging EF Core để kiểm tra tất cả câu SQL sinh ra trong các API.

6. Viết unit test hoặc integration test cho API `GET /api/students/{id}`.

---

## Tóm tắt cuối buổi

- **LINQ** giúp truy vấn dữ liệu trong C#, EF Core dịch sang SQL.
- **CRUD** với EF Core cần `Add`, `FindAsync`, `Remove`, `SaveChangesAsync`.
- Luôn ưu tiên **async/await** khi thao tác database.
- Quan hệ **1-N** dùng navigation property và foreign key.
- **Include** dùng để tải dữ liệu liên quan, tránh null ngoài ý muốn.
- **DTO** là cách chuẩn để tránh Object Cycle và kiểm soát dữ liệu trả về API.
- Luôn theo dõi **SQL logging** để tối ưu hiệu năng và tránh N+1 Query.

