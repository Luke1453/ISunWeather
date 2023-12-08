using ISunWeather.Common.Entities;

namespace ISunWeather.BLL.Interfaces
{
    public interface IWeatherApiService
    {
        Task<WeatherReport> GetCityWeather(string city);
        Task<string[]> GetValidCities();
    }
}
