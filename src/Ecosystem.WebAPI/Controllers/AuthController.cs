using System.Security.Claims;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý xác thực người dùng bằng JWT Bearer Token và Refresh Token Rotation (Buổi 8).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    #region Nested Sub-Resource: Categories of Aspect

    /// <summary>
    /// Lấy danh sách các Chuyên mục (Categories) thuộc về một Khía cạnh cụ thể (Nested Resource).
    /// </summary>
    /// <param name="aspectId">Mã định danh của Khía cạnh cha.</param>
    /// <returns>Danh sách Chuyên mục dạng CategoryResponseDto.</returns>
    /// <response code="200">Lấy danh sách Chuyên mục thành công.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cha tương ứng.</response>
    [HttpGet("{aspectId:guid}/categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetCategoriesByAspectId([FromRoute] Guid aspectId)
    {
        var categories = await _taxonomyService.GetCategoriesByAspectIdAsync(aspectId);
        var dtos = categories.Select(c => c.ToResponseDto());
        return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.Ok(dtos, $"Lấy danh sách Chuyên mục của Khía cạnh '{aspectId}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Chuyên mục (Category) trực thuộc một Khía cạnh cụ thể (Nested Resource).
    /// </summary>
    /// <param name="aspectId">Mã định danh của Khía cạnh cha.</param>
    /// <param name="request">Dữ liệu tạo mới CategoryCreateDto.</param>
    /// <returns>Chuyên mục vừa tạo dạng CategoryResponseDto.</returns>
    /// <response code="201">Tạo Chuyên mục mới thành công.</response>
    /// <response code="400">Dữ liệu đầu vào vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cha tương ứng.</response>
    [HttpPost("{aspectId:guid}/categories")]
    [Authorize(Policy = AuthConstants.Policies.RequireHost)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategoryForAspect(
        [FromRoute] Guid aspectId,
        [FromBody] CategoryCreateDto request)
    {
        var createRequest = new CreateCategoryRequest
        {
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        var created = await _taxonomyService.CreateCategoryAsync(aspectId, createRequest);
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Categories",
            routeValues: new { id = created.Id },
            value: ApiResponse<CategoryResponseDto>.Ok(created.ToResponseDto(), "Tạo Chuyên mục mới trực thuộc Khía cạnh thành công."));
    }

    /// <summary>
    /// Lấy thông tin hồ sơ của người dùng đang đăng nhập từ JWT Claims (Endpoint kiểm chứng [Authorize]).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Token không hợp lệ",
                "Không thể trích xuất định danh người dùng từ JWT Claims."));
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy người dùng",
                "Tài khoản tương ứng với Token không còn tồn tại trong hệ thống."));
        }

        return Ok(ApiResponse<UserResponseDto>.Ok(user, "Xác thực danh tính JWT thành công."));
    }

    /// <summary>
    /// Trích xuất toàn bộ JWT Claims, Roles và Department của người dùng hiện tại (Bài tập 4 - Buổi 9: WhoAmI).
    /// </summary>
    [HttpGet("claims")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyClaims()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var department = User.FindFirstValue(AuthConstants.Claims.Department);
        var dateOfBirth = User.FindFirstValue(ClaimTypes.DateOfBirth);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();

        var payload = new
        {
            userId,
            username,
            email,
            department,
            dateOfBirth,
            roles,
            greeting = $"Xin chào {username ?? "Thành viên"} (Vai trò: {string.Join(", ", roles)})!",
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        return Ok(ApiResponse<object>.Ok(payload, "Trích xuất danh sách JWT Claims thành công."));
    }

    /// <summary>
    /// Kiểm chứng Custom Authorization Policy dựa trên độ tuổi tối thiểu (>= 18) từ DateOfBirth Claim (Bài tập 5 - Buổi 9).
    /// </summary>
    [HttpGet("adult-verified")]
    [Authorize(Policy = AuthConstants.Policies.AtLeast18)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult CheckAdultVerified()
    {
        var username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
        var dateOfBirth = User.FindFirstValue(ClaimTypes.DateOfBirth);

        return Ok(ApiResponse<object>.Ok(new
        {
            verified = true,
            policy = AuthConstants.Policies.AtLeast18,
            minimumAge = 18,
            username,
            dateOfBirth
        }, "Xác thực chính sách độ tuổi (>= 18) thành công."));
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = HttpContext?.Request?.Path ?? "/api/v1/auth"
        };
    }
}
