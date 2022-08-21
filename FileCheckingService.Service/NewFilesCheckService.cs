using FileCheckingService.Entities.ConfigurationModels;
using FileCheckingService.Logging;
using FileCheckingService.Service.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileCheckingService.Service
{
    public class NewFilesCheckService : BackgroundService
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ServiceConfig _serviceConfig;
        private readonly TimeSpan _period;

        public NewFilesCheckService(ILoggerManager logger, IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _serviceConfig = configuration.GetSection("Service").Get<ServiceConfig>();
            _period = TimeSpan.FromSeconds(_serviceConfig.IntervalInSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWorkAsync(stoppingToken);
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInfo($"{nameof(NewFilesCheckService)} started working, will be checking for new files in sftp server every {_period.TotalSeconds} seconds.");
            PeriodicTimer timer = new PeriodicTimer(_period);     

            // Creates a scoped background service to check for new files periodically.
            // New scope will be created for every iteration, it will improve resilience
            // of the application.
            // New service will not be created until the old one has finished its job or failed
            // even if interval period has already been passed 
            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using AsyncServiceScope asyncScope = _serviceScopeFactory.CreateAsyncScope();
                    IScopedBackgroundService scopedBackgroundService = asyncScope.ServiceProvider.GetRequiredService<IScopedBackgroundService>();
                    await scopedBackgroundService.DoWorkAsync(stoppingToken);
                    await asyncScope.DisposeAsync();
                }
                catch(Exception exception)
                {
                    _logger.LogError($"Failed to run scoped checking for new files", exception);
                }           
            }
        }
    }
}
