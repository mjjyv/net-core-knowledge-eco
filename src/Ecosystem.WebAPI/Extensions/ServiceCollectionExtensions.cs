using System.Reflection;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Services;
using Microsoft.OpenApi.Models;

namespace Ecosystem.WebAPI.Extensions;

/// <summary>
/// Các phương thức mở rộng đăng ký dịch vụ vào DI Container một cách mô-đun hóa và sạch sẽ.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký tài liệu tự động Swagger / OpenAPI kèm XML Comments chi tiết (Buổi 4).
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "The Ecosystem Web API",
                Version = "v1",
                Description = "Hệ sinh thái tri thức và giao tiếp hợp nhất (Unified Knowledge & Communication Ecosystem) - Backend ASP.NET Core .NET 8 C#. Quản lý Cây tri thức phân cấp và Tài khoản người dùng với DTO & Validation chặt chẽ.",
                Contact = new OpenApiContact
                {
                    Name = "The Ecosystem Architecture Team",
                    Email = "architecture@ecosystem.vn",
                    Url = new Uri("https://ecosystem.vn")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // 1. Tải XML Comments của WebAPI Assembly
            var webApiXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var webApiPath = Path.Combine(AppContext.BaseDirectory, webApiXml);
            if (File.Exists(webApiPath))
            {
                options.IncludeXmlComments(webApiPath, includeControllerXmlComments: true);
            }

            // 2. Tải XML Comments của Application Assembly (chứa mô tả DTOs & Custom Validation)
            var appXml = "Ecosystem.Application.xml";
            var appPath = Path.Combine(AppContext.BaseDirectory, appXml);
            if (File.Exists(appPath))
            {
                options.IncludeXmlComments(appPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Đăng ký các dịch vụ nghiệp vụ cốt lõi của tầng Application.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Dịch vụ quản lý sinh viên thực hành Buổi 1
        services.AddSingleton<IStudentManager, StudentManager>();

        // Dịch vụ Cây Tri Thức cho The Ecosystem (Overview & Full CRUD)
        services.AddScoped<ITaxonomyOverviewService, TaxonomyOverviewService>();
        services.AddSingleton<ITaxonomyService, TaxonomyService>();

        // Dịch vụ Quản lý & Đăng ký Người dùng (Lab Buổi 4)
        services.AddSingleton<IUserService, UserService>();

        return services;
    }

    /// <summary>
    /// Đăng ký các dịch vụ kiểm chứng 3 vòng đời Native DI: Transient, Scoped, Singleton (Buổi 2).
    /// </summary>
    public static IServiceCollection AddLifetimeDemonstrationServices(this IServiceCollection services)
    {
        services.AddTransient<ITransientService, LifetimeService>();
        services.AddScoped<IScopedService, LifetimeService>();
        services.AddSingleton<ISingletonService, LifetimeService>();

        return services;
    }
}
