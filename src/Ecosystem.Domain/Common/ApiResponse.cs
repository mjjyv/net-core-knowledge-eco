namespace Ecosystem.Domain.Common;

/// <summary>
/// Generic Response Wrapper chuẩn hóa dữ liệu phản hồi cho toàn hệ thống.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của payload trả về</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Thao tác thành công.") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? [message]
        };
}
