using FileCheckingService.Entities.ConfigurationModels;
using FileCheckingService.Repository;
using FileCheckingService.Repository.Contract;
using FileCheckingService.Service.Contract;
using FileCheckingService.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FileCheckingService.Logging;

namespace FileCheckingService.Extensions
{
    public static class ServiceExtensions
    {
        // Adds db context to the application, configures postgresql database connection, configures db connection resilience
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("sqlConnection"), builder =>
                {
                    SqlConnectionConfig config = configuration.GetSection("SqlConnection").Get<SqlConnectionConfig>();
                    builder.EnableRetryOnFailure(config.MaxRetryCount, TimeSpan.FromSeconds(config.RetryDelayInSeconds), null);
                });
            });
        }

        // Adds unit of work for database access and interactions
        public static void ConfigureUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // Adds singleton background service NewFilesCheckService which will
        // create scoped services of type ScopedNewFilesCheckService
        public static void ConfigureBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<NewFilesCheckService>();
            services.AddScoped<IScopedBackgroundService, ScopedNewFilesCheckService>();
        }

        // Adds service for communication with sftp server
        public static void ConfigureSftpService(this IServiceCollection services)
        {
            services.AddScoped<ISftpService, SftpService>();
        }

        // Adds custom logger
        public static void ConfigureLogger(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }
    }
}
