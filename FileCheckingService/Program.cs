// See https://aka.ms/new-console-template for more information
using FileCheckingService.Entities.ConfigurationModels;
using FileCheckingService.Extensions;
using FileCheckingService.Logging;
using FileCheckingService.Repository;
using FileCheckingService.Repository.Contract;
using FileCheckingService.Service;
using FileCheckingService.Service.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

// To configure connections, paths etc. use appsettings.json file in the project root
// To configure nlog logging use nlog.config file in the project root

// Gets location of current working directory
string workingDirectory = Environment.CurrentDirectory;
// Provides configuration for nlog
LogManager.LoadConfiguration(string.Concat(workingDirectory, "/nlog.config"));

// Creates host builder
var builder = Host.CreateDefaultBuilder(args);

// Creates host which will be responsible for DI, Logging, Configuration,
// App Shutdown, Hosted Background Services 
var host =  builder.ConfigureServices((context, services) =>
    {
        services.ConfigureLogger();
        services.ConfigureSftpService();
        services.ConfigureBackgroundWorkers();
        services.ConfigureUnitOfWork();
        services.ConfigureSqlContext(context.Configuration);
    })
    .ConfigureAppConfiguration(app =>
    {
        // Adds configuration data from json file to application configuration
        app.AddJsonFile(string.Concat(workingDirectory, "/appsettings.json"));
    })
    .Build();

// Runs the host
await host.RunAsync();




