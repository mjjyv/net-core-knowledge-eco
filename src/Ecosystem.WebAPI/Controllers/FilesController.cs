using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful Upload, Download và Xóa tệp đính kèm với Validation đa tầng &amp; Options Pattern (Buổi 10).
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FilesController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Lấy cấu hình giới hạn kích thước và định dạng tệp được phép tải lên từ FileStorageOptions.
    /// </summary>
    [HttpGet("limits")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FileLimitsDto>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<FileLimitsDto>> GetLimits()
    {
        var limits = _fileStorageService.GetLimits();
        return Ok(ApiResponse<FileLimitsDto>.Ok(limits, "Lấy thông tin cấu hình giới hạn tải tệp thành công."));
    }

    /// <summary>
    /// Tải lên một tệp đính kèm (hình ảnh, PDF hoặc Markdown &lt;= 5MB) qua multipart/form-data.
    /// Kiểm tra kích thước, whitelist phần mở rộng, MIME Content-Type và chữ ký Magic Bytes.
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> Upload(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Thiếu tệp tải lên",
                "Vui lòng chọn một tệp trong trường 'file' (multipart/form-data)."));
        }

        if (file.Length == 0)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tệp rỗng",
                "Tệp được chọn có dung lượng 0 bytes, vui lòng chọn tệp hợp lệ."));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var input = new FileUploadInput
            {
                Content = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                Length = file.Length
            };

            var uploaded = await _fileStorageService.UploadAsync(input, cancellationToken);
            return CreatedAtAction(
                nameof(Download),
                new { fileName = uploaded.StoredFileName },
                ApiResponse<FileUploadResultDto>.Ok(uploaded, "Tải tệp đính kèm lên hệ thống thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tệp tải lên không hợp lệ",
                ex.Message));
        }
    }

    /// <summary>
    /// Tải lên nhiều tệp đính kèm cùng lúc (trả về kết quả từng tệp hợp lệ và danh sách lỗi nếu có).
    /// </summary>
    [HttpPost("upload-multiple")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<MultiFileUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<MultiFileUploadResultDto>>> UploadMultiple(
        List<IFormFile>? files,
        CancellationToken cancellationToken = default)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Thiếu danh sách tệp",
                "Vui lòng chọn ít nhất một tệp trong trường 'files' (multipart/form-data)."));
        }

        var inputs = new List<FileUploadInput>(files.Count);
        try
        {
            foreach (var file in files)
            {
                inputs.Add(new FileUploadInput
                {
                    Content = file.OpenReadStream(),
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    Length = file.Length
                });
            }

            var result = await _fileStorageService.UploadMultipleAsync(inputs, cancellationToken);
            if (result.SuccessCount == 0 && result.ErrorCount > 0)
            {
                var problem = CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Tất cả tệp tải lên đều không hợp lệ",
                    string.Join(" | ", result.Errors.Select(e => $"{e.OriginalFileName}: {e.Reason}")));
                problem.Extensions["errors"] = result.Errors;
                return BadRequest(problem);
            }

            return Ok(ApiResponse<MultiFileUploadResultDto>.Ok(
                result,
                $"Đã tải lên thành công {result.SuccessCount}/{inputs.Count} tệp."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Yêu cầu tải nhiều tệp không hợp lệ",
                ex.Message));
        }
        finally
        {
            foreach (var input in inputs)
            {
                await input.Content.DisposeAsync();
            }
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
