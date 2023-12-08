using System.Text.Json.Serialization;

namespace ISunWeather.Common.Entities;

public class WeatherReport
{
    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("summary")]
    public required string Summary { get; set; }


    [JsonPropertyName("precipitation")]
    public int Precipitation { get; set; }

    [JsonPropertyName("windSpeed")]
    public int WindSpeed { get; set; }

    [JsonPropertyName("temperature")]
    public int Temperature { get; set; }
}
