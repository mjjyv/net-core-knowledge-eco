using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Ecosystem.WebAPI.Authorization;

/// <summary>
/// AuthorizationHandler xử lý MinimumAgeRequirement, tính chính xác tuổi kể cả trường hợp chưa qua ngày sinh nhật trong năm.
/// </summary>
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    private readonly Func<DateTime> _todayProvider;

    public MinimumAgeHandler() : this(() => DateTime.UtcNow.Date)
    {
    }

    public MinimumAgeHandler(Func<DateTime> todayProvider)
    {
        _todayProvider = todayProvider;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var dobClaim = context.User.FindFirst(ClaimTypes.DateOfBirth);
        if (dobClaim == null || string.IsNullOrWhiteSpace(dobClaim.Value))
        {
            return Task.CompletedTask;
        }

        if (!DateTime.TryParse(
                dobClaim.Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dob))
        {
            return Task.CompletedTask;
        }

        var today = _todayProvider().Date;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age))
        {
            age--;
        }

        if (age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
