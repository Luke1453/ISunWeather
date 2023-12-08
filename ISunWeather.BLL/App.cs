using ISunWeather.BLL.Interfaces;
using ISunWeather.Common.Entities;
using System.Text;

namespace ISunWeather.BLL;

public class App(
    IWeatherApiService weatherApi,
    IDataSaverService dataSaverService)
{
    private readonly IWeatherApiService _weatherApiService = weatherApi;
    private readonly IDataSaverService _dataSaverService = dataSaverService;


    public async Task Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided.");
            return;
        }

        // Check for --cities
        if (!args[0].Equals("--cities", StringComparison.CurrentCultureIgnoreCase) || args.Length <= 1)
        {
            if (args[0].StartsWith("--"))
            {
                Console.WriteLine($"Invalid argument: {args[0]}");
            }
            else
            {
                Console.WriteLine("Invalid arguments provided.");
            }
            return;
        }

        var cities = TruncateCities(args[1..]);
        await MainLoopEntry(cities);
    }


    private async Task MainLoopEntry(string[] cities)
    {
        var validCities = await _weatherApiService.GetValidCities();
        var invalidCities = cities.Except(validCities);
        var requiredCities = cities.Except(invalidCities);

        if (invalidCities.Any())
        {
            Console.WriteLine();
            Console.WriteLine("We have found some invalid cities:");
            foreach (var c in invalidCities)
            {
                Console.WriteLine(c);
            }
            Console.WriteLine("Proceding weather reporting using valid cities, if any.");
        }

        if (!requiredCities.Any())
        {
            Console.WriteLine();
            Console.WriteLine("No valid cities");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Reporting weather using cities:");
        foreach (var c in requiredCities)
        {
            Console.WriteLine(c);
        }
        Console.WriteLine();

        // Main report loop
        while (true)
        {
            await ReportValidCities(requiredCities.ToArray());
            await Task.Delay(TimeSpan.FromSeconds(15));
        }
    }

    private async Task ReportValidCities(string[] cities)
    {
        foreach (var c in cities)
        {
            var report = await _weatherApiService.GetCityWeather(c);
            var reportString = BuildWeatherReport(report);

            // Write report to file
            await _dataSaverService.SaveWeatherReport(reportString);

            Console.WriteLine(reportString);
        }
    }


    #region Utility methods

    private static string[] TruncateCities(string[] cities)
    {
        for (int i = 0; i < cities.Length; i++)
        {
            cities[i] = cities[i].TrimEnd(',');
        }
        return cities;
    }

    private static string BuildWeatherReport(WeatherReport wr)
    {
        StringBuilder sb = new();
        sb.AppendLine();
        sb.AppendLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --- Weather in {wr.City}:");
        sb.Append($"\tTemperature: {wr.Temperature}°C");
        sb.Append($"\tPrecipitation: {wr.Precipitation}mm");
        sb.Append($"\tWind Speed: {wr.WindSpeed}m/s");
        sb.Append($"\tIn summary - {wr.Summary}");
        sb.AppendLine();
        return sb.ToString();
    }

    #endregion
}
