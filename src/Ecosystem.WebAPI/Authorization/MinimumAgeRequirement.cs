using Microsoft.AspNetCore.Authorization;

namespace Ecosystem.WebAPI.Authorization;

/// <summary>
/// Requirement kiểm tra độ tuổi tối thiểu từ Claim DateOfBirth (Bài tập 5 - Buổi 9).
/// </summary>
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}
