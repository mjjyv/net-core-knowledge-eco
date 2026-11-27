using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý và kiểm chứng cấu hình ứng dụng theo Environment (Development vs Production)
/// và Options Pattern: IOptions&lt;T&gt;, IOptionsSnapshot&lt;T&gt;, IOptionsMonitor&lt;T&gt; (Buổi 10).
/// </summary>
[ApiController]
[Route("api/v1/config")]
[Produces("application/json")]
public class ConfigController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IOptions<FileStorageOptions> _singletonFileOptions;
    private readonly IOptionsSnapshot<FileStorageOptions> _snapshotFileOptions;
    private readonly IOptionsSnapshot<EcosystemFeaturesOptions> _snapshotFeatureOptions;
    private readonly IOptionsMonitor<EcosystemFeaturesOptions> _monitorFeatureOptions;

    public ConfigController(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IOptions<FileStorageOptions> singletonFileOptions,
        IOptionsSnapshot<FileStorageOptions> snapshotFileOptions,
        IOptionsSnapshot<EcosystemFeaturesOptions> snapshotFeatureOptions,
        IOptionsMonitor<EcosystemFeaturesOptions> monitorFeatureOptions)
    {
        _environment = environment;
        _configuration = configuration;
        _singletonFileOptions = singletonFileOptions;
        _snapshotFileOptions = snapshotFileOptions;
        _snapshotFeatureOptions = snapshotFeatureOptions;
        _monitorFeatureOptions = monitorFeatureOptions;
    }

    /// <summary>
    /// Xem thông tin môi trường chạy hiện tại (Development / Staging / Production) và Feature Flags an toàn.
    /// Không làm lộ Secret Keys, Password hoặc đường dẫn vật lý trên Server.
    /// </summary>
    [HttpGet("environment")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetEnvironmentInfo()
    {
        var features = _snapshotFeatureOptions.Value;

        var payload = new
        {
            environmentName = _environment.EnvironmentName,
            applicationName = _environment.ApplicationName,
            isDevelopment = _environment.IsDevelopment(),
            isStaging = _environment.IsStaging(),
            isProduction = _environment.IsProduction(),
            features = new
            {
                features.ApplicationTitle,
                features.EnvironmentBanner,
                features.EnableSwagger,
                features.EnableDetailedErrors,
                features.EnableAutoMigration,
                features.ConfigVersion,
                features.DefaultPageSize
            }
        };

        return Ok(ApiResponse<object>.Ok(payload, "Lấy thông tin môi trường ứng dụng thành công."));
    }

    /// <summary>
    /// So sánh trực tiếp 3 biến thể Options Pattern (IOptions, IOptionsSnapshot, IOptionsMonitor)
    /// và kiểm tra cấu hình môi trường dành riêng cho Host.
    /// </summary>
    [HttpGet("options")]
    [Authorize(Roles = AuthConstants.Roles.Host)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult GetOptionsComparison()
    {
        var rawConnection = _configuration.GetConnectionString("DefaultConnection");
        var provider = _configuration["DatabaseProvider"] ?? "Sqlite";

        var payload = new
        {
            environmentName = _environment.EnvironmentName,
            databaseProvider = provider,
            maskedConnectionString = MaskConnectionString(rawConnection),
            contentRootPath = _environment.ContentRootPath,
            webRootPath = _environment.WebRootPath,
            optionsPatternComparison = new
            {
                iOptionsSingleton = new
                {
                    lifetime = "Singleton (đọc một lần lúc khởi động)",
                    value = _singletonFileOptions.Value
                },
                iOptionsSnapshotScoped = new
                {
                    lifetime = "Scoped (tự động đọc lại cấu hình mới trên từng HTTP request)",
                    value = _snapshotFileOptions.Value
                },
                iOptionsMonitorSingleton = new
                {
                    lifetime = "Singleton + OnChange (CurrentValue luôn đồng bộ thay đổi cấu hình)",
                    currentValue = _monitorFeatureOptions.CurrentValue
                }
            }
        };

        return Ok(ApiResponse<object>.Ok(payload, "Truy xuất báo cáo so sánh Options Pattern thành công."));
    }
    }

    /// <summary>
    /// Tải hoặc xem tệp đính kèm theo tên lưu trữ (bảo vệ chống Path Traversal &amp; MIME Sniffing XSS).
    /// </summary>
    [HttpGet("{fileName}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [FromRoute] string fileName,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileResult = await _fileStorageService.OpenDownloadAsync(fileName, cancellationToken);
            if (fileResult == null)
            {
                return NotFound(CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    "Không tìm thấy tệp",
                    $"Tệp '{fileName}' không tồn tại trên hệ thống."));
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return download
                ? File(fileResult.FileStream, fileResult.ContentType, fileResult.StoredFileName, enableRangeProcessing: true)
                : File(fileResult.FileStream, fileResult.ContentType, enableRangeProcessing: true);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tên tệp không hợp lệ",
                ex.Message));
        }
    }

    /// <summary>
    /// Xóa một tệp đính kèm khỏi kho lưu trữ (chỉ dành cho Presenter hoặc Host).
    /// </summary>
    [HttpDelete("{fileName}")]
    [Authorize(Policy = AuthConstants.Policies.RequireContentCreator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _fileStorageService.DeleteAsync(fileName, cancellationToken);
            if (!deleted)
            {
                return NotFound(CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    "Không tìm thấy tệp",
                    $"Không tìm thấy tệp '{fileName}' để xóa."));
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tên tệp không hợp lệ",
                ex.Message));
        }
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = HttpContext?.Request?.Path ?? "/api/v1/files"
        };
    }
}
