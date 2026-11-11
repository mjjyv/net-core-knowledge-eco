using Ecosystem.Domain.Enums;

namespace Ecosystem.Application.Common;

/// <summary>
/// Hằng số tập trung quản lý Roles, Policies và Custom Claims — tuyệt đối không hardcode chuỗi phân quyền rải rác (Buổi 9).
/// </summary>
public static class AuthConstants
{
    public static class Roles
    {
        public const string Host = nameof(UserRole.Host);
        public const string Presenter = nameof(UserRole.Presenter);
        public const string Audience = nameof(UserRole.Audience);

        public const string HostOrPresenter = $"{Host},{Presenter}";
    }

    public static class Policies
    {
        public const string RequireHost = nameof(RequireHost);
        public const string RequireContentCreator = nameof(RequireContentCreator);
        public const string ArticleOwnerOrHost = nameof(ArticleOwnerOrHost);
        public const string AtLeast18 = nameof(AtLeast18);
        public const string MustHaveEcosystemEmail = nameof(MustHaveEcosystemEmail);
        public const string RequireEducatorDepartment = nameof(RequireEducatorDepartment);
    }

    public static class Claims
    {
        public const string Department = "Department";
    }

    public static class Departments
    {
        public const string PlatformAdmin = "PlatformAdmin";
        public const string Education = "Education";
        public const string Community = "Community";
    }
}
