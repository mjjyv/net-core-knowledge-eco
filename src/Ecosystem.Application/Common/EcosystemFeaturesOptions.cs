using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp cấu hình Strongly-Typed cho các tính năng và cờ môi trường ứng dụng (Section "EcosystemFeatures" - Buổi 10).
/// Minh họa sự khác biệt giữa Development vs Production và cơ chế Options Pattern.
/// </summary>
public class EcosystemFeaturesOptions
{
    public const string SectionName = "EcosystemFeatures";

    [Required(AllowEmptyStrings = false, ErrorMessage = "ApplicationTitle không được để trống.")]
    public string ApplicationTitle { get; set; } = "The Ecosystem Web API";

    [Required(AllowEmptyStrings = false, ErrorMessage = "EnvironmentBanner không được để trống.")]
    public string EnvironmentBanner { get; set; } = "Standard Mode";

    public bool EnableSwagger { get; set; }

    public bool EnableDetailedErrors { get; set; }

    public bool EnableAutoMigration { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string ConfigVersion { get; set; } = "1.0.0";

    [Range(1, 100, ErrorMessage = "DefaultPageSize phải nằm trong khoảng 1 đến 100.")]
    public int DefaultPageSize { get; set; } = 20;
}
