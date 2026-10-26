---
role: acad
topic: it
---
# Buổi 11: Pagination, Filtering, Searching, Sorting

## Mục lục

```text
Buổi 11: Pagination, Filtering, Searching, Sorting
│
├── 1. Lý thuyết nền tảng
│   ├── 1.1. Vấn đề khi Load Data
│   └── 1.2. Tổng quan 4 kỹ thuật
│
├── 2. Lý thuyết chính
│   ├── 2.1. Pagination (Phân trang)
│   ├── 2.2. Filtering (Lọc)
│   ├── 2.3. Searching (Tìm kiếm)
│   ├── 2.4. Sorting (Sắp xếp)
│   ├── 2.5. Gom Query Parameter thành Object
│   └── 2.6. Trả về Metadata Phân trang
│
├── 3. Ví dụ nền tảng
│   ├── 3.1. Pagination cơ bản
│   ├── 3.2. Filtering cơ bản
│   ├── 3.3. Searching cơ bản
│   └── 3.4. Sorting cơ bản
│
├── 4. Thực hành / Lab cơ bản
│   └── 4.1. API phân trang sản phẩm
│
├── 5. Bài tập mở rộng
│   └── 5.1. Bài thực hành (3 bài, giải chi tiết)
│
├── 6. Bảng thuật ngữ
│
└── 7. Cheatsheet
```

## 1. Lý thuyết nền tảng

### 1.1. Vấn đề khi Load Data

**Vì sao không thể cứ `ToList()` tất cả mọi thứ?**

Khi một bảng CSDL có 1.000.000 dòng dữ liệu:

- Gọi `_context.Products.ToList()` sẽ lấy toàn bộ về.
- Gây quá tải bộ nhớ (RAM) của Server.
- Chậm thời gian truyền tải qua HTTP (Response rất to).
- Client (Mobile/Web) bị giật lag khi cố gắng render.

> **Giải pháp:** Chỉ trả về dữ liệu Client thực sự cần bằng cách áp dụng **Pagination, Filtering, Searching, Sorting**.

> **Ghi chú bổ sung:** `ToList()` kích hoạt **eager loading** — EF Core dịch toàn bộ query thành SQL, chạy xuống DB, rồi materialize tất cả kết quả vào RAM. Với bảng lớn, điều này còn kéo theo việc **tracking** từng entity trong `ChangeTracker` (tốn thêm bộ nhớ). Nếu chỉ đọc, nên dùng `.AsNoTracking()` để tắt tracking, giảm 30-50% bộ nhớ và tăng tốc độ đáng kể.

### 1.2. Tổng quan 4 kỹ thuật

| Kỹ thuật | Mục đích | Ví dụ tham số |
|---|---|---|
| **Pagination** | Giới hạn số bản ghi trả về mỗi lần | `pageNumber=2&pageSize=20` |
| **Filtering** | Lọc chính xác theo thuộc tính | `categoryId=3&minPrice=100` |
| **Searching** | Tìm kiếm chuỗi con trong text | `keyword=iphone` |
| **Sorting** | Sắp xếp theo trường và chiều | `sortBy=price&desc=true` |

**Thứ tự áp dụng chuẩn:** `Filtering → Searching → Sorting → Pagination`

> **Lý do thứ tự:** Phải lọc/tìm/sắp xếp **trước** khi phân trang, vì `Skip/Take` cần biết tập dữ liệu cuối cùng đã được sắp xếp như thế nào. Nếu phân trang trước rồi mới sort, mỗi trang sẽ hiển thị dữ liệu sai thứ tự. `CountAsync()` phải chạy **sau** Filter+Search (nhưng **trước** Skip/Take) để đếm đúng tổng bản ghi khớp điều kiện.

## 2. Lý thuyết chính

### 2.1. Pagination (Phân trang)

**Định nghĩa:** Chia nhỏ tổng số bản ghi thành nhiều "trang", mỗi trang chứa một số lượng bản ghi nhất định.

**Hai thông số quan trọng:**

1. `PageNumber`: Số trang hiện tại (mặc định = 1).
2. `PageSize`: Số phần tử trên một trang (mặc định = 10 hoặc 20).

**Công thức trong LINQ:**

- Sử dụng `Skip()` và `Take()`.
- `Skip = (PageNumber - 1) * PageSize`

> **Ghi nhớ:** Phải có cơ chế `OrderBy` (sắp xếp mặc định) trước khi dùng `Skip/Take` để đảm bảo thứ tự phân trang chính xác trên SQL Server.

> **Ghi chú bổ sung:** Trong SQL Server, `OFFSET ... FETCH` (mà EF Core sinh ra từ `Skip/Take`) **bắt buộc** phải đi kèm `ORDER BY`. Nếu không có, EF Core sẽ ném lỗi runtime `InvalidOperationException`. Ngoài ra cần **clamp** `PageSize` (ví dụ `Math.Clamp(pageSize, 1, 50)`) để tránh client gửi `pageSize=1000000` làm sập server — đây là một dạng tấn công DoS đơn giản.

### 2.2. Filtering (Lọc)

**Định nghĩa:** Lọc dữ liệu dựa trên các thuộc tính cụ thể, chính xác.

**Ví dụ:** Lọc sản phẩm theo `Category`, lọc theo giá tiền.

> **Lưu ý:** Ví dụ riêng lẻ chỉ minh họa điều kiện Filter — chưa kết hợp phân trang. Phần Tích hợp sẽ gộp tất cả lại.

> **Best Practice:** Dùng `IQueryable` để nối (chain) các điều kiện `Where` lại với nhau. EF Core sẽ biên dịch thành một câu SQL duy nhất khi gọi `ToListAsync()`.

> **Ghi chú bổ sung:** Kỹ thuật này gọi là **deferred execution** — query chưa thực thi cho đến khi có toán tử terminal (`ToList`, `Count`, `First`...). Nhờ vậy có thể build query động: kiểm tra `if (param.HasValue)` rồi mới `.Where()`, tránh phải viết nhiều biến thể câu SQL. Nếu dùng `IEnumerable` thay vì `IQueryable`, toàn bộ dữ liệu sẽ được load vào RAM **trước** khi filter → mất hoàn toàn lợi ích.

### 2.3. Searching (Tìm kiếm)

**Định nghĩa:** Cho phép người dùng nhập chuỗi văn bản, tìm kiếm các bản ghi có chứa chuỗi đó.

**Đặc điểm:**

- Thường dùng với điều kiện `Contains` (tương đương `LIKE '%text%'` trong SQL): `WHERE Name LIKE '%keyword%'`.
- Thường áp dụng trên các trường String (Tên, Tiêu đề, Mô tả).

> **Ghi chú bổ sung:** `Contains` trong EF Core dịch thành `LIKE '%keyword%'` — **không dùng được index** (trừ khi có full-text index). Với bảng lớn, nên cân nhắc:
> - **Full-text search** của SQL Server (`CONTAINS`, `FREETEXT`).
> - **Elasticsearch / Meilisearch** cho ứng dụng tìm kiếm nặng.
> - **Case-insensitive**: SQL Server mặc định đã CI, nhưng nếu collation là CS, phải dùng `EF.Functions.Like(p.Name, $"%{keyword}%")` và cẩn thận escape `%` `_` `[` trong keyword để tránh SQL injection kiểu wildcard.

### 2.4. Sorting (Sắp xếp)

**Định nghĩa:** Cho phép người dùng chỉ định trường muốn sắp xếp và chiều sắp xếp (Tăng dần — ASC, Giảm dần — DESC).

**Cách truyền tham số:** Có thể truyền chuỗi như `sortBy=price&sortOrder=desc`.

> **Ghi nhớ:** Chỉ cho phép sort trên danh sách field xác định (**whitelist**). Không bao giờ ghép chuỗi từ client trực tiếp vào câu truy vấn.

> **Ghi chú bổ sung:** Đây là lỗ hổng **SQL Injection qua ORDER BY** — kẻ tấn công có thể chèn `sortBy=Id; DROP TABLE Products--`. Whitelist (như `switch` expression trong ví dụ) là cách phòng thủ chuẩn. Nếu cần sort động linh hoạt trên nhiều cột, có thể dùng thư viện `System.Linq.Dynamic.Core` — nhưng vẫn phải validate tên cột trước khi truyền vào.

### 2.5. Gom Query Parameter thành Object

Khi có quá nhiều tham số (`pageNumber`, `pageSize`, `keyword`, `sortBy`...), nên gom chúng vào một Class (thường gọi là Query Parameters class).

```csharp
public class ProductQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
}
```

Controller sẽ rất gọn:

```csharp
public async Task<IActionResult> Get([FromQuery] ProductQueryParameters queryParams)
```

> **Ghi chú bổ sung:** `[FromQuery]` sẽ tự động **bind** từng property của class từ query string theo tên (case-insensitive). `int` mặc định = 0, `bool` = false, `string?` = null. Có thể đặt `[Range]`, `[StringLength]` trên property và bật `[ApiController]` để validation tự động trả 400 nếu sai.

### 2.6. Trả về Metadata Phân trang

Client không chỉ cần danh sách Data, mà cần cả số trang tổng để vẽ nút "Next/Prev". Ta nên thiết kế một Response chung:

```csharp
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
```

> **Ghi chú bổ sung:** `TotalPages` là **computed property** — không cần gán giá trị, tự tính từ `TotalRecords` và `PageSize`. Nhớ dùng `(double)` trước khi chia để tránh **integer division** (ví dụ `5/2 = 2` thay vì `2.5` → `Ceiling` ra `3`). Client thường cần thêm `HasNextPage`, `HasPreviousPage` — có thể tính tương tự.

## 3. Ví dụ nền tảng

### 3.1. Pagination cơ bản

```csharp
[HttpGet]
public async Task<IActionResult> GetProducts(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
{
    pageNumber = Math.Max(pageNumber, 1);
    pageSize = Math.Clamp(pageSize, 1, 50);

    var skip = (pageNumber - 1) * pageSize;

    var products = await _context.Products
        .OrderBy(p => p.Id)
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync();

    return Ok(products);
}
```

### 3.2. Filtering cơ bản

```csharp
[HttpGet("filter")]
public async Task<IActionResult> FilterProducts(
    [FromQuery] int? categoryId,
    [FromQuery] decimal? minPrice)
{
    var query = _context.Products.AsQueryable();

    if (categoryId.HasValue)
        query = query.Where(p => p.CategoryId == categoryId.Value);

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice.Value);

    var result = await query.ToListAsync();
    return Ok(result);
}
```

### 3.3. Searching cơ bản

```csharp
[HttpGet("search")]
public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
{
    var query = _context.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(keyword))
    {
        query = query.Where(p => p.Name.Contains(keyword)
                              || p.Description.Contains(keyword));
    }

    var result = await query.ToListAsync();
    return Ok(result);
}
```

### 3.4. Sorting cơ bản

```csharp
query = sortBy?.ToLower() switch
{
    "name" => isDescending
        ? query.OrderByDescending(p => p.Name)
        : query.OrderBy(p => p.Name),

    "price" => isDescending
        ? query.OrderByDescending(p => p.Price)
        : query.OrderBy(p => p.Price),

    _ => query.OrderBy(p => p.Id) // Mặc định sắp xếp theo Id
};
```

## 4. Thực hành / Lab cơ bản

### 4.1. API phân trang sản phẩm

**Quy trình:** `Filtering → Searching → Sorting → Pagination`

```csharp
[HttpGet]
public async Task<IActionResult> GetProducts(
    [FromQuery] ProductQueryParameters parameters)
{
    // 1. Chuẩn hóa tham số phân trang
    parameters.PageNumber = Math.Max(parameters.PageNumber, 1);
    parameters.PageSize = Math.Clamp(parameters.PageSize, 1, 50);

    var query = _context.Products.AsQueryable();

    // 2. Filtering
    if (parameters.CategoryId.HasValue)
        query = query.Where(x => x.CategoryId == parameters.CategoryId.Value);

    // 3. Searching
    if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        query = query.Where(x => x.Name.Contains(parameters.Keyword));

    // 4. Đếm tổng bản ghi (sau Filter + Search, trước Sort/Pagination)
    var totalRecords = await query.CountAsync();

    // 5. Sorting
    query = parameters.SortBy?.ToLower() switch
    {
        "name" => parameters.IsDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name),

        "price" => parameters.IsDescending
            ? query.OrderByDescending(x => x.Price)
            : query.OrderBy(x => x.Price),

        _ => query.OrderBy(x => x.Id)
    };

    // 6. Pagination
    var data = await query
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .ToListAsync();

    // 7. Trả về PagedResponse
    return Ok(new PagedResponse<Product>
    {
        Data = data,
        TotalRecords = totalRecords,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize
    });
}
```

## 5. Bài tập mở rộng

### 5.1. Bài thực hành

---

#### Bài 1: API Get danh sách Sản phẩm có Phân trang

**Yêu cầu:** Viết API GET danh sách Sản phẩm có Phân trang (`PageNumber`, `PageSize`). Trả về kiểu `PagedResponse` gồm `Data` và `TotalPages`.

**Giải thích chi tiết:**

- Dùng `PagedResponse<T>` để đóng gói dữ liệu + metadata.
- `TotalRecords` phải đếm **sau** filter/search (nếu có) để client biết chính xác tổng số bản ghi khớp điều kiện.
- `TotalPages` được tính tự động qua computed property, **không** cần gán.

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters parameters)
{
    parameters.PageNumber = Math.Max(parameters.PageNumber, 1);
    parameters.PageSize = Math.Clamp(parameters.PageSize, 1, 50);

    var query = _context.Products.AsQueryable();

    var totalRecords = await query.CountAsync();

    var data = await query
        .OrderBy(x => x.Id)
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .AsNoTracking()
        .ToListAsync();

    return Ok(new PagedResponse<Product>
    {
        Data = data,
        TotalRecords = totalRecords,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize
    });
}
```

**Kết quả trả về mẫu:**

```json
{
  "data": [
    { "id": 1, "name": "Sản phẩm 1", "price": 100 },
    { "id": 2, "name": "Sản phẩm 2", "price": 200 }
  ],
  "totalRecords": 42,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

#### Bài 2: Bổ sung Tìm kiếm và Lọc theo Giá

**Yêu cầu:** Bổ sung chức năng Tìm kiếm tên sản phẩm và Lọc theo Giá (từ giá `min` đến giá `max`) vào API ở bài tập 1.

**Giải thích chi tiết:**

- **Search:** dùng `Contains` trên `Name`, chỉ chạy khi keyword có giá trị (`!string.IsNullOrWhiteSpace`).
- **Filter giá:** dùng 2 tham số `MinPrice` và `MaxPrice`; mỗi điều kiện được `Where` độc lập và **chain** trên `IQueryable`.
- Lưu ý `decimal?` để phân biệt "không lọc" (null) và "lọc giá = 0".
- `CountAsync` phải chạy **sau** filter + search để đếm đúng.

**Cập nhật `ProductQueryParameters`:**

```csharp
public class ProductQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
}
```

**Controller:**

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters parameters)
{
    parameters.PageNumber = Math.Max(parameters.PageNumber, 1);
    parameters.PageSize = Math.Clamp(parameters.PageSize, 1, 50);

    var query = _context.Products.AsQueryable();

    // Filtering
    if (parameters.CategoryId.HasValue)
        query = query.Where(x => x.CategoryId == parameters.CategoryId.Value);

    if (parameters.MinPrice.HasValue)
        query = query.Where(x => x.Price >= parameters.MinPrice.Value);

    if (parameters.MaxPrice.HasValue)
        query = query.Where(x => x.Price <= parameters.MaxPrice.Value);

    // Searching
    if (!string.IsNullOrWhiteSpace(parameters.Keyword))
    {
        var keyword = parameters.Keyword.Trim();
        query = query.Where(x => x.Name.Contains(keyword));
    }

    var totalRecords = await query.CountAsync();

    var data = await query
        .OrderBy(x => x.Id)
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .AsNoTracking()
        .ToListAsync();

    return Ok(new PagedResponse<Product>
    {
        Data = data,
        TotalRecords = totalRecords,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize
    });
}
```

**Ví dụ gọi API:**

```
GET /api/products?keyword=iphone&minPrice=10000000&maxPrice=30000000&pageNumber=1&pageSize=20
```

---

#### Bài 3: Bổ sung Sorting động

**Yêu cầu:** Bổ sung chức năng Sorting động (cho phép client gửi lên thuộc tính cần sort: `Name` hoặc `Price`, và truyền biến `desc` để quyết định tăng hay giảm). Gom toàn bộ Params vào một đối tượng Query Class.

**Giải thích chi tiết:**

- **Whitelist field sort:** Chỉ chấp nhận `name`, `price`; fallback về `id`.
- **Không dùng reflection** để tránh SQL injection và mất IntelliSense.
- Đặt Sorting **sau** Count nhưng **trước** Skip/Take (thứ tự bắt buộc theo quy trình).
- Sort **bắt buộc phải có** trước `Skip/Take` (SQL Server yêu cầu `ORDER BY` với `OFFSET/FETCH`).

**Cập nhật `ProductQueryParameters` (đã có đủ từ Bài 2):**

```csharp
public class ProductQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }        // "name" | "price" | null
    public bool IsDescending { get; set; }     // true = DESC, false = ASC
}
```

**Controller hoàn chỉnh (gộp cả 3 bài):**

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters parameters)
{
    parameters.PageNumber = Math.Max(parameters.PageNumber, 1);
    parameters.PageSize = Math.Clamp(parameters.PageSize, 1, 50);

    var query = _context.Products.AsQueryable();

    // --- FILTERING ---
    if (parameters.CategoryId.HasValue)
        query = query.Where(x => x.CategoryId == parameters.CategoryId.Value);

    if (parameters.MinPrice.HasValue)
        query = query.Where(x => x.Price >= parameters.MinPrice.Value);

    if (parameters.MaxPrice.HasValue)
        query = query.Where(x => x.Price <= parameters.MaxPrice.Value);

    // --- SEARCHING ---
    if (!string.IsNullOrWhiteSpace(parameters.Keyword))
    {
        var keyword = parameters.Keyword.Trim();
        query = query.Where(x => x.Name.Contains(keyword));
    }

    // --- COUNT (sau filter+search) ---
    var totalRecords = await query.CountAsync();

    // --- SORTING (whitelist) ---
    query = parameters.SortBy?.ToLower() switch
    {
        "name" => parameters.IsDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name),

        "price" => parameters.IsDescending
            ? query.OrderByDescending(x => x.Price)
            : query.OrderBy(x => x.Price),

        _ => query.OrderBy(x => x.Id)   // fallback mặc định
    };

    // --- PAGINATION ---
    var data = await query
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .AsNoTracking()
        .ToListAsync();

    return Ok(new PagedResponse<Product>
    {
        Data = data,
        TotalRecords = totalRecords,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize
    });
}
```

**Ví dụ gọi API:**

```
GET /api/products?keyword=laptop&minPrice=10000000&maxPrice=30000000&sortBy=price&isDescending=true&pageNumber=2&pageSize=5
```

**Giải thích thứ tự thực thi (rất quan trọng):**

1. **Filtering** → thu hẹp tập dữ liệu theo `CategoryId`, `MinPrice`, `MaxPrice`.
2. **Searching** → tiếp tục thu hẹp theo `Name.Contains(keyword)`.
3. **Count** → đếm tổng số bản ghi khớp **cả filter + search** (đây là con số client cần để vẽ số trang).
4. **Sorting** → sắp xếp tập đã lọc.
5. **Pagination** → `Skip` + `Take` để lấy đúng trang.
6. **AsNoTracking** → tắt tracking để giảm bộ nhớ cho thao tác chỉ đọc.
7. **ToListAsync** → trigger chạy SQL xuống DB một lần duy nhất.

> **Bẫy thường gặp:** Nếu đặt `CountAsync()` **sau** `Skip/Take` sẽ đếm sai (chỉ đếm bản ghi của 1 trang). Nếu đặt `OrderBy` **sau** `Skip/Take` sẽ lỗi runtime trên SQL Server. Nếu quên **whitelist sort** sẽ dính SQL Injection.

## 6. Bảng thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| **Pagination** | Phân trang — chia tập kết quả lớn thành nhiều trang nhỏ để giảm tải bộ nhớ và băng thông. |
| **Filtering** | Lọc dữ liệu theo điều kiện chính xác trên thuộc tính (VD: `CategoryId = 3`). |
| **Searching** | Tìm kiếm chuỗi con trong trường text (VD: `Name LIKE '%keyword%'`). |
| **Sorting** | Sắp xếp theo trường và chiều (ASC/DESC). |
| **`Skip(n)`** | Bỏ qua `n` bản ghi đầu tiên — dùng cho phân trang. |
| **`Take(n)`** | Lấy `n` bản ghi — dùng cho phân trang. |
| **`OrderBy` / `OrderByDescending`** | Sắp xếp tăng/giảm dần — **bắt buộc** trước `Skip/Take` trên SQL Server. |
| **`IQueryable<T>`** | Interface cho phép build query động (deferred execution) — chỉ chạy SQL khi có terminal operator. |
| **`IEnumerable<T>`** | Interface cho collection đã nằm trong RAM — dùng sẽ mất lợi ích của `IQueryable`. |
| **Deferred Execution** | Cơ chế hoãn thực thi query đến khi có `ToList`, `Count`, `First`... |
| **Eager Loading** | Nạp dữ liệu ngay (`ToList`, `Include`) — khác với deferred. |
| **`AsNoTracking()`** | Tắt ChangeTracker cho query chỉ đọc — giảm bộ nhớ, tăng tốc. |
| **`CountAsync()`** | Đếm số bản ghi khớp điều kiện — chạy `SELECT COUNT(*)` riêng. |
| **`Math.Clamp(value, min, max)`** | Kẹp giá trị trong khoảng `[min, max]` — dùng để giới hạn `PageSize`. |
| **`Math.Max(a, b)`** | Trả về giá trị lớn hơn — dùng để đảm bảo `PageNumber >= 1`. |
| **Whitelist** | Danh sách cho phép — dùng để chặn giá trị sort/field không hợp lệ. |
| **`[FromQuery]`** | Attribute bind tham số từ query string (hoặc từ property của class nếu đặt trên parameter class). |
| **`PagedResponse<T>`** | Class generic đóng gói `Data` + metadata phân trang (`TotalRecords`, `TotalPages`...). |
| **Computed Property** | Property chỉ có getter, giá trị tính từ property khác (VD: `TotalPages`). |
| **Integer Division** | Chia 2 số nguyên trong C# → kết quả bị cắt phần thập phân (cần cast `(double)` trước khi chia). |
| **SQL Injection qua ORDER BY** | Lỗ hổng khi ghép chuỗi client vào `ORDER BY` — phòng bằng whitelist. |
| **Magic String** | Chuỗi literal rải rác trong code (VD: `"price"`) — dễ sai chính tả, nên đưa vào `const`. |

## 7. Cheatsheet

### 7.1. Query string mẫu

| Tham số | Ý nghĩa | Ví dụ |
|---|---|---|
| `pageNumber` | Trang hiện tại (≥ 1) | `pageNumber=2` |
| `pageSize` | Số bản ghi / trang (1–50) | `pageSize=20` |
| `keyword` | Từ khóa tìm kiếm | `keyword=laptop` |
| `categoryId` | Lọc theo danh mục | `categoryId=3` |
| `minPrice` / `maxPrice` | Lọc theo khoảng giá | `minPrice=100&maxPrice=500` |
| `sortBy` | Trường sắp xếp (`name` \| `price`) | `sortBy=price` |
| `isDescending` | Chiều sắp xếp | `isDescending=true` |

**Ví dụ URL đầy đủ:**

```
/api/products?keyword=laptop&categoryId=3&minPrice=100&maxPrice=500&sortBy=price&isDescending=true&pageNumber=2&pageSize=20
```

### 7.2. Công thức cốt lõi

| Mục đích | Công thức LINQ |
|---|---|
| Tính số bản ghi bỏ qua | `skip = (pageNumber - 1) * pageSize` |
| Lấy 1 trang | `query.Skip(skip).Take(pageSize)` |
| Đếm tổng | `await query.CountAsync()` |
| Tính tổng số trang | `Math.Ceiling((double)totalRecords / pageSize)` |
| Clamp pageSize | `Math.Clamp(pageSize, 1, 50)` |
| Đảm bảo pageNumber ≥ 1 | `Math.Max(pageNumber, 1)` |
| Tắt tracking khi chỉ đọc | `.AsNoTracking()` |

### 7.3. Pipeline xử lý chuẩn

```text
[1] Nhận QueryParameters
        ↓
[2] Chuẩn hóa: Math.Max + Math.Clamp
        ↓
[3] Khởi tạo: _context.X.AsQueryable()
        ↓
[4] Filtering: if (has value) query = query.Where(...)
        ↓
[5] Searching: if (!empty) query = query.Where(x => x.Name.Contains(...))
        ↓
[6] CountAsync() ← đếm SAU filter+search
        ↓
[7] Sorting (whitelist switch) ← BẮT BUỘC trước Skip/Take
        ↓
[8] Pagination: Skip(...).Take(...)
        ↓
[9] AsNoTracking() + ToListAsync()
        ↓
[10] Trả về PagedResponse<T>
```

### 7.4. Checklist phòng lỗi

- [ ] `PageNumber` được clamp ≥ 1.
- [ ] `PageSize` được clamp trong khoảng `[1, 50]`.
- [ ] Có `OrderBy` **trước** `Skip/Take`.
- [ ] `CountAsync()` chạy **sau** filter/search, **trước** Skip/Take.
- [ ] Sort dùng **whitelist** `switch`, có fallback mặc định.
- [ ] Query chỉ đọc dùng `.AsNoTracking()`.
- [ ] `Keyword` được `Trim()` và kiểm tra `!string.IsNullOrWhiteSpace`.
- [ ] `TotalPages` dùng `(double)` trước khi chia.
- [ ] Trả `PagedResponse<T>` thay vì chỉ `List<T>`.
- [ ] Không dùng `IEnumerable` thay `IQueryable` khi build query.

### 7.5. Mẫu `PagedResponse<T>` chuẩn

```csharp
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### 7.6. Mẫu `ProductQueryParameters` chuẩn

```csharp
public class ProductQueryParameters
{
    private const int MaxPageSize = 50;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(value, 1);
    }

    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
}
```

> **Ghi chú:** Cách đặt `Math.Clamp`/`Math.Max` **ngay trong setter** của property giúp controller không cần normalize thủ công — gọn hơn và không thể quên.


# Buổi 12: Global Exception Handling, Logging, Custom Middleware

## Mục lục

```text
Buổi 12: Global Exception Handling, Logging, Custom Middleware
│
├── 1. Lý thuyết nền tảng
│   ├── 1.1. Vấn đề của Exception truyền thống
│   └── 1.2. Request Pipeline trong ASP.NET Core
│
├── 2. Lý thuyết chính
│   ├── 2.1. Global Exception Handling
│   │   ├── 2.1.1. Khái niệm
│   │   ├── 2.1.2. Cơ chế hoạt động
│   │   ├── 2.1.3. Tự viết Custom Exception Middleware
│   │   └── 2.1.4. Đăng ký Middleware vào Pipeline
│   ├── 2.2. Logging
│   │   ├── 2.2.1. Log Level
│   │   ├── 2.2.2. Sử dụng ILogger
│   │   └── 2.2.3. Cấu hình Logging
│   └── 2.3. Custom Middleware
│       ├── 2.3.1. Khái niệm
│       ├── 2.3.2. Ví dụ Logging Middleware đo thời gian
│       └── 2.3.3. Thứ tự đăng ký Middleware
│
├── 3. Ví dụ nền tảng
│   ├── 3.1. GlobalExceptionMiddleware hoàn chỉnh
│   ├── 3.2. RequestTimingMiddleware
│   └── 3.3. Sử dụng ILogger trong Controller
│
├── 4. Thực hành / Lab cơ bản
│   └── 4.1. Xử lý lỗi tập trung + Ghi log
│
├── 5. Bài tập mở rộng
│   └── 5.1. Bài thực hành (3 bài, giải chi tiết)
│
├── 6. Bảng thuật ngữ
│
└── 7. Cheatsheet
```

## 1. Lý thuyết nền tảng

### 1.1. Vấn đề của Exception truyền thống

**Thông thường:** Lập trình viên sử dụng khối `try-catch` trong mọi hàm của Controller hoặc Service.

**Hậu quả:**

- Code bị phình to (**Code Bloat**).
- Lặp lại logic xử lý lỗi.
- Dễ bỏ sót (quên `try-catch`).
- Nếu lỗi xảy ra ngoài phạm vi `try-catch` (VD: lỗi Middleware khác), ứng dụng sẽ **sập (Crash)** hoặc trả về **cấu trúc lỗi không đồng nhất** cho Client.

> **Ghi chú bổ sung:** Vấn đề "cấu trúc lỗi không đồng nhất" rất nghiêm trọng với Client: khi thì nhận `{ "error": "..." }`, khi thì `{ "message": "..." }`, khi thì HTML lỗi mặc định của ASP.NET Core (DeveloperExceptionPage). Client không thể viết một handler thống nhất. **Global Exception Middleware** giải quyết triệt để vấn đề này bằng cách đảm bảo **mọi lỗi 500 đều cùng một schema JSON**.

### 1.2. Request Pipeline trong ASP.NET Core

> **Ghi chú bổ sung (kiến thức nền tảng bị thiếu):** Để hiểu tại sao phải đăng ký Middleware theo thứ tự, cần biết **Request Pipeline** là gì.

**Request Pipeline** là một chuỗi các Middleware được thực thi tuần tự theo mô hình **"vòng cung" (onion model)**:

```text
Request  →  [Middleware A]  →  [Middleware B]  →  [Middleware C]  →  Controller
Response ←  [Middleware A]  ←  [Middleware B]  ←  [Middleware C]  ←  Controller
```

Mỗi Middleware:

- Có thể **xử lý request trước** khi gọi `_next(context)`.
- Có thể **xử lý response sau** khi `_next(context)` trả về.
- Có thể **dừng pipeline** (short-circuit) bằng cách không gọi `_next`.
- Có thể **bắt exception** từ các Middleware phía sau.

**Hệ quả quan trọng:**

- Middleware đăng ký **càng sớm** (ở đầu pipeline) → phạm vi bắt lỗi **càng rộng**.
- Exception Middleware phải đăng ký **đầu tiên** để bắt được lỗi của **tất cả** Middleware phía sau.
- Nếu đặt Exception Middleware **sau** `UseAuthentication()`, lỗi xảy ra trong quá trình Authentication sẽ **không được bắt**.

**Thứ tự khuyến nghị chuẩn của ASP.NET Core:**

```text
1. Exception Handler            ← bắt mọi lỗi phía sau
2. HSTS / HTTPS Redirection
3. Static Files
4. Routing
5. CORS
6. Authentication               ← xác thực (ai?)
7. Authorization                ← phân quyền (được làm gì?)
8. Custom Middleware            ← (timing, audit...)
9. Endpoints / Controllers
```

## 2. Lý thuyết chính

### 2.1. Global Exception Handling

#### 2.1.1. Khái niệm

**Global Exception Handling** là một cơ chế **tập trung duy nhất** đứng trong Request Pipeline, có trách nhiệm **hứng mọi exception chưa được bắt** và quy đổi chúng thành **HTTP Response chuẩn**.

Đặc điểm:

- **Điểm duy nhất** xử lý lỗi (**single responsibility**).
- Áp dụng cho **tất cả request** đi qua pipeline.
- Triển khai dưới dạng **ASP.NET Core Middleware**.

Lợi ích:

- Giữ cho Controller/Service **sạch sẽ** (chỉ chứa Business Logic).
- Đảm bảo **mọi lỗi chưa được bắt** (Unhandled Exceptions) đều được xử lý trước khi trả về Client.
- Trả về **cấu trúc Error Model chuẩn REST** cho Client (ví dụ: format JSON chung cho mọi lỗi 500).

#### 2.1.2. Cơ chế hoạt động

Khi Controller ném ra `Exception`, Exception Middleware sẽ:

1. **Hứng (catch)** lại exception.
2. **Ghi Log** (thường ở mức `Error`).
3. **Tạo HTTP Response** (mã 500) và trả về cho Client theo schema JSON thống nhất.

#### 2.1.3. Tự viết Custom Exception Middleware

Tạo class `GlobalExceptionMiddleware.cs` gồm 2 phần chính:

**Phần 1 — Constructor inject dependencies:**

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
```

**Phần 2 — `InvokeAsync` bọc `_next` trong `try-catch`:**

```csharp
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // Chạy tiếp pipeline
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi hệ thống xảy ra");
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

> **Ghi chú bổ sung:** Tên method **bắt buộc** là `InvokeAsync` (hoặc `Invoke`) — đây là convention mà ASP.NET Core dùng reflection để tìm khi gọi `app.UseMiddleware<T>()`. Constructor nhận `RequestDelegate next` — đây chính là "mũi tên" trỏ tới Middleware kế tiếp trong pipeline. `ILogger<T>` được inject tự động vì đã đăng ký sẵn trong DI container.

**Hàm format JSON lỗi:**

```csharp
private static Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

    var response = new
    {
        StatusCode = context.Response.StatusCode,
        Message = "Đã có lỗi xảy ra từ máy chủ, vui lòng thử lại sau.",
        Detail = ex.Message
        // Chỉ nên log, cân nhắc không gửi ra cho client môi trường Prod
    };

    return context.Response.WriteAsJsonAsync(response);
}
```

> **Lưu ý bảo mật:** Trường `Detail = ex.Message` **có thể rò rỉ thông tin nội bộ** (tên bảng, tên cột, connection string nếu exception từ EF Core). Ở môi trường `Production`, nên:
> - **Ẩn** `Detail` khỏi response.
> - **Chỉ log** đầy đủ ở server.
> - Hoặc dùng `IWebHostEnvironment.IsDevelopment()` để quyết định có trả `Detail` hay không.
>
> Ngoài ra có thể dùng `Activity.Current?.Id` (trace ID) để client báo lại khi cần debug, còn server tra log theo trace ID đó.

#### 2.1.4. Đăng ký Middleware vào Pipeline

Trong `Program.cs`:

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

> **Lỗi thường gặp:** Thứ tự đăng ký Middleware **rất quan trọng**. Exception Middleware nên được đăng ký **đầu tiên** (ngay sau `builder.Build()`) để nó có thể "hứng" được lỗi của **tất cả** các Middleware chạy sau nó.

> **Ghi chú bổ sung:** Có 2 cách viết Middleware:
> - **Convention-based** (như `GlobalExceptionMiddleware`): class có constructor nhận `RequestDelegate`, có method `InvokeAsync(HttpContext)`. Đăng ký bằng `app.UseMiddleware<T>()`.
> - **Factory-based**: dùng `app.Use(async (context, next) => { ... })` — ngắn gọn cho logic đơn giản, không cần class riêng. Phù hợp cho Middleware "dùng một lần" hoặc prototype.
>
> Ngoài ra từ .NET 8 còn có **`IExceptionHandler`** (interface mới) — cách hiện đại hơn để xử lý exception tập trung, đăng ký bằng `builder.Services.AddExceptionHandler<T>()` + `app.UseExceptionHandler()`. Tuy nhiên Custom Middleware vẫn là kiến thức nền tảng cần nắm.

### 2.2. Logging

**Định nghĩa:** Logging là quá trình ghi lại thông tin hoạt động, lỗi hệ thống để phục vụ việc **bảo trì** và **gỡ lỗi (Debugging)**.

ASP.NET Core tích hợp sẵn framework Logging, hỗ trợ nhiều mức độ (**Log Level**):

#### 2.2.1. Log Level

| Level | Mục đích | Ví dụ |
|---|---|---|
| `Trace` | Chi tiết cực cao (từng bước nội bộ) | "Đang parse JSON..." |
| `Debug` | Thông tin gỡ lỗi khi phát triển | "Giá trị biến x = 5" |
| `Information` | Luồng nghiệp vụ bình thường | "User A đã đăng nhập" |
| `Warning` | Vấn đề bất thường nhưng chưa gây sập luồng | "API chậm 2 giây" |
| `Error` | Lỗi (Exception) | "NullReferenceException tại..." |
| `Critical` | Lỗi nghiêm trọng toàn hệ thống | "Mất kết nối DB" |

> **Ghi nhớ:** `_logger.LogError(ex, ...)` ở phần 2.1 chính là một lời gọi Logging — giờ ta tìm hiểu kỹ cơ chế phía sau nó.

> **Ghi chú bổ sung:** Log Level được **sắp xếp tăng dần theo độ nghiêm trọng**: `Trace (0) < Debug (1) < Information (2) < Warning (3) < Error (4) < Critical (5) < None (6)`. Khi cấu hình `MinimumLevel` (hoặc `LogLevel:Default`), **chỉ các log có level ≥ ngưỡng** mới được ghi. Ví dụ set `Warning` → các log `Trace/Debug/Information` bị **bỏ qua hoàn toàn** (không tốn CPU format message).

#### 2.2.2. Sử dụng ILogger

Inject `ILogger<T>` vào bất kỳ class nào (Controller, Service, Middleware):

```csharp
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;

    public ProductController(ILogger<ProductController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Client gọi API Get danh sách sản phẩm.");
        return Ok();
    }
}
```

> **Ghi chú bổ sung:** `ILogger<T>` với `T` là **category name** — thường là tên class. Log sẽ được gắn category để dễ filter (VD: `Microsoft.AspNetCore.Mvc.ProductController`). Khi inject trong `Program.cs`, dùng `ILogger<Program>` hoặc `ILoggerFactory.CreateLogger("MyCategory")`.
>
> **Structured Logging:** Nên dùng **message template** với placeholder thay vì string interpolation:
> - ✅ `_logger.LogInformation("User {UserId} logged in at {Time}", userId, DateTime.UtcNow);`
> - ❌ `_logger.LogInformation($"User {userId} logged in at {DateTime.UtcNow}");`
>
> Lý do: cách đầu giữ **tên tham số** (UserId, Time) — Seq/Elasticsearch có thể query theo `UserId = 123`. Cách sau chỉ là string thô, mất cấu trúc.

#### 2.2.3. Cấu hình Logging trong appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"  // Ẩn các log mặc định của ASP.NET
    }
  }
}
```

> **Best Practice:** **Không dùng `Console.WriteLine`** trong ASP.NET Core, hãy luôn dùng `ILogger`.

> **Ghi chú bổ sung:** `"Microsoft.AspNetCore": "Warning"` nghĩa là **riêng namespace** `Microsoft.AspNetCore.*` chỉ ghi từ mức `Warning` trở lên — giúp **giảm nhiễu log** (framework log rất nhiều ở mức `Information`). Đây là pattern phổ biến: `Default = Information` cho code của mình, `Microsoft.* = Warning` để tắt noise.
>
> **Logging Provider:** `ILogger` là **abstraction**; output đi đâu do **Provider** quyết định. Mặc định có:
> - `Console` (in ra terminal).
> - `Debug` (Output window của Visual Studio).
> - `EventSource` / `EventLog` (Windows).
>
> Production thường cài thêm provider: **Serilog** (ghi file, gửi Seq/Elasticsearch), **NLog**, hoặc **Application Insights** (Azure). Có thể cấu hình nhiều provider cùng lúc.

### 2.3. Custom Middleware

#### 2.3.1. Khái niệm

**Middleware** là một thành phần xử lý nằm trên chuỗi (**pipeline**) xử lý Request/Response. `GlobalExceptionMiddleware` ở trên chính là một Custom Middleware — giờ ta **khái quát hóa** kỹ thuật này cho các mục đích khác.

Không chỉ dùng để **bắt lỗi**. Có thể tự viết Middleware để **can thiệp vào mọi Request**, ví dụ:

- **Đo thời gian** xử lý (timing).
- **Kiểm tra Header** (VD: API key, version client).
- **Ghi Audit Log** (ai làm gì, khi nào).
- **Rate limiting** (giới hạn request/phút).
- **Response caching**.
- **Thêm security headers** (X-Frame-Options, CSP...).

> **Ghi chú bổ sung — Phân biệt Middleware và Filter:**
>
> | Đặc điểm | Middleware | Filter (`IActionFilter`, `IExceptionFilter`...) |
> |---|---|---|
> | Phạm vi | **Toàn bộ** pipeline (kể cả static file, routing) | **Chỉ trong MVC** (trước/sau action) |
> | Truy cập `HttpContext` | ✅ Có | ✅ Có |
> | Truy cập `ActionDescriptor`, model binding | ❌ Không | ✅ Có |
> | Đăng ký | `app.UseMiddleware<T>()` | `[ServiceFilter]` hoặc `options.Filters.Add<T>()` |
>
> **Quy tắc chọn:** Logic áp dụng **mọi request** → Middleware. Logic chỉ liên quan đến **MVC action** (validation, logging model) → Filter. `GlobalExceptionMiddleware` chọn Middleware vì muốn bắt cả lỗi từ static file, từ routing, từ authentication.

#### 2.3.2. Ví dụ Logging Middleware đo thời gian

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        await _next(context);   // Chờ Request chạy xong
        watch.Stop();

        _logger.LogInformation(
            "Request {Path} mất {ElapsedMilliseconds} ms",
            context.Request.Path,
            watch.ElapsedMilliseconds);
    }
}
```

> **Ghi chú bổ sung:**
>
> - `Stopwatch.StartNew()` dùng **high-resolution timer** (`QueryPerformanceCounter` trên Windows) — chính xác hơn `DateTime.UtcNow` cho việc đo khoảng thời gian ngắn.
> - **Hạn chế:** Middleware này **không đo được request bị exception** (vì `await _next` ném lỗi → code sau không chạy). Cách sửa: bọc `try-finally`:
>
> ```csharp
> public async Task InvokeAsync(HttpContext context)
> {
>     var watch = Stopwatch.StartNew();
>     try { await _next(context); }
>     finally
>     {
>         watch.Stop();
>         _logger.LogInformation(
>             "Request {Path} → {StatusCode} mất {Ms} ms",
>             context.Request.Path,
>             context.Response.StatusCode,
>             watch.ElapsedMilliseconds);
>     }
> }
> ```
>
> - Nên log thêm `StatusCode` để biết request có thành công không.
> - Trong Production thực tế, ASP.NET Core đã có sẵn **`UseHttpLogging()`** (từ .NET 6) hoặc **`app.UseSerilogRequestLogging()`** làm việc này tốt hơn — nhưng tự viết vẫn là bài học cần nắm.

#### 2.3.3. Thứ tự đăng ký Middleware

```text
┌─────────────────────────────────────────────────────────────┐
│  app.UseMiddleware<GlobalExceptionMiddleware>();   ← ĐẦU   │
│      ↓                                                       │
│  app.UseMiddleware<RequestTimingMiddleware>();              │
│      ↓                                                       │
│  app.UseAuthentication();                                   │
│      ↓                                                       │
│  app.UseAuthorization();                                    │
│      ↓                                                       │
│  app.MapControllers();                             ← CUỐI   │
└─────────────────────────────────────────────────────────────┘
```

**Quy tắc:** Middleware **bắt lỗi** phải đứng **trước** mọi Middleware có thể ném lỗi. Middleware **đo thời gian** thường đứng **sau** Exception Middleware (để vẫn chạy được khi lỗi xảy ra, nhờ `try-finally` hoặc nằm trong vùng bảo vệ của Exception Middleware).

## 3. Ví dụ nền tảng

### 3.1. GlobalExceptionMiddleware hoàn chỉnh (khuyến nghị dùng)

```csharp
using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Sinh trace ID để client báo lại khi cần
            var traceId = context.TraceIdentifier;

            _logger.LogError(ex,
                "Unhandled exception. TraceId={TraceId}, Path={Path}",
                traceId, context.Request.Path);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private Task HandleExceptionAsync(
        HttpContext context, Exception ex, string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Đã có lỗi xảy ra từ máy chủ, vui lòng thử lại sau.",
            TraceId = traceId,
            // Chỉ trả Detail ở Development
            Detail = _env.IsDevelopment() ? ex.Message : null
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

> **Cải tiến so với bản gốc:** (1) Ẩn `Detail` ở Production, (2) thêm `TraceId` để debug qua log, (3) log kèm `Path` và `TraceId` để filter dễ.

### 3.2. RequestTimingMiddleware (khuyến nghị dùng `try-finally`)

```csharp
using System.Diagnostics;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            watch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} → {StatusCode} mất {Ms} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                watch.ElapsedMilliseconds);
        }
    }
}
```

### 3.3. Sử dụng ILogger trong Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;

    public ProductController(ILogger<ProductController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Client gọi API Get danh sách sản phẩm.");
        return Ok(new[] { "Sản phẩm A", "Sản phẩm B" });
    }
}
```

### 3.4. Đăng ký đầy đủ trong Program.cs

```csharp
var app = builder.Build();

// 1. Exception Middleware phải đứng ĐẦU TIÊN
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Timing Middleware đứng sau Exception Middleware
app.UseMiddleware<RequestTimingMiddleware>();

// 3. Các Middleware khác
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## 4. Thực hành / Lab cơ bản

### 4.1. Xử lý lỗi tập trung + Ghi log

**Các bước:**

1. Tạo API `/api/test/error` — cố tình throw exception.
2. Tạo `GlobalExceptionMiddleware` — xử lý lỗi, trả JSON chuẩn.
3. Tạo `RequestTimingMiddleware` — đo thời gian xử lý.
4. Đăng ký cả hai vào `Program.cs` **đúng thứ tự**.

## 5. Bài tập mở rộng

### 5.1. Bài thực hành

---

#### Bài 1: API cố tình sinh lỗi + ghi log

**Yêu cầu:** Tạo API `/api/test/error` cố tình sinh ra (throw) `Exception("Lỗi thử nghiệm")`. Inject `ILogger` và ghi ra console 1 dòng `LogError` **trước khi** ném lỗi.

**Giải thích chi tiết:**

- Controller này **không có** `try-catch` — đó là mục đích của bài: chứng minh Global Exception Middleware sẽ hứng được lỗi.
- `_logger.LogError(ex, ...)` ghi **cả exception object** → log sẽ chứa stack trace đầy đủ, không chỉ message.
- Sau `throw`, code phía dưới **không chạy** → có thể thấy trong log.

**Code:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("error")]
    public IActionResult ThrowError()
    {
        var ex = new Exception("Lỗi thử nghiệm");

        // Ghi log TRƯỚC khi ném lỗi — nhưng lưu ý GlobalExceptionMiddleware
        // cũng sẽ log lần nữa → có thể trùng. Thực tế chỉ nên log 1 nơi.
        _logger.LogError(ex, "API test/error cố tình ném lỗi.");

        throw ex;   // Không có try-catch → Middleware sẽ hứng
    }
}
```

> **Lưu ý thực tế:** Việc log ở **cả Controller và Middleware** sẽ tạo ra **2 dòng log trùng lặp** cho cùng 1 exception. Best Practice: **chỉ log ở Middleware** (nơi duy nhất biết exception là "unhandled"). Controller chỉ nên log khi exception là **expected** (ví dụ validation fail). Trong bài tập này, log ở Controller để **thấy rõ cả 2 giai đoạn**.

**Test bằng Postman/curl:**

```bash
curl -i http://localhost:5000/api/test/error
```

**Kết quả mong đợi** (sau khi đăng ký Middleware ở Bài 2):

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json

{
  "statusCode": 500,
  "message": "Đã có lỗi xảy ra từ máy chủ, vui lòng thử lại sau.",
  "traceId": "0HN7G...",
  "detail": "Lỗi thử nghiệm"
}
```

---

#### Bài 2: GlobalExceptionMiddleware

**Yêu cầu:** Tạo class `GlobalExceptionMiddleware`. Xử lý ngoại lệ, thiết lập `StatusCode = 500` và trả về JSON chuẩn chứa trường `message`. Đăng ký vào `Program.cs`.

**Giải thích chi tiết:**

- Phải có `RequestDelegate _next` (để chạy tiếp pipeline) và `ILogger` (để ghi log).
- Method **bắt buộc tên `InvokeAsync`** với tham số `HttpContext`.
- Bọc `await _next(context)` trong `try-catch` — đây là **điểm mấu chốt**: mọi exception từ **bất kỳ Middleware nào phía sau** đều nổi lên đây.
- Ghi `ContentType = "application/json"` **trước** `StatusCode` để đảm bảo response đúng format.
- **Phải đăng ký ĐẦU TIÊN** trong `Program.cs`.

**Code (xem đầy đủ ở mục 3.1 phía trên):**

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex,
                "Unhandled exception. TraceId={TraceId}, Path={Path}",
                traceId, context.Request.Path);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private Task HandleExceptionAsync(
        HttpContext context, Exception ex, string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Đã có lỗi xảy ra từ máy chủ, vui lòng thử lại sau.",
            TraceId = traceId,
            Detail = _env.IsDevelopment() ? ex.Message : null
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

**Đăng ký trong `Program.cs`:**

```csharp
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();  // ĐẦU TIÊN
// ... các middleware khác

app.MapControllers();
app.Run();
```

**Test:**

1. **Chưa đăng ký Middleware** → gọi `/api/test/error` → nhận HTML lỗi mặc định (DeveloperExceptionPage ở Development, hoặc trang trắng 500 ở Production) → **không đồng nhất**.
2. **Đã đăng ký Middleware** → gọi `/api/test/error` → nhận JSON chuẩn với `statusCode: 500` → **đồng nhất**.

**Kiểm chứng thứ tự Middleware sai:**

- Nếu đăng ký `GlobalExceptionMiddleware` **sau** `app.MapControllers()` — **không có tác dụng** (vì routing đã chạy xong trước đó, middleware này không nằm trong pipeline của request).
- Nếu đặt **sau** `UseAuthentication()` — lỗi xảy ra **trong** quá trình authentication (VD: JWT malformed) sẽ **không được bắt** → lộ thông tin qua trang lỗi mặc định.

---

#### Bài 3: RequestTimingMiddleware

**Yêu cầu:** Viết thêm một `RequestTimingMiddleware` tự tạo. Ghi ra log (`LogInformation`) thời gian (**milliseconds**) thực thi của **mọi request** đi vào hệ thống. Đăng ký nó **đứng sau** Exception Middleware.

**Giải thích chi tiết:**

- Dùng `Stopwatch.StartNew()` **trước** `await _next` và `Stop()` **sau** — đó chính là thời gian pipeline phía sau xử lý request.
- **Bọc `try-finally`** thay vì không bọc — để vẫn log được khi có exception (nếu không có `try-finally`, khi exception xảy ra code log sẽ **không chạy** vì `await _next` ném lỗi lên trên).
- Log kèm `Method`, `Path`, `StatusCode` để dễ truy vết.
- **Đăng ký sau Exception Middleware** — để khi Exception Middleware bắt lỗi và trả response, timing Middleware vẫn log được.

**Code (xem đầy đủ ở mục 3.2 phía trên):**

```csharp
using System.Diagnostics;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            watch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} → {StatusCode} mất {Ms} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                watch.ElapsedMilliseconds);
        }
    }
}
```

**Đăng ký trong `Program.cs`:**

```csharp
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();   // 1. ĐẦU TIÊN
app.UseMiddleware<RequestTimingMiddleware>();     // 2. SAU Exception MW

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**Minh họa thứ tự thực thi:**

```text
Request /api/test/error
│
├─ GlobalExceptionMiddleware.InvokeAsync   ← vào
│  ├─ RequestTimingMiddleware.InvokeAsync  ← vào
│  │  ├─ Stopwatch.StartNew()
│  │  ├─ TestController.ThrowError()
│  │  │  └─ throw Exception("Lỗi thử nghiệm") ⚡
│  │  └─ finally: Stopwatch.Stop() → log "500 ... mất X ms"
│  │     (exception tiếp tục nổi lên)
│  ├─ catch (Exception ex)                 ← bắt
│  └─ HandleExceptionAsync → response 500 JSON
│
Response 500
```

**Kết quả log mẫu:**

```text
info: RequestTimingMiddleware[0]
      Request GET /api/test/error → 500 mất 12 ms
fail: GlobalExceptionMiddleware[0]
      Unhandled exception. TraceId=0HN7G..., Path=/api/test/error
      System.Exception: Lỗi thử nghiệm
         at TestController.ThrowError() in ...
```

**Thử thách mở rộng (tự luyện):**

1. Đổi `LogInformation` thành `LogWarning` khi `ElapsedMilliseconds > 1000` (cảnh báo API chậm).
2. Thêm header `X-Response-Time: 12ms` vào response để client tự đo.
3. Thay `Stopwatch` bằng `System.Diagnostics.Activity` (OpenTelemetry) để tích hợp distributed tracing.

## 6. Bảng thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| **Exception** | Ngoại lệ — tình huống bất thường xảy ra lúc runtime (VD: null reference, DB timeout). |
| **Unhandled Exception** | Exception **không được** `try-catch` ở đâu cả → nổi lên tận cùng pipeline. |
| **Global Exception Handling** | Cơ chế xử lý **tập trung** mọi unhandled exception tại **một điểm duy nhất**. |
| **Request Pipeline** | Chuỗi các Middleware thực thi tuần tự theo mô hình "vòng cung" (onion). |
| **Middleware** | Thành phần xử lý HTTP request/response — có thể chặn, sửa, hoặc chuyển tiếp. |
| **`RequestDelegate`** | Delegate đại diện cho Middleware kế tiếp trong pipeline — gọi `await _next(context)` để chuyển tiếp. |
| **`InvokeAsync(HttpContext)`** | Method **bắt buộc** trong Convention-based Middleware — được gọi mỗi request. |
| **Convention-based Middleware** | Middleware viết dưới dạng class có constructor nhận `RequestDelegate` + method `InvokeAsync`. |
| **Factory-based Middleware** | Middleware viết bằng `app.Use(async (ctx, next) => { ... })` — không cần class. |
| **Short-circuit** | Middleware **dừng** pipeline (không gọi `_next`) — VD: auth fail trả 401 luôn. |
| **Logging** | Ghi log — lưu lại thông tin hoạt động/lỗi để debug và giám sát. |
| **Log Level** | Mức độ nghiêm trọng của log: `Trace < Debug < Information < Warning < Error < Critical`. |
| **`ILogger<T>`** | Interface logging — `T` là **category** (thường là tên class). Inject qua DI. |
| **Structured Logging** | Log dùng **message template** với placeholder (`{UserId}`) thay vì string interpolation — cho phép query theo field. |
| **Logging Provider** | Đích ghi log (Console, Debug, Serilog, Application Insights...). |
| **`IWebHostEnvironment`** | Interface cung cấp thông tin môi trường (`IsDevelopment()`, `IsProduction()`...). |
| **`HttpContext`** | Đối tượng chứa toàn bộ thông tin 1 request/response (Request, Response, User, TraceIdentifier...). |
| **`context.TraceIdentifier`** | Mã ID duy nhất cho mỗi request — dùng để correlate log ↔ response. |
| **`Stopwatch`** | Lớp đo thời gian high-resolution của .NET — chính xác hơn `DateTime.Now`. |
| **`WriteAsJsonAsync`** | Extension method ghi object ra response dưới dạng JSON — tự set `Content-Type: application/json`. |
| **`UseMiddleware<T>()`** | Extension đăng ký Middleware class vào pipeline. |
| **Code Bloat** | Code phình to do lặp lại logic — hệ quả của `try-catch` rải rác. |
| **Onion Model** | Mô hình pipeline: request đi vào qua các lớp, response đi ra ngược lại. |
| **`IExceptionHandler`** | Interface mới (.NET 8+) — cách hiện đại thay thế Custom Exception Middleware. |

## 7. Cheatsheet

### 7.1. Thứ tự Middleware chuẩn

```text
1. GlobalExceptionMiddleware       ← BẮT BUỘC ĐẦU TIÊN
2. RequestTimingMiddleware         ← đo thời gian
3. HSTS / HttpsRedirection
4. StaticFiles
5. Routing
6. CORS
7. Authentication
8. Authorization
9. Custom business Middleware
10. MapControllers()
```

**Quy tắc nhớ nhanh:** *"Bắt lỗi sớm — Xác thực muộn — Controller cuối cùng"*

### 7.2. Mẫu Middleware nhanh

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MyMiddleware> _logger;

    public MyMiddleware(RequestDelegate next, ILogger<MyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // [A] Trước khi vào pipeline phía sau
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // [B] Bắt lỗi
            _logger.LogError(ex, "Lỗi");
        }
        finally
        {
            // [C] Luôn chạy — kể cả có lỗi
        }
        // [D] Sau khi pipeline phía sau chạy xong
    }
}
```

### 7.3. Bảng Log Level

| Level | Khi nào dùng | Ngưỡng Production khuyến nghị |
|---|---|---|
| `Trace` | Chi tiết nội bộ từng bước | ❌ Tắt |
| `Debug` | Giá trị biến khi dev | ❌ Tắt |
| `Information` | Sự kiện nghiệp vụ | ✅ Bật |
| `Warning` | Bất thường chưa gây lỗi | ✅ Bật |
| `Error` | Exception đã bắt | ✅ Bật |
| `Critical` | Sập hệ thống | ✅ Bật (alert ngay) |

### 7.4. Mẫu cấu hình appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### 7.5. Checklist xử lý Exception

- [ ] `GlobalExceptionMiddleware` đăng ký **đầu tiên** trong pipeline.
- [ ] Method tên **`InvokeAsync`** (đúng convention).
- [ ] `_next(context)` bọc trong `try-catch`.
- [ ] Ghi log **kèm exception object** (`_logger.LogError(ex, ...)`).
- [ ] Response có **schema JSON thống nhất** (`statusCode`, `message`, `traceId`).
- [ ] Trả **`TraceId`** để client báo lại khi cần.
- [ ] **Ẩn `Detail`** ở Production (chỉ trả ở Development).
- [ ] `ContentType = "application/json"` **trước** khi ghi body.
- [ ] Không quên `app.UseMiddleware<GlobalExceptionMiddleware>()` trong `Program.cs`.

### 7.6. So sánh nhanh

| Tiêu chí | `try-catch` rải rác | Global Exception Middleware |
|---|---|---|
| Code bloat | ❌ Nhiều | ✅ Không |
| Bắt lỗi ngoài Controller | ❌ Không | ✅ Có |
| Cấu trúc lỗi đồng nhất | ❌ Không | ✅ Có |
| Log tập trung | ❌ Không | ✅ Có |
| Debug từng hàm | ✅ Dễ (local) | ⚠️ Phải xem stack trace |
| Single Responsibility | ❌ Vi phạm | ✅ Tuân thủ |

> **Kết luận:** Dùng `try-catch` **chỉ khi cần xử lý cục bộ** (fallback, retry, business exception). Còn lại → để Global Middleware lo.

### 7.7. Câu lệnh test nhanh

```bash
# Test endpoint gây lỗi
curl -i http://localhost:5000/api/test/error

# Xem response header + body
curl -v http://localhost:5000/api/test/error

# Test nhiều request liên tiếp để xem timing log
for i in {1..5}; do curl -s http://localhost:5000/api/products > /dev/null; done
```


# Buổi 13: Tài liệu API với Swagger, CORS, Triển khai ứng dụng và Tối ưu cấu hình

## Mục lục

```text
Buổi 13: Swagger, CORS, Publish, Docker
│
├── 1. Lý thuyết nền tảng
│   ├── 1.1. Tại sao cần cấu hình JWT cho Swagger?
│   ├── 1.2. CORS là gì và vì sao cần?
│   ├── 1.3. Publish là gì?
│   ├── 1.4. Docker là gì?
│   └── 1.5. So sánh các phương thức triển khai
│
├── 2. Lý thuyết chính
│   ├── 2.1. Swagger nâng cao — Cấu hình JWT
│   ├── 2.2. CORS — Cấu hình trong ASP.NET Core
│   ├── 2.3. Publish — Chế độ và lệnh CLI
│   ├── 2.4. Triển khai lên IIS
│   ├── 2.5. Triển khai bằng Docker
│   └── 2.6. Ghi nhớ, Lỗi thường gặp & Best Practice
│
├── 3. Ví dụ nền tảng
│   ├── 3.1. Cấu hình Swagger JWT hoàn chỉnh
│   ├── 3.2. Cấu hình CORS hoàn chỉnh
│   ├── 3.3. Lệnh Publish
│   └── 3.4. Dockerfile mẫu
│
├── 4. Thực hành / Lab cơ bản
│   ├── 4.1. Publish ứng dụng
│   └── 4.2. Kiểm thử API
│
├── 5. Bài tập mở rộng
│   └── 5.1. Bài thực hành (5 bài, giải chi tiết)
│
├── 6. Bảng thuật ngữ
│
└── 7. Cheatsheet
```

## 1. Lý thuyết nền tảng

### 1.1. Tại sao cần cấu hình JWT cho Swagger?

Mặc định, **Swagger không biết cách gửi token** vào header `Authorization`. Cần cấu hình để Swagger có ô nhập **Bearer Token**, giúp test các API yêu cầu `[Authorize]` ngay trên giao diện Swagger UI thay vì phải dùng Postman.

> **Ghi chú bổ sung:** Swagger (OpenAPI) sinh tài liệu tự động từ metadata của Controller/Action. Các API có `[Authorize]` sẽ yêu cầu header `Authorization: Bearer <token>` — nhưng Swagger UI không tự hiểu cần nhập gì nếu không có **Security Scheme** được khai báo. Cấu hình `AddSecurityDefinition` + `AddSecurityRequirement` chính là "dạy" Swagger UI hiển thị nút **Authorize** ở góc phải và đính kèm token vào mọi request test.

### 1.2. CORS là gì và vì sao cần?

**CORS (Cross-Origin Resource Sharing)** là **cơ chế bảo mật của trình duyệt**, ngăn các trang web tự động gọi API từ một **domain khác** (cross-origin).

**Origin** bao gồm ba phần: **Scheme (http/https) + Host (domain.com) + Port (80/443)**.

> **Origin = SCHEME + HOST + PORT**
>
> Ví dụ: `http://localhost:3000` và `http://localhost:5000` là **hai origin khác nhau** (vì port khác). `https://localhost:3000` và `http://localhost:3000` cũng khác (vì scheme khác).

Khi thiếu CORS headers, trình duyệt sẽ **chặn response** và báo lỗi trong Console.

**Vì sao cần CORS?**

- API backend thường chạy ở `http://localhost:5000`, trong khi Frontend (React/Angular/Vue) chạy ở `http://localhost:3000` — đây là **hai origin khác nhau**.
- Trình duyệt sẽ **chặn request** từ Frontend trừ khi Backend trả header `Access-Control-Allow-Origin` cho phép.
- CORS bảo vệ người dùng khỏi các cuộc tấn công **đọc chéo trái phép** (Cross-Site Request Forgery — CSRF, Cross-Site Scripting — XSS).

> **Ghi chú bổ sung — Same-Origin Policy (SOP):** CORS thực chất là **nới lỏng** của SOP. SOP là chính sách mặc định của trình duyệt: **JavaScript chạy ở origin A KHÔNG được đọc response từ origin B**. CORS là cơ chế để origin B **cho phép tường minh** origin A truy cập. Cơ chế hoạt động:
>
> - Request **đơn giản** (GET/POST với content-type đơn giản): browser gửi request kèm header `Origin: http://localhost:3000`. Server phải trả `Access-Control-Allow-Origin: http://localhost:3000` (hoặc `*`) thì browser mới cho JS đọc response.
> - Request **phức tạp** (PUT/DELETE, custom header, `application/json`): browser tự gửi **preflight request** (OPTIONS) trước để hỏi server có cho phép không. Server phải trả các header `Access-Control-Allow-Methods`, `Access-Control-Allow-Headers`, và `Access-Control-Max-Age`.

> **Lưu ý quan trọng:** CORS là **bảo vệ của Trình duyệt**, không phải của Backend. Nghĩa là dùng **Postman/curl vẫn gọi được API** dù không cấu hình CORS — vì các công cụ này không phải browser, không thực thi SOP. Đừng nhầm CORS với Authentication/Authorization.

### 1.3. Publish là gì?

**Publish** là quá trình **biên dịch mã nguồn**, **gom các thư viện (.dll), cấu hình và tài nguyên tĩnh** thành một **bộ thư mục duy nhất** sẵn sàng để chạy trên server.

**Các chế độ Publish:**

| Chế độ | Viết tắt | Yêu cầu server | Kích thước |
|---|---|---|---|
| **Framework-dependent** | FDD | Phải cài sẵn .NET Runtime | Nhỏ (~vài MB) |
| **Self-contained** | SCD | Không cần cài .NET | Lớn (~60-150MB) |

> **Ghi chú bổ sung:** Ngoài FDD/SCD, còn có **Single-file publish** (`-p:PublishSingleFile=true`) — gộp tất cả `.dll` vào **một file `.exe` duy nhất** (tiện copy/deploy). Và **Trimmed** (`-p:PublishTrimmed=true`) — loại bỏ code không dùng để giảm dung lượng (chỉ hiệu quả với SCD, đôi khi gây lỗi reflection).

### 1.4. Docker là gì?

**Docker** là nền tảng **ảo hóa** cho phép đóng gói ứng dụng và tất cả **dependencies** (runtime, thư viện, cấu hình) vào một đơn vị gọi là **Container**.

- **Container** chạy độc lập trên host OS, **chia sẻ kernel** với máy chủ nên **nhẹ** và **khởi động nhanh**.
- **Docker Image** là **bản template bất biến**.
- **Container** là **instance chạy** của Image.

**Tại sao dùng Docker?**

- Giải quyết bài toán kinh điển **"It works on my machine"** — chạy được trên máy dev thì sẽ chạy được trên server.
- Container **nhẹ hơn VM** nhiều (không cần Guest OS).
- **Khởi động tính bằng giây** (VM tính bằng phút).
- Đảm bảo **môi trường đồng nhất** từ dev → staging → production.

> **Ghi chú bổ sung — Container vs VM:**
>
> | Tiêu chí | Container | Virtual Machine |
> |---|---|---|
> | Ảo hóa | Mức OS (chia sẻ kernel host) | Mức phần cứng (có kernel riêng) |
> | Kích thước | Vài chục MB → vài trăm MB | Vài GB |
> | Khởi động | Giây | Phút |
> | Cô lập | Mức process (yếu hơn) | Mức phần cứng (mạnh hơn) |
> | Mật độ | Hàng chục → hàng trăm/host | Vài chục/host |
>
> **Kiến trúc Docker:** `Dockerfile` (recipe) → `docker build` → `Image` (bất biến) → `docker run` → `Container` (chạy). Image được lưu trong **Registry** (Docker Hub, Azure Container Registry). Các layer của Image được **cache** — build lần 2 nhanh hơn nhiều nếu Dockerfile không đổi.

### 1.5. So sánh các phương thức triển khai

| Phương thức | Ưu điểm | Nhược điểm | Khi nào dùng |
|---|---|---|---|
| **IIS** | Có sẵn trên Windows Server, quản lý GUI | Chỉ Windows, cần Hosting Bundle | Doanh nghiệp Windows |
| **Docker** | Đa nền tảng, đồng nhất, dễ scale | Cần học Docker, overhead nhẹ | Cloud, microservices, CI/CD |
| **Kestrel trực tiếp** | Đơn giản nhất, không cần IIS | Thiếu reverse proxy (SSL termination, static file cache) | Dev, demo nội bộ |
| **Nginx/Apache + Kestrel** | Reverse proxy mạnh, đa nền tảng | Cấu hình phức tạp hơn | Linux production |

## 2. Lý thuyết chính

### 2.1. Swagger nâng cao — Cấu hình JWT

Trong `Program.cs`, dùng `AddSwaggerGen` với 2 phần:

**Phần 1 — Khai báo Security Scheme `Bearer`:**

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "Nhập JWT token (Swagger sẽ tự thêm tiền tố Bearer).",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});
```

**Phần 2 — Yêu cầu Security Scheme cho toàn bộ API:**

```csharp
c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
        new List<string>()
    }
});
```

> **Ghi chú bổ sung — Điểm cực kỳ quan trọng dễ sai:**
>
> - `Type = SecuritySchemeType.Http` + `Scheme = "bearer"` → Swagger UI sẽ **tự thêm prefix `Bearer `** khi gửi token. Bạn chỉ cần **paste token thô** vào ô input.
> - Nếu dùng `Type = SecuritySchemeType.ApiKey` + `Name = "Authorization"` + `In = ParameterLocation.Header` (cách cũ phổ biến trên mạng), Swagger **KHÔNG tự thêm prefix** → bạn phải gõ `Bearer eyJhbGci...` (kèm chữ Bearer). Đây là lỗi 401 rất phổ biến khi copy code cũ.
> - `Id = "Bearer"` ở `AddSecurityRequirement` **phải khớp** với tên `"Bearer"` ở `AddSecurityDefinition`.

### 2.2. CORS — Cấu hình trong ASP.NET Core

**Cấu hình CORS Policy trong `Program.cs`:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Chỉ cho phép domain này
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**Đăng ký trong Pipeline:**

```csharp
app.UseCors("AllowFrontend");
// Phải đặt sau UseRouting và trước UseAuthorization
```

> **Ghi chú bổ sung:**
>
> **Thứ tự chuẩn trong pipeline:**
> ```text
> app.UseRouting();
> app.UseCors("AllowFrontend");    ← phải ở giữa
> app.UseAuthentication();
> app.UseAuthorization();
> app.MapControllers();
> ```
>
> **Lý do:** `UseCors` phải chạy **sau** `UseRouting` để biết endpoint nào áp dụng policy nào. Phải chạy **trước** `UseAuthentication`/`UseAuthorization` để xử lý **preflight OPTIONS request** (browser gửi OPTIONS không kèm token — nếu auth chạy trước sẽ trả 401 và browser báo lỗi CORS).
>
> **Các method cấu hình khác:**
>
> | Method | Ý nghĩa |
> |---|---|
> | `.WithOrigins("https://a.com", "https://b.com")` | Whitelist domain cụ thể (khuyến nghị) |
> | `.AllowAnyOrigin()` | Cho phép **mọi** domain (`*`) — **KHÔNG dùng ở Production** |
> | `.SetIsOriginAllowed(origin => true)` | Cho phép động (dùng khi cần whitelist theo config) |
> | `.AllowAnyHeader()` | Cho phép mọi header từ client |
> | `.AllowAnyMethod()` | Cho phép GET, POST, PUT, DELETE, PATCH... |
> | `.AllowCredentials()` | Cho phép gửi cookie/token qua cross-origin |
> | `.WithExposedHeaders("X-Total-Count")` | Cho JS đọc các header response này |
>
> **Lưu ý:** `AllowAnyOrigin()` và `AllowCredentials()` **loại trừ nhau** — browser không cho phép `*` + credentials. Nếu cần cả hai, phải dùng `.SetIsOriginAllowed(_ => true)`.

### 2.3. Publish — Chế độ và lệnh CLI

**Framework-dependent (FDD) — mặc định:**

```bash
dotnet publish -c Release -o ./publish
```

**Self-contained (SCD) — kèm .NET Runtime:**

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

Có thể dùng **Visual Studio** (chuột phải Project → Publish → chọn Folder/IIS).

> **Ghi chú bổ sung:**
>
> - `-c Release`: build ở chế độ Release (bật optimization, bỏ debug symbol) — **bắt buộc** khi deploy production. Mặc định `dotnet publish` đã là `Release`, nhưng nên ghi rõ.
> - `-o ./publish`: thư mục output.
> - `-r win-x64` (Runtime Identifier): target OS/arch. Các RID phổ biến: `win-x64`, `linux-x64`, `linux-arm64`, `osx-arm64`.
> - `--self-contained false` (hoặc bỏ trống khi đã dùng FDD): không kèm runtime.
> - `-p:PublishSingleFile=true`: gộp thành 1 file `.exe` duy nhất.
> - `-p:PublishReadyToRun=true`: biên dịch trước một phần để khởi động nhanh hơn.
>
> **Cấu trúc thư mục sau publish (FDD):**
> ```text
> publish/
> ├── MyApi.dll               ← entry point
> ├── MyApi.exe               ← launcher (Windows)
> ├── appsettings.json
> ├── appsettings.Production.json
> ├── MyApi.deps.json        ← dependency manifest
> ├── MyApi.runtimeconfig.json
> ├── *.dll                   ← các thư viện
> └── wwwroot/                ← static files (nếu có)
> ```
>
> **Chạy sau publish:**
> ```bash
> dotnet MyApi.dll
> # hoặc
> ./MyApi.exe
> ```
>
> **Lưu ý về môi trường:** Sau publish, `ASPNETCORE_ENVIRONMENT` mặc định là `Production`. Nhớ set biến môi trường nếu cần chạy `Development`/`Staging`:
> ```bash
> # Windows
> set ASPNETCORE_ENVIRONMENT=Production
> # Linux
> export ASPNETCORE_ENVIRONMENT=Production
> ```

### 2.4. Triển khai lên IIS

**IIS (Internet Information Services)** là Web Server của Windows.

Để IIS chạy được ASP.NET Core, cần cài **.NET Core Hosting Bundle** trên server. Hosting Bundle cài **ASP.NET Core Module (ANCM)** — một module của IIS chịu trách nhiệm **reverse proxy** request từ IIS sang tiến trình `dotnet` (Kestrel).

> **Ghi chú bổ sung — Kiến trúc IIS + ASP.NET Core:**
>
> ```text
> Client → IIS (port 80/443) → ASP.NET Core Module (ANCM) → Kestrel (port nội bộ) → App
> ```
>
> IIS **không** trực tiếp chạy code .NET như với ASP.NET MVC cũ (.NET Framework). Thay vào đó:
>
> 1. **IIS** nhận request, xử lý static file, SSL termination, compression...
> 2. **ANCM** chuyển tiếp request động sang tiến trình `dotnet` (thường gọi là **out-of-process hosting**). Từ .NET Core 3.0, mặc định là **in-process hosting** — app chạy **ngay trong** process `w3wp.exe` (nhanh hơn).
> 3. **Kestrel** xử lý và trả response ngược lại.
>
> **Các bước deploy lên IIS:**
> 4. Cài **.NET Core Hosting Bundle** trên server (tải từ Microsoft).
> 5. Mở **IIS Manager** → tạo **Application Pool** mới, đặt `.NET CLR Version = No Managed Code` (vì ANCM tự host .NET Core, IIS không cần load CLR).
> 6. Tạo **Website** trỏ tới thư mục đã publish, bind port/domain.
> 7. Cấp quyền **Read & Execute** cho user của Application Pool (thường là `IIS_IUSRS` hoặc `ApplicationPoolIdentity`) trên thư mục publish.
> 8. Khởi động website, test.

### 2.5. Triển khai bằng Docker

**Dockerfile mẫu cho ASP.NET Core 8 (multi-stage build):**

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApi.csproj", "./"]
RUN dotnet restore "MyApi.csproj"
COPY . .
RUN dotnet publish "MyApi.csproj" -c Release -o /app/publish

# Stage 2: Runtime (nhẹ hơn, không có SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

> **Ghi chú bổ sung:**
>
> - **Multi-stage build**: Stage `build` dùng SDK (~800MB) để compile; stage `runtime` chỉ dùng ASP.NET Runtime (~200MB) — **image cuối cùng nhỏ hơn nhiều**.
> - **Thứ tự COPY**: copy file `.csproj` trước, chạy `dotnet restore` → tận dụng **Docker layer cache**. Khi chỉ sửa code (không đổi package), bước `restore` được cache → build nhanh hơn.
> - **Port mặc định trong container .NET 8 là 8080** (không phải 80 như .NET 6 trước đây). Nếu muốn đổi, set `ASPNETCORE_URLS`.
> - **Lệnh build và chạy:**
> ```bash
> docker build -t myapi:1.0 .
> docker run -d -p 5000:8080 --name myapi myapi:1.0
> ```
> Truy cập `http://localhost:5000`.
> - **`docker-compose.yml`** (nếu cần chạy kèm DB):
> ```yaml
> services:
>   api:
>     build: .
>     ports: ["5000:8080"]
>     environment:
>       - ASPNETCORE_ENVIRONMENT=Production
>       - ConnectionStrings__Default=Server=db;...
>     depends_on: [db]
>   db:
>     image: mcr.microsoft.com/mssql/server:2022-latest
>     environment:
>       - SA_PASSWORD=YourStrong!Pass
>       - ACCEPT_EULA=Y
> ```

### 2.6. Ghi nhớ, Lỗi thường gặp & Best Practice

> **Ghi nhớ:**
>
> - **CORS là bảo vệ của Trình duyệt**, không phải của Backend (dùng Postman vẫn gọi được API dù không cấu hình CORS).
> - Cần cài **Hosting Bundle** nếu muốn chạy ASP.NET Core trên IIS.

> **Lỗi thường gặp:**
>
> - Cấu hình CORS nhưng đặt **sai vị trí trong Pipeline** (ví dụ: đặt sau `UseAuthorization`).
> - Lỗi **HTTP Error 500.19** trên IIS do thiếu Hosting Bundle hoặc sai quyền truy cập thư mục.

> **Best Practice:**
>
> - **Bảo mật CORS:** Không dùng `AllowAnyOrigin()` trên môi trường Production. Chỉ cho phép các domain cụ thể của Frontend.
> - **Quản lý cấu hình:** Khi Publish, không lưu các thông tin nhạy cảm (Connection String thật) trong `appsettings.json`; nên dùng **Environment Variables** hoặc **User Secrets** (Development). Dùng `appsettings.Production.json` riêng hoặc **Secret Manager** cho từng môi trường.
> - **Swagger:** Có thể **ẩn Swagger UI** trên môi trường Production để bảo mật.

> **Ghi chú bổ sung — Ẩn Swagger ở Production:**
>
> ```csharp
> if (app.Environment.IsDevelopment())
> {
>     app.UseSwagger();
>     app.UseSwaggerUI();
> }
> ```
>
> **Lý do:** Swagger UI phơi bày **toàn bộ cấu trúc API** (endpoint, schema, version) — hacker có thể dùng để dò lỗ hổng. Nếu cần dùng ở Production (cho đối tác), hãy bảo vệ bằng **authentication** hoặc **IP whitelist**.

## 3. Ví dụ nền tảng

### 3.1. Cấu hình Swagger JWT hoàn chỉnh

```csharp
using Microsoft.OpenApi.Models;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Khai báo Security Scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token (Swagger sẽ tự thêm tiền tố Bearer).",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Yêu cầu áp dụng cho toàn bộ API
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
            new List<string>()
        }
    });
});

// ...

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Cách dùng trên Swagger UI:**

1. Đăng nhập qua endpoint `/api/auth/login` → lấy token.
2. Bấm nút **Authorize** (ổ khóa góc phải).
3. Dán **token thô** (không kèm chữ `Bearer`) vào ô → bấm **Authorize**.
4. Từ giờ các API có `[Authorize]` sẽ tự động đính kèm header `Authorization: Bearer <token>`.

### 3.2. Cấu hình CORS hoàn chỉnh

```csharp
// 1. Đăng ký service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",       // React dev
                "https://myapp.example.com"    // Production frontend
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 2. Đăng ký middleware — đúng thứ tự
app.UseRouting();
app.UseCors("AllowFrontend");       // ← SAU Routing, TRƯỚC Auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### 3.3. Lệnh Publish

```bash
# FDD (Framework-dependent) — nhỏ, cần .NET Runtime trên server
dotnet publish -c Release -o ./publish

# SCD (Self-contained) — lớn, không cần cài .NET
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Self-contained + single file — 1 file .exe duy nhất
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o ./publish

# Linux + Docker (không cần cài .NET)
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish
```

### 3.4. Dockerfile mẫu (xem mục 2.5)

## 4. Thực hành / Lab cơ bản

### 4.1. Publish ứng dụng

1. Cấu hình CORS cho phép origin `http://localhost:3000` gọi API.
2. **Publish ứng dụng** ra một thư mục cục bộ bằng dòng lệnh.
3. Cấu hình Swagger để nhập và gửi JWT Token cho các API yêu cầu xác thực.
4. Viết file `Dockerfile` cho dự án của bạn (nếu đã cài Docker).
5. Cài đặt **IIS** (trên Windows), cấu hình và deploy API vừa publish lên IIS. Kiểm tra API qua trình duyệt.

### 4.2. Kiểm thử API

**Sau khi deploy, kiểm tra các bước sau:**

1. **Mở Swagger UI:** `http://<server>/swagger` (nếu bật ở môi trường hiện tại).
2. **Test CORS:** Mở DevTools → Console của Frontend `localhost:3000`, gọi API → không còn lỗi CORS.
3. **Test JWT trên Swagger:** Đăng nhập → Authorize → gọi API có `[Authorize]` → trả 200.
4. **Test 401/403:** Gọi API có `[Authorize]` mà không nhập token → 401. Gọi API yêu cầu Role `Admin` bằng token `User` → 403.

## 5. Bài tập mở rộng

### 5.1. Bài thực hành

---

#### Bài 1: Cấu hình CORS cho phép origin `http://localhost:3000`

**Yêu cầu:** Cấu hình CORS cho phép origin `http://localhost:3000` gọi API.

**Giải thích chi tiết:**

- **Bước 1:** Đăng ký CORS service bằng `AddCors` — khai báo policy có tên (VD: `"AllowFrontend"`).
- **Bước 2:** Đăng ký middleware `UseCors("AllowFrontend")` **đúng vị trí** (sau `UseRouting`, trước `UseAuthentication`).
- **Bước 3:** Test bằng cách tạo một trang HTML đơn giản chạy ở port 3000, gọi `fetch` tới API → mở DevTools thấy response OK, không có lỗi CORS.

**Code đầy đủ:**

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");       // ← đúng vị trí
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Cách test:**

```html
<!-- test.html, serve ở http://localhost:3000 -->
<script>
fetch('http://localhost:5000/api/products')
  .then(r => r.json())
  .then(d => console.log('OK:', d))
  .catch(e => console.error('FAIL:', e));
</script>
```

> **Kiểm chứng sai vị trí:** Thử đặt `app.UseCors()` **sau** `app.UseAuthorization()` → preflight OPTIONS request sẽ bị chặn với 401/403 → browser báo lỗi CORS.

---

#### Bài 2: Publish ứng dụng ra thư mục cục bộ

**Yêu cầu:** Publish ứng dụng ra một thư mục cục bộ bằng dòng lệnh.

**Giải thích chi tiết:**

- Mở terminal tại thư mục project (chứa file `.csproj`).
- Chạy lệnh publish với `-c Release`.
- **Lưu ý:** Trước khi publish, kiểm tra `appsettings.json` **không chứa secret thật** — nếu có, chuyển sang biến môi trường.
- **Lưu ý:** `wwwroot/` (static files) được copy tự động vào thư mục publish.

**Lệnh:**

```bash
# FDD — nhỏ, cần .NET 8 Runtime trên server
dotnet publish -c Release -o ./publish

# Kiểm tra cấu trúc
ls publish/
# MyApi.dll  MyApi.exe  appsettings.json  *.dll  wwwroot/
```

**Kiểm chứng:**

```bash
# Chạy thử ở local
cd publish
dotnet MyApi.dll
# Hoặc
./MyApi.exe
```

**Mở trình duyệt:** `http://localhost:5000/swagger` (nếu đang Development).

> **Bẫy thường gặp:** Quên `-c Release` → file `.pdb` (debug symbol) được copy vào publish → tăng dung lượng, có thể lộ source code. Hoặc quên copy `appsettings.Production.json` khi deploy → app crash vì thiếu config.

---

#### Bài 3: Cấu hình Swagger để nhập và gửi JWT Token

**Yêu cầu:** Cấu hình Swagger để nhập và gửi JWT Token cho các API yêu cầu xác thực.

**Giải thích chi tiết:**

- Cần **2 phần** cấu hình: `AddSecurityDefinition` (khai báo kiểu auth) + `AddSecurityRequirement` (áp dụng cho toàn bộ API).
- **Dùng `SecuritySchemeType.Http` + `Scheme = "bearer"`** (cách mới) — Swagger tự thêm prefix `Bearer`, người dùng chỉ cần paste token thô.
- **Tránh `ApiKey` + `Name = "Authorization"`** (cách cũ) — dễ nhầm, người dùng phải gõ `Bearer eyJ...` thủ công.

**Code đầy đủ:** xem mục 3.1.

**Cách dùng trên Swagger UI:**

1. Mở Swagger UI.
2. Gọi API `POST /api/auth/login` với `{ "username": "admin", "password": "123" }`.
3. Copy token trong response (chuỗi `eyJhbGci...`).
4. Bấm nút **Authorize** (ổ khóa).
5. Dán token **thô** (không kèm chữ `Bearer`) → Authorize.
6. Gọi API có `[Authorize]` → nhận 200 (nếu token hợp lệ).

> **Debug khi Swagger vẫn 401:**
>
> - Mở DevTools → Network → xem request có header `Authorization: Bearer <token>` chưa.
> - Nếu thiếu prefix `Bearer` → bạn đang dùng `ApiKey` thay vì `Http`.
> - Nếu token có prefix `Bearer Bearer eyJ...` → bạn đã paste kèm chữ Bearer, hãy xóa và paste lại token thô.

---

#### Bài 4: Viết Dockerfile cho dự án

**Yêu cầu:** Viết file `Dockerfile` cho dự án của bạn (nếu đã cài Docker).

**Giải thích chi tiết:**

- Dùng **multi-stage build** để image nhỏ gọn.
- **Stage 1 (`build`)**: dùng SDK image, restore + publish.
- **Stage 2 (`runtime`)**: dùng ASP.NET runtime image, copy output từ stage 1.
- **Đặt `ENV ASPNETCORE_URLS=http://+:8080`** — port mặc định của .NET 8 container.
- **`EXPOSE 8080`** chỉ là documentation — map port thật qua `docker run -p`.
- **Thêm `.dockerignore`** để loại `bin/`, `obj/`, `.vs/`, `*.user` — tránh copy rác vào build context.

**Code đầy đủ:** xem mục 2.5.

**.dockerignore mẫu:**

```text
bin/
obj/
.vs/
.vscode/
*.user
*.suo
appsettings.Development.json
```

**Lệnh build và chạy:**

```bash
docker build -t myapi:1.0 .
docker run -d -p 5000:8080 --name myapi myapi:1.0
curl http://localhost:5000/weatherforecast
```

**Kiểm tra image:**

```bash
docker images myapi
# REPOSITORY  TAG   SIZE
# myapi       1.0   ~220MB
```

> **Bẫy:** Nếu quên `ASPNETCORE_URLS` hoặc `ASPNETCORE_HTTP_PORTS`, app chỉ listen `localhost:5000` bên trong container → từ ngoài không truy cập được dù đã map `-p 5000:8080`.

---

#### Bài 5: Deploy lên IIS

**Yêu cầu:** Cài đặt IIS (trên Windows), cấu hình và deploy API vừa publish lên IIS. Kiểm tra API qua trình duyệt.

**Giải thích chi tiết:**

**Bước 1 — Cài Hosting Bundle:**

- Tải **.NET Core Hosting Bundle** từ Microsoft (chứa .NET Runtime + ASP.NET Core Module).
- Cài xong **restart IIS** (`iisreset` trong CMD admin).

**Bước 2 — Tạo Application Pool:**

- Mở **IIS Manager** (`inetmgr`).
- Tạo Application Pool mới:
  - **.NET CLR Version:** `No Managed Code` (vì ANCM tự host .NET Core).
  - **Managed pipeline mode:** `Integrated`.
  - **Identity:** `ApplicationPoolIdentity` (mặc định).

**Bước 3 — Tạo Website:**

- Chuột phải **Sites** → **Add Website**.
- **Physical path:** trỏ tới thư mục đã publish.
- **Port:** 80 (hoặc 8080 nếu port 80 đã dùng).
- **Application Pool:** chọn pool vừa tạo.

**Bước 4 — Cấp quyền thư mục:**

- Chuột phải thư mục publish → **Properties** → **Security** → **Edit** → **Add**.
- Thêm user `IIS AppPool\<TênPool>` (VD: `IIS AppPool\MyApiPool`).
- Cấp quyền **Read & Execute** + **Write** (nếu app cần ghi file — VD: upload).

**Bước 5 — Kiểm tra:**

- Mở trình duyệt: `http://localhost/weatherforecast` hoặc `http://localhost/swagger`.
- Nếu báo **HTTP Error 500.19** → thiếu Hosting Bundle hoặc cấu hình `web.config` sai.
- Nếu báo **HTTP Error 500.30** → app crash khi khởi động (xem Event Viewer → Windows Logs → Application).

**Kiểm chứng nâng cao:**

- Mở **Event Viewer** → **Windows Logs** → **Application** → filter theo `Source = IIS-ASP.NET Core Module` → xem log lỗi chi tiết.
- Bật **stdout logging** trong `web.config` (nếu cần debug): `<aspNetCore stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />` — log sẽ ghi vào `logs\stdout_*.log`.

> **Bẫy phổ biến:**
>
> - **500.19**: `web.config` có section không được cài module (thường do thiếu ANCM sau khi cài Hosting Bundle).
> - **500.30**: App crash — nguyên nhân thường do `appsettings.json` thiếu key bắt buộc, hoặc connection string sai.
> - **403.14**: Chưa bật **Directory Browsing** hoặc `web.config` không tìm thấy `MyApi.dll`.
> - **502.5**: Tiến trình `dotnet` bị kill — kiểm tra quyền user pool trên thư mục publish.

## 6. Bảng thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| **Swagger / OpenAPI** | Chuẩn mô tả REST API. Swagger UI là giao diện test API sinh từ metadata. |
| **Security Scheme** | Khai báo kiểu xác thực (Bearer, ApiKey, OAuth2...) cho Swagger UI. |
| **`AddSecurityDefinition`** | Method đăng ký Security Scheme với Swagger. |
| **`AddSecurityRequirement`** | Method yêu cầu Security Scheme áp dụng cho API. |
| **CORS** | Cross-Origin Resource Sharing — cơ chế **trình duyệt** cho phép cross-origin request. |
| **Origin** | Bộ ba `Scheme + Host + Port` — đơn vị so sánh same-origin. |
| **Same-Origin Policy (SOP)** | Chính sách mặc định của browser: chặn JS đọc response từ origin khác. |
| **Preflight Request** | Request `OPTIONS` browser tự gửi để hỏi server có cho phép cross-origin. |
| **`Access-Control-Allow-Origin`** | Header server trả để cho phép origin cụ thể. |
| **`UseCors`** | Middleware đăng ký CORS policy vào pipeline. |
| **Publish** | Quá trình build + đóng gói app thành bộ thư mục chạy được trên server. |
| **FDD** | Framework-dependent — publish không kèm runtime, cần server cài .NET. |
| **SCD** | Self-contained — publish kèm runtime, không cần server cài .NET. |
| **RID** | Runtime Identifier — target OS/arch (VD: `win-x64`, `linux-x64`). |
| **IIS** | Internet Information Services — Web Server của Windows. |
| **Hosting Bundle** | Gói cài đặt .NET Runtime + ASP.NET Core Module (ANCM) cho IIS. |
| **ANCM** | ASP.NET Core Module — module IIS reverse proxy request sang Kestrel. |
| **Kestrel** | Web server cross-platform mặc định của ASP.NET Core. |
| **Application Pool** | Nhóm process độc lập trong IIS — cô lập app, tự restart khi crash. |
| **Docker** | Nền tảng đóng gói app vào container — nhẹ, đồng nhất, dễ deploy. |
| **Docker Image** | Template bất biến chứa filesystem + metadata để tạo container. |
| **Container** | Instance đang chạy của Image — cô lập process, chia sẻ kernel host. |
| **Dockerfile** | File recipe mô tả cách build Image. |
| **Multi-stage Build** | Kỹ thuật dùng nhiều `FROM` trong 1 Dockerfile — build dùng SDK, runtime dùng image nhỏ. |
| **Registry** | Kho lưu Image (Docker Hub, ACR, ECR...). |
| **`ASPNETCORE_ENVIRONMENT`** | Biến môi trường quyết định môi trường chạy (Development/Staging/Production). |
| **`ASPNETCORE_URLS`** | Biến môi trường chỉ định URL Kestrel lắng nghe (VD: `http://+:8080`). |
| **Reverse Proxy** | Server trung gian nhận request từ client, chuyển tiếp tới app backend (IIS, Nginx). |
| **SSL Termination** | Reverse proxy giải mã HTTPS, chuyển tiếp HTTP tới backend — giảm tải backend. |

## 7. Cheatsheet

### 7.1. Thứ tự Pipeline chuẩn (đầy đủ)

```text
1.  GlobalExceptionMiddleware       ← bắt lỗi đầu tiên
2.  HttpsRedirection / HSTS
3.  StaticFiles                     ← phục vụ wwwroot
4.  Routing                         ← bắt buộc TRƯỚC UseCors
5.  Cors                            ← SAU Routing, TRƯỚC Auth
6.  Authentication                  ← xác thực
7.  Authorization                   ← phân quyền
8.  Custom Middleware (timing, audit...)
9.  Endpoints / Controllers
```

**Câu nhớ:** *"Exception → Https → Static → Routing → **CORS** → Auth → Authz → Controller"*

### 7.2. Swagger JWT — 2 khối code bắt buộc

```csharp
// Khối 1: Định nghĩa
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});

// Khối 2: Yêu cầu
c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
```

### 7.3. CORS — Bảng method

| Method | Dùng khi |
|---|---|
| `.WithOrigins("https://a.com")` | Whitelist domain (khuyến nghị) |
| `.AllowAnyOrigin()` | Chỉ dev — **KHÔNG dùng prod** |
| `.SetIsOriginAllowed(_ => true)` | Cần cả credentials + mọi origin |
| `.AllowAnyHeader()` | Cho phép mọi header client |
| `.AllowAnyMethod()` | Cho phép GET/POST/PUT/DELETE... |
| `.AllowCredentials()` | Cho phép cookie/auth cross-origin |
| `.WithExposedHeaders("X-Custom")` | Cho JS đọc header response này |

### 7.4. Lệnh Publish

```bash
# FDD (nhỏ, cần runtime)
dotnet publish -c Release -o ./publish

# SCD (lớn, không cần runtime)
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Single-file
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o ./publish

# Trimmed (giảm dung lượng)
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishTrimmed=true -o ./publish
```

### 7.5. Dockerfile + lệnh

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

```bash
docker build -t myapi:1.0 .
docker run -d -p 5000:8080 --name myapi myapi:1.0
docker logs -f myapi                 # xem log
docker exec -it myapi sh             # vào shell
docker compose up -d                 # nếu có compose file
```

### 7.6. IIS — Checklist deploy

- [ ] Cài **.NET Core Hosting Bundle** trên server.
- [ ] Restart IIS (`iisreset`).
- [ ] Tạo **Application Pool** với `.NET CLR Version = No Managed Code`.
- [ ] Tạo **Website** trỏ tới thư mục publish.
- [ ] Cấp quyền `IIS AppPool\<PoolName>` = **Read & Execute** (+ Write nếu cần).
- [ ] Test `http://localhost/` → phải trả JSON, không phải trang lỗi.
- [ ] Nếu 500.19 → kiểm tra Hosting Bundle + `web.config`.
- [ ] Nếu 500.30 → xem Event Viewer → Windows Logs → Application.

### 7.7. Bảng lỗi thường gặp

| Lỗi | Nguyên nhân | Khắc phục |
|---|---|---|
| **CORS preflight fails** | `UseCors` sai vị trí | Đặt sau `UseRouting`, trước `UseAuthentication` |
| **401 trên Swagger** | Paste token kèm chữ `Bearer` (khi dùng `Http`) | Paste token thô |
| **401 trên Swagger** | Dùng `ApiKey` thay vì `Http` | Đổi sang `Type = Http`, `Scheme = bearer` |
| **HTTP 500.19 (IIS)** | Thiếu Hosting Bundle / `web.config` sai | Cài Bundle, restart IIS |
| **HTTP 500.30 (IIS)** | App crash khi start | Xem Event Viewer → Application |
| **HTTP 502.5 (IIS)** | Process `dotnet` bị kill | Kiểm tra quyền user pool |
| **HTTP 403.14 (IIS)** | Không có `web.config` hoặc app không listen | Kiểm tra publish đủ file |
| **Docker container exit ngay** | Sai `ENTRYPOINT` hoặc thiếu file | `docker logs <container>` |
| **Docker không truy cập được** | Quên `ASPNETCORE_URLS` hoặc `-p` sai | Thêm `ENV ASPNETCORE_URLS=http://+:8080` |

### 7.8. Bảo mật — Checklist Production

- [ ] **Không** dùng `AllowAnyOrigin()` ở CORS.
- [ ] **Ẩn Swagger** UI ở Production (dùng `if (env.IsDevelopment())`).
- [ ] **Không** commit `appsettings.Production.json` chứa secret thật → dùng **Environment Variables** hoặc **Azure Key Vault / AWS Secrets Manager**.
- [ ] **Bật HTTPS** — dùng `app.UseHttpsRedirection()` + `app.UseHsts()`.
- [ ] **Giới hạn CORS origin** chỉ domain frontend thật.
- [ ] **Không trả stack trace** ra response ở Production (dùng `GlobalExceptionMiddleware` ẩn `Detail`).
- [ ] **Cấu hình `ASPNETCORE_ENVIRONMENT=Production`** trên server thật.
- [ ] **Bật rate limiting** cho API public.
- [ ] **Cấu hình request size limit** để chống DoS upload file lớn.

# Buổi 14: Thực hiện dự án tổng hợp, tích hợp các chức năng đã học

## Mục lục

```text
Buổi 14: Thực hiện dự án tổng hợp
│
├── 1. Lý thuyết nền tảng
│   ├── 1.1. Quy trình phát triển (SDLC thu gọn)
│   │   ├── 1.1.1. SDLC là gì?
│   │   ├── 1.1.2. Các giai đoạn SDLC thu gọn
│   │   └── 1.1.3. Áp dụng vào đồ án ASP.NET Core
│   ├── 1.2. Tích hợp các thành phần
│   │   ├── 1.2.1. Tích hợp là gì?
│   │   ├── 1.2.2. Các lớp tích hợp trong dự án
│   │   └── 1.2.3. Thứ tự tích hợp khuyến nghị
│   └── 1.3. Kiến trúc dự án
│       ├── 1.3.1. Cấu trúc 1 project (N-tier đơn giản)
│       └── 1.3.2. Cấu trúc nhiều project (Clean Architecture)
│
├── 2. Lý thuyết chính
│   ├── 2.1. Tổ chức nhóm và Git workflow
│   ├── 2.2. Checklist hoàn thành dự án
│   │   ├── 2.2.1. Phần 1 — Data & API
│   │   └── 2.2.2. Phần 2 — Security & Architecture
│   ├── 2.3. Tích hợp Frontend & Backend
│   ├── 2.4. Lỗi thường gặp khi tích hợp
│   └── 2.5. Best Practice
│
├── 3. Ví dụ nền tảng
│   ├── 3.1. Cấu trúc thư mục project mẫu
│   ├── 3.2. Mẫu Program.cs tích hợp đầy đủ
│   └── 3.3. Mẫu Repository + Service Pattern
│
├── 4. Thực hành / Lab cơ bản
│   ├── 4.1. Hoàn thiện dự án nhóm
│   ├── 4.2. Sửa lỗi
│   └── 4.3. Kiểm thử
│
├── 5. Bài tập mở rộng
│   └── 5.1. Bài thực hành (6 bài, giải chi tiết)
│
├── 6. Bảng thuật ngữ
│
└── 7. Cheatsheet
```

## 1. Lý thuyết nền tảng

### 1.1. Quy trình phát triển (SDLC thu gọn)

#### 1.1.1. SDLC là gì?

> **Ghi chú bổ sung (kiến thức bị thiếu trong file gốc):** File gốc chỉ ghi "Quy trình phát triển dự án (SDLC thu gọn)" mà không giải thích.

**SDLC (Software Development Life Cycle)** — Vòng đời phát triển phần mềm — là quy trình có cấu trúc mô tả các giai đoạn từ lúc **hình thành ý tưởng** đến khi **bàn giao và bảo trì** phần mềm.

**Mục đích:** Đảm bảo dự án được phát triển có kế hoạch, kiểm soát chất lượng, giảm rủi ro, và đáp ứng đúng yêu cầu người dùng.

#### 1.1.2. Các giai đoạn SDLC thu gọn

| Giai đoạn | Mục đích | Sản phẩm đầu ra |
|---|---|---|
| **1. Requirements** (Thu thập yêu cầu) | Hiểu rõ cần xây gì | Tài liệu yêu cầu, user stories |
| **2. Design** (Thiết kế) | Lên kiến trúc, CSDL, API contract | Sơ đồ ERD, API spec, wireframe |
| **3. Implementation** (Lập trình) | Viết code | Source code, database |
| **4. Testing** (Kiểm thử) | Đảm bảo code chạy đúng | Test report, bug list |
| **5. Deployment** (Triển khai) | Đưa lên server | Bản release chạy production |
| **6. Maintenance** (Bảo trì) | Sửa lỗi, nâng cấp | Bản vá, version mới |

> **Lưu ý:** Mô hình **Waterfall** (thác nước) chạy tuần tự 6 bước. Mô hình **Agile/Scrum** (linh hoạt) lặp lại các chu kỳ ngắn (sprint 1-4 tuần), mỗi sprint đều có đủ 6 giai đoạn thu nhỏ. Đồ án sinh viên thường dùng Agile thu gọn.

#### 1.1.3. Áp dụng vào đồ án ASP.NET Core

Quy trình thực tế cho đồ án môn học (4-6 tuần):

```text
Tuần 1:  Requirements + Design
         → Chốt đề tài, vẽ ERD, thiết kế API endpoints

Tuần 2:  Setup + Auth
         → Tạo project, cấu hình EF Core, làm Login/JWT

Tuần 3:  CRUD cơ bản
         → Làm các API chính (Products, Users, Orders...)

Tuần 4:  Nâng cao
         → Pagination, Filtering, Upload file, Exception Handling

Tuần 5:  Tích hợp
         → Ghép code các thành viên, fix conflict Git, test end-to-end

Tuần 6:  Deploy + Báo cáo
         → Publish, viết tài liệu, chuẩn bị demo
```

### 1.2. Tích hợp các thành phần

#### 1.2.1. Tích hợp là gì?

> **Ghi chú bổ sung (kiến thức bị thiếu trong file gốc):** File gốc chỉ có tiêu đề "Tích hợp các thành phần" mà không giải thích.

**Tích hợp (Integration)** là quá trình **ghép nối** các thành phần riêng lẻ (do các thành viên khác nhau phát triển, hoặc các module khác nhau) thành một **hệ thống hoàn chỉnh** hoạt động đồng bộ.

**Đặc điểm:**

- Mỗi thành viên làm một module (Auth, Products, Orders...) trên nhánh Git riêng.
- Tích hợp = **merge code** + **cấu hình DI chung** + **test chéo chức năng**.
- Đây là giai đoạn **dễ phát sinh lỗi nhất** trong dự án nhóm.

#### 1.2.2. Các lớp tích hợp trong dự án

| Lớp tích hợp | Cần làm gì | Rủi ro thường gặp |
|---|---|---|
| **Database** | Chạy migration chung, thống nhất schema | Migration xung đột giữa các nhánh |
| **DI Container** | Đăng ký tất cả service vào `Program.cs` | Quên `AddScoped` → lỗi runtime |
| **Middleware Pipeline** | Đăng ký đúng thứ tự | Đặt sai thứ tự → CORS/Auth fail |
| **API Contract** | Thống nhất route, DTO, mã lỗi | Frontend gọi sai route, sai schema |
| **Authentication** | Dùng chung JWT scheme | Mỗi module tự sinh token khác nhau |
| **Exception Handling** | Dùng chung `GlobalExceptionMiddleware` | Mỗi controller tự try-catch → lộn xộn |
| **Configuration** | Chia sẻ `appsettings.json` | Mỗi người một connection string |

#### 1.2.3. Thứ tự tích hợp khuyến nghị

> **Ghi chú bổ sung:** File gốc không nói thứ tự — đây là kinh nghiệm thực tế.

```text
Bước 1: Tích hợp DATABASE trước
        → Merge migrations, chạy update-database thành công

Bước 2: Tích hợp DI + Program.cs
        → Ghép tất cả AddScoped từ các nhánh

Bước 3: Tích hợp MIDDLEWARE
        → Đảm bảo thứ tự: Exception → CORS → Auth → Authz → Controller

Bước 4: Tích hợp CONTROLLER + SERVICE
        → Từng module một, test riêng từng module

Bước 5: Tích hợp END-TO-END
        → Gọi chuỗi API liên hoàn (đăng ký → login → lấy token → gọi API bảo mật)
```

> **Tại sao thứ tự này?** Database sai → mọi thứ trên đều fail. DI thiếu → app không start. Middleware sai → request không tới controller. Controller sai → không test được. Tích hợp từ dưới lên giúp **khoanh vùng lỗi** dễ hơn.

### 1.3. Kiến trúc dự án

#### 1.3.1. Cấu trúc 1 project (N-tier đơn giản)

**Khuyến nghị cho đồ án quy mô vừa và nhỏ:** Tổ chức một Web API project theo thư mục:

```text
Project.API/
├── Entities/          ← POCO class map với bảng CSDL
├── DTOs/              ← Request/Response models
├── Repositories/      ← Truy cập dữ liệu (EF Core)
├── Services/          ← Business logic
├── Controllers/       ← API endpoints
├── Middlewares/       ← Custom middleware
├── Data/              ← DbContext
├── Migrations/        ← EF Core migrations
└── Program.cs
```

#### 1.3.2. Cấu trúc nhiều project (Clean Architecture)

**Tùy chọn nâng cao:** Chia thành nhiều project khi nhóm đã nắm chắc kiến thức:

```text
Project.Domain/          ← Entities, Interface (không phụ thuộc ai)
Project.Infrastructure/  ← DbContext, Repositories, External services
Project.Application/     ← Services, DTOs, Business logic
Project.API/             ← Controllers, Middleware, Program.cs
```

**Quy tắc phụ thuộc (Dependency Rule):**

```text
API  →  Application  →  Domain
 ↓                          ↑
Infrastructure  ────────────┘
```

- `Domain` **không** phụ thuộc ai — là trung tâm.
- `Infrastructure` chỉ phụ thuộc `Domain`.
- `Application` phụ thuộc `Domain`.
- `API` phụ thuộc `Application` (và qua DI, phụ thuộc `Infrastructure`).

> **Lợi ích:** Đổi ORM (EF Core → Dapper) chỉ cần sửa `Infrastructure`, không đụng `Domain`/`Application`. Unit test dễ vì có thể mock interface.

## 2. Lý thuyết chính

### 2.1. Tổ chức nhóm và Git workflow

> **Ghi chú bổ sung:** File gốc chỉ ghi "Sử dụng Git/GitHub/GitLab" — không nói rõ **workflow** nào.

**Phân chia công việc rõ ràng:**

- Người làm **Auth** (đăng ký, đăng nhập, JWT, refresh token).
- Người làm **Sản phẩm** (CRUD Products, upload ảnh, filter/sort).
- Người làm **Đơn hàng** (CRUD Orders, OrderItems, tính tiền).
- Người làm **Frontend** (nếu có) hoặc **QA/Testing** (viết Postman collection).

**Git workflow khuyến nghị — Feature Branch:**

```bash
# Mỗi người tạo nhánh riêng từ main
git checkout -b feature/auth
git checkout -b feature/products
git checkout -b feature/orders

# Commit thường xuyên lên nhánh riêng
git add .
git commit -m "feat(auth): thêm API login sinh JWT"
git push origin feature/auth

# Khi xong → tạo Pull Request → review → merge vào develop
# Cuối kỳ: merge develop → main → tag v1.0
```

**Quy ước commit message (Conventional Commits):**

| Prefix | Ý nghĩa | Ví dụ |
|---|---|---|
| `feat` | Tính năng mới | `feat(products): thêm API tìm kiếm` |
| `fix` | Sửa lỗi | `fix(auth): sửa lỗi token hết hạn` |
| `docs` | Tài liệu | `docs: cập nhật README` |
| `refactor` | Tái cấu trúc | `refactor(order): tách service` |
| `chore` | Việc lặt vặt | `chore: cập nhật package` |

### 2.2. Checklist hoàn thành dự án

#### 2.2.1. Phần 1 — Data & API

**Cơ sở dữ liệu:**

- [ ] **Đã thiết kế đúng quan hệ 1-n, n-n?**
  - **1-n:** `User` (1) — `Order` (n). Mỗi user có nhiều đơn hàng.
  - **n-n:** `Product` ↔ `Category`. Một sản phẩm thuộc nhiều category, một category có nhiều sản phẩm → cần bảng trung gian `ProductCategory`.
  - **Kiểm tra:** Vẽ lại ERD, đảm bảo không có bảng "mồ côi", khóa ngoại đúng.

- [ ] **Đã sử dụng EF Core Migration?**
  - **Lý do:** Migration giúp version hóa schema CSDL, ai clone repo chỉ cần `dotnet ef database update` là có DB chạy được — không cần script SQL thủ công.
  - **Kiểm tra:** Chạy `dotnet ef migrations list` xem có migration chưa. Nếu chỉ có `EnsureCreated()`, hãy chuyển sang Migration.

**Cấu trúc API:**

- [ ] **Routing tuân thủ chuẩn RESTful (`/api/products`, `/api/products/{id}`)?**
  - **Danh từ số nhiều** (`products`, không phải `product`).
  - **Không dùng động từ trong URL** (`/api/getProducts` ❌, `/api/products` ✅).
  - **ID trên path** (`/api/products/5`), không trên query (`/api/products?id=5` ❌) cho tài nguyên đơn lẻ.

- [ ] **Sử dụng đúng HTTP Methods (GET, POST, PUT, DELETE)?**

  | Method | Mục đích | Trả về |
  |---|---|---|
  | `GET` | Đọc dữ liệu | 200 + data |
  | `POST` | Tạo mới | 201 + Location header |
  | `PUT` | Cập nhật toàn bộ | 204 hoặc 200 |
  | `PATCH` | Cập nhật một phần | 204 hoặc 200 |
  | `DELETE` | Xóa | 204 |

- [ ] **Validate dữ liệu đầu vào bằng Data Annotations?**
  - Ví dụ: `[Required]`, `[StringLength(100)]`, `[Range(1, 1000)]`, `[EmailAddress]`.
  - **FluentValidation** là lựa chọn nâng cao nếu nhóm tự tìm hiểu thêm — tách validation ra khỏi DTO, dễ test và tái sử dụng.

#### 2.2.2. Phần 2 — Security & Architecture

**Bảo mật:**

- [ ] **Tính năng Login sinh ra JWT Token thành công?**
  - Test: `POST /api/auth/login` với `admin/123` → trả JSON có `token`.
  - Decode tại `jwt.io` → thấy `sub`, `role`, `exp` đúng.

- [ ] **Các API cần thiết đã chặn `[Authorize]`?**
  - API thao tác dữ liệu nhạy cảm (tạo/sửa/xóa) phải có `[Authorize]`.
  - API công khai (xem danh sách) có thể `[AllowAnonymous]`.

- [ ] **Phân quyền (Role) Admin/User hoạt động đúng?**
  - Test: token Admin gọi API xóa → 200; token User gọi API xóa → 403.

**Architecture & Code Quality:**

- [ ] **Áp dụng DI, Repository Pattern và Service Pattern chưa?**
  - **DI:** Tất cả service/repository đăng ký trong `Program.cs`.
  - **Repository:** Tách truy cập dữ liệu khỏi Service — dễ đổi ORM, dễ mock test.
  - **Service:** Business logic nằm ở Service, Controller chỉ điều phối.

- [ ] **Đã có Global Exception Middleware để bắt lỗi thay vì sập app chưa?**
  - Test: gọi API cố tình throw exception → nhận JSON lỗi 500 chuẩn, không thấy stack trace.

- [ ] **Đã hỗ trợ phân trang (Pagination) cho các danh sách dài?**
  - Test: `GET /api/products?pageNumber=2&pageSize=10` → trả về `PagedResponse` có `totalPages`.

### 2.3. Tích hợp Frontend & Backend

> **Ghi chú bổ sung:** Đây là bước **hay bị bỏ qua** trong đồ án — chỉ test backend bằng Postman mà không test với frontend thật.

**Ba yêu cầu bắt buộc:**

1. **Cấu hình CORS** — Frontend mới gọi được API.
   - Backend phải `app.UseCors("AllowFrontend")` **đúng vị trí** (sau `UseRouting`, trước `UseAuthentication`).
   - Policy phải `WithOrigins("http://localhost:3000")` (origin frontend).

2. **Frontend lưu trữ JWT** — chọn một trong ba:

   | Nơi lưu | Ưu điểm | Nhược điểm |
   |---|---|---|
   | **LocalStorage** | Đơn giản, tồn tại qua tab | Dễ bị XSS đọc trộm |
   | **SessionStorage** | Mất khi đóng tab | Vẫn bị XSS |
   | **HttpOnly Cookie** | An toàn nhất (JS không đọc được) | Phức tạp, cần cấu hình CSRF |

3. **Frontend đính kèm Header `Authorization: Bearer <token>`** vào mỗi request gọi API bảo mật.

   **Mẫu với Axios (React):**

   ```javascript
   // Cấu hình interceptor tự động gắn token
   axios.interceptors.request.use((config) => {
     const token = localStorage.getItem('token');
     if (token) {
       config.headers.Authorization = `Bearer ${token}`;
     }
     return config;
   });
   ```

### 2.4. Lỗi thường gặp khi tích hợp

| Lỗi | Nguyên nhân | Cách khắc phục |
|---|---|---|
| **Circular Reference** khi `Include` | Entity A có navigation tới B, B có navigation ngược lại A → serializer đệ quy vô hạn | **Trả về DTO** thay vì Entity. Hoặc dùng `[JsonIgnore]` (không khuyến nghị). Hoặc cấu hình `ReferenceHandler.IgnoreCycles` trong `AddControllers().AddJsonOptions(...)`. |
| **Quên đăng ký DI** | Service `IProductService` được inject vào Controller nhưng chưa `AddScoped<IProductService, ProductService>()` trong `Program.cs` | Lỗi runtime: `InvalidOperationException: Unable to resolve service...`. Khắc phục: kiểm tra lại toàn bộ `AddScoped`/`AddTransient`/`AddSingleton`. |
| **Fat Controller** | Controller chứa cả validate, tính toán, gọi DB, gửi email... | Tách logic vào `Service`. Controller chỉ: nhận DTO → gọi Service → trả kết quả. Mục tiêu: mỗi action ≤ 10 dòng. |
| **Conflict Git** | Nhiều người cùng sửa 1 file (thường là `Program.cs`, `appsettings.json`, file migration) | Chia nhỏ file, pull thường xuyên, review trước khi merge. Riêng `Program.cs` có thể tách thành extension method (`AddAuthServices()`, `AddProductServices()`) để mỗi người sửa file riêng. |

> **Ghi chú bổ sung — Conflict Git trong migration:**
>
> Khi 2 người cùng tạo migration trên 2 nhánh, merge xong sẽ có **2 file migration cùng timestamp** hoặc **snapshot không khớp**. Cách xử lý:
> 1. Xóa migration của nhánh mình (`dotnet ef migrations remove`).
> 2. Pull nhánh kia về.
> 3. Chạy lại `dotnet ef migrations add TenMigrationMoi` trên code mới nhất.
> 4. Commit migration đã gộp.
>
> **Nguyên tắc:** **Không bao giờ merge 2 migration tự động** — phải viết lại migration trên code đã gộp.

### 2.5. Best Practice

> **Ghi chú bổ sung:** File gốc có 3 mục ngắn — mình mở rộng thêm cho rõ.

**Commit thường xuyên:**

- Viết xong chức năng nào → commit + push lên nhánh của mình ngay.
- **Tránh** gộp cuối tuần mới push — vì conflict sẽ cực lớn, không ai review nổi.
- **Quy tắc ngón tay cái:** commit khi hoàn thành một đơn vị logic (một API, một bug fix), không commit khi code chưa chạy được.

**Sử dụng DTO mọi lúc mọi nơi:**

- **Không bao giờ** trả trực tiếp Entity có `Include` các bảng liên quan ra ngoài API.
- **Lý do:** (1) Circular reference; (2) Lộ schema DB nội bộ; (3) Client có thể thấy field nhạy cảm (password hash, audit fields); (4) Khó version hóa API.
- **Pattern:** Entity ↔ DTO chuyển đổi qua AutoMapper hoặc extension method `ToDto()`.

**Cấu hình môi trường:**

- **Không push** `appsettings.json` có chứa chuỗi kết nối cá nhân lên GitHub.
- **Giải pháp:**
  - Dùng **User Secrets** cho dev (`dotnet user-secrets set "ConnectionStrings:Default" "..."`).
  - Dùng **Environment Variables** cho staging/production.
  - Thêm `appsettings.Development.json` vào `.gitignore` nếu chứa secret.
  - Thông báo cho nhóm biết cấu hình chung qua **kênh riêng** (Discord, Zalo) — không qua Git.

> **Best Practice bổ sung:**
>
> - **Viết README** cho repo: cách clone, cách cài, cách chạy, tài khoản test mẫu.
> - **Tạo Postman Collection** chung — mỗi API có test case mẫu, có `{{baseUrl}}` và `{{token}}` variable.
> - **Bật Swagger** ở Development để mọi thành viên tự test API của nhau.
> - **Viết unit test** cho Service layer — ít nhất cho các hàm tính toán quan trọng (tính giá đơn hàng, áp mã giảm giá).
> - **Dùng `.editorconfig`** để thống nhất format code (indent, dấu ngoặc, thứ tự using).

## 3. Ví dụ nền tảng

### 3.1. Cấu trúc thư mục project mẫu (N-tier đơn giản)

```text
ECommerce.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IProductService.cs
│   │   └── IOrderService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── ProductService.cs
│       └── OrderService.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   └── IProductRepository.cs
│   └── Implementations/
│       ├── Repository.cs
│       └── ProductRepository.cs
├── Entities/
│   ├── User.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   └── RegisterDto.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductDto.cs
│   │   └── ProductQueryParameters.cs
│   └── Common/
│       └── PagedResponse.cs
├── Middlewares/
│   └── GlobalExceptionMiddleware.cs
├── Data/
│   └── AppDbContext.cs
├── Migrations/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 3.2. Mẫu `Program.cs` tích hợp đầy đủ

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. DATABASE ==========
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ========== 2. DEPENDENCY INJECTION ==========
// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ========== 3. AUTHENTICATION (JWT) ==========
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey)
        };
    });

// ========== 4. AUTHORIZATION (Policy) ==========
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// ========== 5. CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ========== 6. OPTIONS PATTERN ==========
builder.Services.Configure<FileSettingsOptions>(
    builder.Configuration.GetSection("FileSettings"));

// ========== 7. CONTROLLERS + SWAGGER ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

var app = builder.Build();

// ========== PIPELINE (THỨ TỰ RẤT QUAN TRỌNG) ==========
app.UseMiddleware<GlobalExceptionMiddleware>();  // 1. ĐẦU TIÊN

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();           // cho upload file
app.UseRouting();
app.UseCors("AllowFrontend");    // SAU Routing, TRƯỚC Auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 3.3. Mẫu Repository + Service Pattern

**Repository (truy cập dữ liệu):**

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters p);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters p)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.Keyword))
            query = query.Where(x => x.Name.Contains(p.Keyword));

        if (p.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == p.CategoryId.Value);

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(x => x.Id)
            .Skip((p.PageNumber - 1) * p.PageSize)
            .Take(p.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Product>(data, total, p.PageNumber, p.PageSize);
    }
}
```

**Service (business logic):**

```csharp
public interface IProductService
{
    Task<PagedResponse<ProductDto>> GetPagedAsync(ProductQueryParameters p);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(ProductQueryParameters p)
    {
        var result = await _repo.GetPagedAsync(p);

        return new PagedResponse<ProductDto>
        {
            Data = result.Items.Select(_mapper.Map<ProductDto>),
            TotalRecords = result.Total,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Business rule: không cho trùng tên sản phẩm
        if (await _repo.ExistsAsync(x => x.Name == dto.Name))
            throw new BusinessException("Tên sản phẩm đã tồn tại.");

        var entity = _mapper.Map<Product>(dto);
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return _mapper.Map<ProductDto>(entity);
    }
}
```

**Controller (mỏng, chỉ điều phối):**

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] ProductQueryParameters p)
        => Ok(await _service.GetPagedAsync(p));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
```

## 4. Thực hành / Lab cơ bản

### 4.1. Hoàn thiện dự án nhóm

1. **Rà soát source code** hiện tại theo Checklist. Đánh dấu các phần chưa hoàn thành.
2. **Giải quyết các cảnh báo (Warnings)** trong Visual Studio.
3. **Ghép nối các API** của các thành viên.
4. **Xử lý conflict Git** nếu có.
5. **Tích hợp thử nghiệm** với Frontend (nếu có) hoặc mô phỏng gọi liên hoàn nhiều API qua Postman (dùng Postman Variables).

### 4.2. Sửa lỗi

**Các bước debug hiệu quả:**

1. **Đọc lỗi từ đầu** — không bỏ qua dòng đầu tiên.
2. **Tìm Exception đầu tiên** trong stack trace (dòng `at ...` đầu tiên).
3. **Google error message** kèm framework (VD: `"Unable to resolve service" ASP.NET Core`).
4. **Bật log chi tiết:** `appsettings.Development.json` → `LogLevel:Default = Debug`.
5. **Dùng breakpoint** thay vì `Console.WriteLine` để debug.
6. **Cô lập lỗi:** comment dần code để xác định dòng gây lỗi.

### 4.3. Kiểm thử

**Kiểm thử thủ công (Postman) — tạo Collection với biến:**

```text
Collection variables:
  baseUrl = http://localhost:5000
  token = (để trống)

Request 1: POST {{baseUrl}}/api/auth/login
  Body: { "username": "admin", "password": "123" }
  Tests: 
    pm.collectionVariables.set("token", pm.response.json().token);

Request 2: GET {{baseUrl}}/api/products
  Headers: Authorization = Bearer {{token}}
```

**Kiểm thử tự động (Unit test) — mẫu với xUnit:**

```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsBusinessException()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(true);
        var service = new ProductService(mockRepo.Object, Mock.Of<IMapper>());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(new CreateProductDto { Name = "Áo thun" }));
    }
}
```

## 5. Bài tập mở rộng

### 5.1. Bài thực hành

---

#### Bài 1: Rà soát dự án theo Checklist

**Yêu cầu:** Rà soát source code hiện tại theo Checklist (mục 2.2). Đánh dấu các phần chưa hoàn thành.

**Giải thích chi tiết:**

- Tạo file `CHECKLIST.md` trong repo, copy toàn bộ checklist từ mục 2.2.
- Mỗi mục dùng `- [x]` (đã xong) hoặc `- [ ]` (chưa xong).
- **Ưu tiên sửa:** Bảo mật > Kiến trúc > Tính năng.

**Mẫu `CHECKLIST.md`:**

```markdown
# Checklist hoàn thành dự án

## Data & API
- [x] Đã thiết kế đúng quan hệ 1-n, n-n
- [x] Đã sử dụng EF Core Migration
- [ ] Routing tuân thủ chuẩn RESTful — còn `/api/getProducts` cần đổi
- [x] Sử dụng đúng HTTP Methods
- [ ] Validate bằng Data Annotations — `CreateProductDto` chưa có `[Required]`

## Security & Architecture
- [x] Login sinh JWT thành công
- [x] Các API cần thiết đã chặn `[Authorize]`
- [x] Phân quyền Admin/User hoạt động
- [x] Áp dụng DI, Repository, Service
- [ ] Global Exception Middleware — chưa tạo
- [x] Phân trang cho danh sách dài
```

---

#### Bài 2: Giải quyết Warnings

**Yêu cầu:** Giải quyết các cảnh báo (Warnings) trong Visual Studio.

**Giải thích chi tiết:**

- Mở **Error List** → filter **Warnings** → **Build + IntelliSense**.
- **Các warning phổ biến:**

| Warning | Ý nghĩa | Cách sửa |
|---|---|---|
| `CS8618: Non-nullable property must contain a non-null value` | Property không nullable nhưng chưa khởi tạo | Thêm `= null!;` hoặc khởi tạo `= string.Empty;` hoặc thêm `?` |
| `CS8602: Dereference of a possibly null reference` | Có thể null | Dùng `?.` hoặc `if (x != null)` |
| `CS1998: Async method lacks 'await'` | Method `async` nhưng không `await` | Bỏ `async` hoặc thêm `await` |
| `NU1605: Detected package downgrade` | Package conflict | `dotnet list package --vulnerable` rồi update |

- **Nguyên tắc:** Không tắt warning bằng `#pragma warning disable` trừ khi có lý do chính đáng (comment giải thích).

---

#### Bài 3: Ghép nối API các thành viên

**Yêu cầu:** Cùng nhau ghép nối các API của các thành viên.

**Giải thích chi tiết:**

**Quy trình ghép nối an toàn (theo thứ tự mục 1.2.3):**

1. **Tạo nhánh `develop`** trên Git — nhánh chung để tích hợp.

   ```bash
   git checkout main
   git checkout -b develop
   git push origin develop
   ```

2. **Mỗi thành viên merge nhánh mình vào `develop`** (qua Pull Request).

   ```bash
   # Trên nhánh feature/auth
   git checkout feature/auth
   git pull origin develop        # cập nhật develop mới nhất
   git merge develop              # merge develop vào nhánh mình
   # Fix conflict nếu có
   git push origin feature/auth
   # Tạo PR: feature/auth → develop
   ```

3. **Test sau mỗi lần merge:**
   - App có build được không? (`dotnet build`)
   - App có start được không? (`dotnet run`)
   - Swagger có load được không?
   - Gọi thử 1 API cơ bản (login).

4. **Chỉ khi `develop` xanh** (build OK + test pass) mới merge tiếp nhánh khác.

---

#### Bài 4: Xử lý conflict Git

**Yêu cầu:** Xử lý conflict Git nếu có.

**Giải thích chi tiết:**

**Các bước xử lý conflict:**

1. **Pull/merge** → Git báo conflict:

   ```bash
   git merge develop
   # CONFLICT (content): Merge conflict in Program.cs
   ```

2. **Mở file conflict** — thấy các marker:

   ```csharp
   <<<<<<< HEAD
   builder.Services.AddScoped<IAuthService, AuthService>();
   =======
   builder.Services.AddScoped<IProductService, ProductService>();
   >>>>>>> develop
   ```

3. **Giải quyết:** **Giữ cả hai** nếu cả hai cần thiết:

   ```csharp
   builder.Services.AddScoped<IAuthService, AuthService>();
   builder.Services.AddScoped<IProductService, ProductService>();
   ```

   **Xóa các marker** `<<<<<<<`, `=======`, `>>>>>>>`.

4. **Test lại code** — conflict merge xong phải build được.

5. **Commit kết quả merge:**

   ```bash
   git add Program.cs
   git commit -m "merge: gộp DI của auth và product"
   git push
   ```

**Phòng tránh conflict trong tương lai:**

- **Tách `Program.cs` thành extension methods:**

  ```csharp
  // Program.cs (gọn)
  builder.Services.AddAuthServices(builder.Configuration);
  builder.Services.AddProductServices();
  builder.Services.AddOrderServices();

  // Extensions/AuthServiceExtensions.cs — nhánh auth sửa file này
  public static class AuthServiceExtensions
  {
      public static IServiceCollection AddAuthServices(
          this IServiceCollection services, IConfiguration config)
      {
          services.AddScoped<IAuthService, AuthService>();
          // ... JWT config
          return services;
      }
  }

  // Extensions/ProductServiceExtensions.cs — nhánh product sửa file này
  public static class ProductServiceExtensions
  {
      public static IServiceCollection AddProductServices(
          this IServiceCollection services)
      {
          services.AddScoped<IProductService, ProductService>();
          return services;
      }
  }
  ```

  **Lợi ích:** Mỗi nhánh chỉ sửa file riêng → không bao giờ conflict trong `Program.cs`.

- **Pull thường xuyên:** mỗi sáng `git pull origin develop` trước khi code.

---

#### Bài 5: Tích hợp Postman Collection

**Yêu cầu:** Tích hợp thử nghiệm với Frontend (nếu có) hoặc mô phỏng gọi liên hoàn nhiều API qua Postman (dùng Postman Variables).

**Giải thích chi tiết:**

**Tạo Collection với chuỗi request liên hoàn:**

**Request 1 — Login (lưu token):**

```text
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

Body: {
  "username": "admin",
  "password": "123"
}

Tests (JavaScript):
  const response = pm.response.json();
  pm.collectionVariables.set("token", response.token);
  console.log("Token saved:", response.token);
```

**Request 2 — Tạo sản phẩm (dùng token):**

```text
POST {{baseUrl}}/api/products
Headers: Authorization = Bearer {{token}}
Content-Type: application/json

Body: {
  "name": "Áo thun",
  "price": 150000,
  "categoryId": 1
}

Tests:
  pm.test("Status 201", () => pm.response.to.have.status(201));
  const created = pm.response.json();
  pm.collectionVariables.set("productId", created.id);
```

**Request 3 — Lấy sản phẩm vừa tạo:**

```text
GET {{baseUrl}}/api/products/{{productId}}
Headers: Authorization = Bearer {{token}}

Tests:
  pm.test("Status 200", () => pm.response.to.have.status(200));
  pm.test("Name match", () => {
    pm.expect(pm.response.json().name).to.eql("Áo thun");
  });
```

**Request 4 — Test phân quyền (dùng token User):**

```text
DELETE {{baseUrl}}/api/products/{{productId}}
Headers: Authorization = Bearer {{userToken}}

Tests:
  pm.test("Status 403 Forbidden", () => pm.response.to.have.status(403));
```

**Cách chạy tự động:**

- Click chuột phải Collection → **Run collection** → chọn thứ tự request → **Run**.
- Xem kết quả từng request xanh/đỏ.

---

#### Bài 6: Tích hợp Frontend (nếu có)

**Yêu cầu:** Kết nối Frontend React/Vue/Angular với Backend, đảm bảo CORS và JWT hoạt động.

**Giải thích chi tiết:**

**Bước 1 — Cấu hình CORS backend (đã học Buổi 13):**

```csharp
app.UseCors("AllowFrontend");   // policy cho phép http://localhost:3000
```

**Bước 2 — Frontend tạo service gọi API với Axios:**

```javascript
// src/services/api.js
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
});

// Tự động đính kèm token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Tự động xử lý 401 → redirect login
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export default api;
```

**Bước 3 — Component login:**

```javascript
// src/pages/Login.jsx
import { useState } from 'react';
import api from '../services/api';

export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async () => {
    try {
      const { data } = await api.post('/auth/login', { username, password });
      localStorage.setItem('token', data.token);
      window.location.href = '/products';
    } catch (err) {
      alert('Sai tài khoản hoặc mật khẩu');
    }
  };

  return (
    <div>
      <input value={username} onChange={e => setUsername(e.target.value)} />
      <input type="password" value={password} onChange={e => setPassword(e.target.value)} />
      <button onClick={handleLogin}>Đăng nhập</button>
    </div>
  );
}
```

**Bước 4 — Test end-to-end:**

1. Chạy backend: `dotnet run` (port 5000).
2. Chạy frontend: `npm start` (port 3000).
3. Mở `http://localhost:3000/login`.
4. Đăng nhập → kiểm tra Network tab: response có `token`.
5. Chuyển trang → gọi API `/products` → header có `Authorization: Bearer ...`.
6. **Test lỗi CORS:** comment `app.UseCors(...)` → reload trang → DevTools báo lỗi CORS đỏ → bỏ comment khôi phục.

---

## 6. Bảng thuật ngữ

| Thuật ngữ | Giải thích |
|---|---|
| **SDLC** | Software Development Life Cycle — vòng đời phát triển phần mềm (6 giai đoạn). |
| **Agile / Scrum** | Mô hình phát triển lặp (sprint 1-4 tuần) — thay cho Waterfall tuần tự. |
| **Waterfall** | Mô hình thác nước — chạy tuần tự 6 giai đoạn, không quay lại. |
| **Sprint** | Chu kỳ phát triển ngắn (1-4 tuần) trong Agile — kết thúc bằng bản demo. |
| **Integration** | Tích hợp — ghép các module riêng lẻ thành hệ thống hoàn chỉnh. |
| **Feature Branch** | Nhánh Git riêng cho từng tính năng — merge qua Pull Request. |
| **Pull Request (PR)** | Yêu cầu merge nhánh — nơi review code trước khi gộp vào `develop`/`main`. |
| **Merge Conflict** | Xung đột khi 2 nhánh cùng sửa 1 dòng code — phải giải quyết thủ công. |
| **Conventional Commits** | Quy ước đặt tên commit (`feat:`, `fix:`, `docs:`...). |
| **DI (Dependency Injection)** | Tiêm phụ thuộc — container tự khởi tạo và truyền service vào constructor. |
| **Repository Pattern** | Lớp trung gian truy cập dữ liệu — tách business logic khỏi ORM. |
| **Service Pattern** | Lớp chứa business logic — Controller chỉ điều phối, không xử lý. |
| **Fat Controller** | Anti-pattern — Controller phình to chứa quá nhiều logic. |
| **N-tier** | Kiến trúc nhiều lớp (Presentation → Business → Data). |
| **Clean Architecture** | Kiến trúc nhiều project, phụ thuộc hướng vào Domain (không phụ thuộc ngược). |
| **Domain Layer** | Lớp trung tâm chứa Entity — không phụ thuộc ai. |
| **Infrastructure Layer** | Lớp hạ tầng — DbContext, Repository, external service. |
| **Application Layer** | Lớp ứng dụng — Service, DTO, use case. |
| **DTO (Data Transfer Object)** | Object trung gian trao đổi dữ liệu API ↔ Client — tránh lộ Entity. |
| **AutoMapper** | Thư viện map Entity ↔ DTO tự động theo quy ước tên. |
| **Data Annotations** | Attribute validate (`[Required]`, `[Range]`...) đặt trên property. |
| **FluentValidation** | Thư viện validate nâng cao — tách rule ra class riêng, dễ test. |
| **Circular Reference** | Tham chiếu vòng — A trỏ B, B trỏ A → serializer đệ quy vô hạn. |
| **`ReferenceHandler.IgnoreCycles`** | Cấu hình System.Text.Json bỏ qua vòng lặp khi serialize. |
| **User Secrets** | Công cụ lưu secret cục bộ cho dev — không commit lên Git. |
| **Environment Variables** | Biến môi trường — cách cấu hình production không lưu trong code. |
| **`.editorconfig`** | File cấu hình style code chung cho team (indent, using order...). |
| **xUnit / NUnit / MSTest** | Các framework unit test phổ biến cho .NET. |
| **Moq** | Thư viện mock — tạo object giả lập interface để test. |
| **Postman Collection** | Tập hợp request Postman — chia sẻ được, có biến, có test script. |
| **Postman Variable** | Biến trong Postman (`{{baseUrl}}`, `{{token}}`) — tái sử dụng request. |
| **End-to-End Test** | Test toàn bộ luồng từ frontend → API → DB → response. |
| **Smoke Test** | Test nhanh sau khi deploy — kiểm tra app còn "sống" không. |
| **Regression Bug** | Lỗi cũ tái xuất hiện sau khi sửa code mới. |

## 7. Cheatsheet

### 7.1. Quy trình tích hợp — 5 bước

```text
[1] DATABASE
    → Merge migrations, chạy update-database thành công
        ↓
[2] DI + Program.cs
    → Ghép tất cả AddScoped/AddTransient
        ↓
[3] MIDDLEWARE
    → Exception → CORS → Auth → Authz → Controller
        ↓
[4] CONTROLLER + SERVICE
    → Từng module một, test riêng
        ↓
[5] END-TO-END
    → Đăng ký → Login → Lấy token → Gọi API bảo mật
```

### 7.2. Checklist tối thiểu để nộp đồ án

**Bắt buộc (không có = trượt):**

- [ ] Login sinh JWT thành công.
- [ ] API CRUD ít nhất 2 entity (VD: Products + Orders).
- [ ] Phân quyền Admin/User hoạt động (403 khi thiếu quyền).
- [ ] CSDL dùng EF Core Migration.
- [ ] Có Global Exception Middleware.
- [ ] Routing RESTful (`/api/products/{id}`).
- [ ] Có Swagger chạy được.

**Nên có (điểm cộng):**

- [ ] Pagination + Filtering + Searching + Sorting.
- [ ] Upload file + Validation.
- [ ] Refresh Token.
- [ ] Unit test cho Service layer.
- [ ] Dockerfile.
- [ ] README hướng dẫn chạy.
- [ ] Postman Collection kèm repo.

### 7.3. Git workflow — lệnh nhanh

```bash
# 1. Tạo nhánh mới từ develop
git checkout develop
git pull origin develop
git checkout -b feature/ten-tinh-nang

# 2. Code + commit thường xuyên
git add .
git commit -m "feat(module): mô tả ngắn"

# 3. Push lên remote
git push origin feature/ten-tinh-nang

# 4. Trước khi tạo PR, merge develop mới nhất
git checkout develop
git pull origin develop
git checkout feature/ten-tinh-nang
git merge develop
# → fix conflict nếu có

# 5. Push lại + tạo PR trên GitHub/GitLab
git push origin feature/ten-tinh-nang

# 6. Sau khi PR merged, xóa nhánh
git branch -d feature/ten-tinh-nang
git push origin --delete feature/ten-tinh-nang
```

### 7.4. Mẫu `CHECKLIST.md` để copy

```markdown
# Checklist đồ án

## Data & API
- [ ] Quan hệ 1-n, n-n đúng
- [ ] EF Core Migration
- [ ] Routing RESTful
- [ ] HTTP Methods đúng
- [ ] Validate Data Annotations

## Security
- [ ] Login sinh JWT
- [ ] API nhạy cảm có `[Authorize]`
- [ ] Phân quyền Role Admin/User
- [ ] CORS cấu hình đúng

## Architecture
- [ ] DI đầy đủ
- [ ] Repository Pattern
- [ ] Service Pattern
- [ ] Global Exception Middleware
- [ ] Pagination
- [ ] DTO cho mọi response
```

### 7.5. Bảng lỗi tích hợp — tra nhanh

| Triệu chứng | Nguyên nhân | Fix |
|---|---|---|
| `Unable to resolve service for type 'IX'` | Thiếu `AddScoped<IX, X>` | Đăng ký trong `Program.cs` |
| Response JSON khổng lồ, vòng lặp | Circular reference | Trả DTO hoặc `IgnoreCycles` |
| `401` dù đã login | Sai scheme JWT / thiếu prefix `Bearer` | Kiểm tra `SecuritySchemeType` + header |
| `403` dù đúng token | Thiếu Role/Policy | Thêm `ClaimTypes.Role` khi sinh token |
| Browser báo CORS đỏ | `UseCors` sai vị trí | Đặt sau `UseRouting`, trước `UseAuthentication` |
| `500` sau merge | Conflict chưa giải quyết / DI trùng | Đọc stack trace, `dotnet build` |
| Migration lỗi sau merge | 2 migration cùng timestamp | Xóa migration mới, tạo lại trên code gộp |
| Port đã dùng | Process cũ chưa tắt | `netstat -ano \| findstr :5000` → `taskkill /PID x /F` |

### 7.6. Cấu trúc `Program.cs` tách extension (tránh conflict)

```csharp
// Program.cs — chỉ gọi extension
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddProductServices();
builder.Services.AddOrderServices();
builder.Services.AddAppCors();
builder.Services.AddAppSwagger();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAppPipeline();   // CORS, Auth, Authz...

app.MapControllers();
app.Run();
```

```text
Extensions/
├── AuthServiceExtensions.cs      ← nhánh Auth sửa
├── ProductServiceExtensions.cs   ← nhánh Product sửa
├── OrderServiceExtensions.cs     ← nhánh Order sửa
├── DatabaseExtensions.cs         ← ai thêm entity thì sửa
└── PipelineExtensions.cs         ← hạn chế sửa, review kỹ
```

### 7.7. Lệnh CLI hay dùng

```bash
# === DATABASE ===
dotnet ef migrations add TenMigration
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations remove    # xóa migration cuối

# === BUILD & RUN ===
dotnet build                   # build
dotnet run                     # chạy dev
dotnet watch run               # hot reload

# === TEST ===
dotnet test                    # chạy unit test

# === PUBLISH ===
dotnet publish -c Release -o ./publish

# === PACKAGE ===
dotnet add package TenPackage
dotnet list package            # xem package đang dùng
dotnet list package --outdated # xem package lỗi thời
dotnet list package --vulnerable # xem lỗ hổng bảo mật
```

### 7.8. Câu hỏi tự kiểm tra trước khi nộp

- [ ] Clone repo trên máy khác → có chạy được ngay không? (README đủ chưa?)
- [ ] `appsettings.json` có chứa secret thật không? (nếu có → đã bị lộ)
- [ ] Có ai đó (ngoài nhóm) có thể login + gọi API bằng Postman không?
- [ ] Tắt backend → frontend có báo lỗi thân thiện không?
- [ ] Có API nào trả về Entity thay vì DTO không?
- [ ] Có Controller nào > 200 dòng không? (dấu hiệu Fat Controller)
- [ ] Có `Console.WriteLine` sót lại không? (phải dùng `ILogger`)
- [ ] Có `try-catch` rải rác trong Controller không? (phải dùng Global Middleware)
- [ ] Swagger có test được API bảo mật bằng nút Authorize không?
- [ ] Commit cuối cùng là khi nào? (nếu > 3 ngày → cần cập nhật)
