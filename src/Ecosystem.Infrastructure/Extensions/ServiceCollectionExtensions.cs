using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecosystem.Infrastructure.Extensions;

/// <summary>
/// Các phương thức mở rộng đăng ký dịch vụ tầng Infrastructure vào DI Container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký ApplicationDbContext với cơ chế đa nhà cung cấp cơ sở dữ liệu (SQLite mặc định, SQL Server).
    /// Vòng đời đăng ký là Scoped theo chuẩn Best Practice của Web API.
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

        return services;
    }
}
