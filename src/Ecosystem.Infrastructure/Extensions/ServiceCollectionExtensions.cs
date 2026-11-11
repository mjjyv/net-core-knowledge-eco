using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Repositories;
using Ecosystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecosystem.Infrastructure.Extensions;

/// <summary>
/// Các phương thức mở rộng đăng ký dịch vụ tầng Infrastructure vào DI Container (DbContext, Repositories, UnitOfWork).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký ApplicationDbContext, toàn bộ Repositories và UnitOfWork với vòng đời Scoped.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=theEcosystem.db";
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    break;
                case "sqlite":
                default:
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    break;
            }
        });

        // Đăng ký Repositories & Unit of Work (vòng đời Scoped đồng bộ với ApplicationDbContext)
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IAspectRepository, AspectRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Đăng ký dịch vụ chẩn đoán DB cho tầng WebAPI
        services.AddScoped<IDatabaseDiagnosticsService, DatabaseDiagnosticsService>();

        return services;
    }
}
