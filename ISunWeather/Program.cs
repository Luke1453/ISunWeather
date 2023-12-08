using ISunWeather.BLL;
using ISunWeather.BLL.Interfaces;
using ISunWeather.BLL.Services;
using ISunWeather.Common.Configs;
using ISunWeather.Common.Configs.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text;

namespace ISunWeather;

internal class Program
{
    // Since business logic is separated and application is modular 
    // this application entry point can be replaced or extended or even new one can be added
    // (privided that build configuration of the application is setup properly)
    // to convert this app to desktop or web application

    static async Task Main(string[] args)
    {
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .CreateLogger();
        Log.Information("Starting up");

        var hostBuilder = CreateHostBuilder();
        hostBuilder.UseSerilog();

        using IHost host = hostBuilder.Build();
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;
        try
        {
            await services.GetRequiredService<App>().Run(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configurations
                services.AddSingleton<IApiConfig>(provider => GetApiConfig(provider));
                services.AddSingleton<IDataSaverConfig>(provider => GetDataSaverConfig(provider));

                // Other Services
                services.AddMemoryCache();
                services.AddHttpClient("WeatherApiClient");

                // Application Services
                services.AddSingleton<IWeatherApiService, WeatherApiService>();
                services.AddSingleton<IDataSaverService, DataSaverService>();

                services.AddSingleton<App>();
            })
            .ConfigureAppConfiguration(app =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(Appsettings.config);
                using MemoryStream? resourceFileStream = new(buffer);
                if (resourceFileStream is not null)
                {
                    resourceFileStream.Seek(0, SeekOrigin.Begin);
                    MemoryStream reader = new();
                    resourceFileStream.CopyTo(reader);
                    reader.Seek(0, SeekOrigin.Begin);
                    app.AddJsonStream(reader);
                }
                else
                {
                    Console.WriteLine("Failed to read embeded appsettings.json");
                    Environment.Exit(-1);
                }
            });
    }

    #region Private Methods

    private static ApiConfig GetApiConfig(IServiceProvider provider)
    {
        // need to do it manualy since AOT linker gets confused
        var c = provider
            .GetRequiredService<IConfiguration>()
            .GetRequiredSection("ApiConfig");
        return new ApiConfig
        {
            BaseUrl = c["BaseUrl"] ?? "https://weather-api.isun.ch",
            Username = c["Username"] ?? "isun",
            Password = c["Password"] ?? "passwrod",
            CitiesRoute = c["CitiesRoute"] ?? "/api/cities",
            AuthorizeRoute = c["AuthorizeRoute"] ?? "/api/authorize",
            WeathersRoute = c["WeathersRoute"] ?? "/api/weathers/",
            SessionExpiresInMin = int.TryParse(c["SessionExpiresInMin"], out int x) ? x : 1,
        };
    }

    private static DataSaverConfig GetDataSaverConfig(IServiceProvider provider)
    {
        // need to do it manualy since AOT linker gets confused
        var c = provider
            .GetRequiredService<IConfiguration>()
            .GetRequiredSection("DataSaverConfig");
        return new DataSaverConfig
        {
            WorkingDirectoryPath = c["WorkingDirectoryPath"] ?? "",
            Filename = c["Filename"] ?? "weatherReports.txt",
        };
    }

    #endregion
}
