using System.Reflection;
using System.Security.Claims;
using System.Text;
using Ecosystem.Application.Common;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.WebAPI.Authorization;
using Ecosystem.WebAPI.Hubs;
using Ecosystem.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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

            // 3. Tải XML Comments của Domain Assembly (chứa mô tả Entities, Enums & ApiResponse)
            var domainXml = "Ecosystem.Domain.xml";
            var domainPath = Path.Combine(AppContext.BaseDirectory, domainXml);
            if (File.Exists(domainPath))
            {
                options.IncludeXmlComments(domainPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Cấu hình xác thực JWT Bearer Authentication và hệ thống Authorization đa lớp Roles / Claims / Policies (Buổi 8 &amp; Buổi 9).
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        if (!context.Response.Headers.ContainsKey("WWW-Authenticate"))
                        {
                            var headerValue = string.IsNullOrWhiteSpace(context.Error)
                                ? "Bearer"
                                : $"Bearer error=\"{context.Error}\"";
                            context.Response.Headers.Append("WWW-Authenticate", headerValue);
                        }

                        var problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Chưa xác thực (Unauthorized)",
                            Detail = string.IsNullOrWhiteSpace(context.ErrorDescription)
                                ? "Yêu cầu bắt buộc phải đính kèm JWT Access Token hợp lệ trong Header Authorization: Bearer <token>."
                                : context.ErrorDescription,
                            Instance = context.Request.Path
                        };
                        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                        await context.Response.WriteAsJsonAsync(
                            problem,
                            options: null,
                            contentType: "application/problem+json; charset=utf-8");
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        var problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = "Không có quyền truy cập (Forbidden)",
                            Detail = "Tài khoản của bạn đã được xác thực nhưng không đủ vai trò, claim hoặc quyền sở hữu tài nguyên để thực hiện thao tác này.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                        await context.Response.WriteAsJsonAsync(
                            problem,
                            options: null,
                            contentType: "application/problem+json; charset=utf-8");
                    }
                };
            });

        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
        services.AddSingleton<IAuthorizationHandler, ArticleOwnerOrHostHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.Policies.RequireHost, policy =>
                policy.RequireRole(AuthConstants.Roles.Host));

            options.AddPolicy(AuthConstants.Policies.RequireContentCreator, policy =>
                policy.RequireRole(AuthConstants.Roles.Host, AuthConstants.Roles.Presenter));

            options.AddPolicy(AuthConstants.Policies.MustHaveEcosystemEmail, policy =>
                policy.RequireAssertion(context =>
                {
                    var email = context.User.FindFirstValue(ClaimTypes.Email);
                    return !string.IsNullOrWhiteSpace(email) &&
                           email.EndsWith("@ecosystem.vn", StringComparison.OrdinalIgnoreCase);
                }));

            options.AddPolicy(AuthConstants.Policies.RequireEducatorDepartment, policy =>
                policy.RequireClaim(
                    AuthConstants.Claims.Department,
                    AuthConstants.Departments.PlatformAdmin,
                    AuthConstants.Departments.Education));

            options.AddPolicy(AuthConstants.Policies.AtLeast18, policy =>
                policy.Requirements.Add(new MinimumAgeRequirement(18)));

            options.AddPolicy(AuthConstants.Policies.ArticleOwnerOrHost, policy =>
                policy.Requirements.Add(new ArticleOwnerOrHostRequirement()));
        });

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

    /// <summary>
    /// Đăng ký các lớp cấu hình Strongly-Typed theo Options Pattern với ValidateDataAnnotations().ValidateOnStart() (Buổi 10).
    /// </summary>
    public static IServiceCollection AddConfigurationAndOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EcosystemFeaturesOptions>()
            .Bind(configuration.GetSection(EcosystemFeaturesOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ObservabilityOptions>()
            .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
        });

        return services;
    }

    /// <summary>
    /// Cấu hình chính sách CORS bảo mật theo môi trường đọc từ CorsOptions (Buổi 13).
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                if (corsOptions.ExposedHeaders.Length > 0)
                {
                    policy.WithExposedHeaders(corsOptions.ExposedHeaders);
                }

                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Đăng ký các tối ưu hiệu năng cho môi trường Production (Response Compression).
    /// </summary>
    public static IServiceCollection AddProductionOptimizations(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return services;
    }

    /// <summary>
    /// Đăng ký SignalR Hubs, bộ theo dõi hiện diện phòng học và dịch vụ nền Eviction Policy 30s (Buổi 14).
    /// </summary>
    public static IServiceCollection AddRealtimeServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IClassroomPresenceTracker, ClassroomPresenceTracker>();
        services.AddHostedService<ClassroomHeartbeatMonitorService>();

        return services;
    }
}
