using System.Reflection;
using System.Text;
using Ecosystem.Application.Common;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Ecosystem.WebAPI.Extensions;

/// <summary>
/// Các phương thức mở rộng đăng ký dịch vụ vào DI Container một cách mô-đun hóa và sạch sẽ.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký tài liệu tự động Swagger / OpenAPI kèm XML Comments và cấu hình JWT Bearer Authorization (Buổi 4 &amp; Buổi 8).
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
                Description = "Hệ sinh thái tri thức và giao tiếp hợp nhất (Unified Knowledge & Communication Ecosystem) - Backend ASP.NET Core .NET 8 C#. Hỗ trợ xác thực JWT Bearer Token, N-Tier Architecture, CRUD EF Core và Validation chặt chẽ.",
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

            // Cấu hình hỗ trợ Authorize Bearer Token trên giao diện Swagger UI (Bài tập 4 - Buổi 8)
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Nhập JWT Access Token (Swagger sẽ tự động thêm tiền tố 'Bearer '). Ví dụ: eyJhbGciOiJIUzI1Ni...",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    /// Cấu hình xác thực JWT Bearer Authentication (Buổi 8).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException("Cấu hình JwtSettings:SecretKey bị thiếu hoặc nhỏ hơn 32 bytes (256 bit).");
        }

        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Đăng ký AutoMapper và các dịch vụ nghiệp vụ cốt lõi của tầng Application với vòng đời chuẩn DI (Scoped cho các dịch vụ dùng UnitOfWork).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký AutoMapper Profile
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // Dịch vụ thực hành Buổi 1 (In-memory manager)
        services.AddSingleton<IStudentManager, StudentManager>();

        // Các dịch vụ nghiệp vụ N-Tier sử dụng IUnitOfWork (bắt buộc Scoped để khớp ApplicationDbContext)
        services.AddScoped<ITaxonomyOverviewService, TaxonomyOverviewService>();
        services.AddScoped<ITaxonomyService, TaxonomyService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

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
