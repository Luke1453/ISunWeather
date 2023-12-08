using ISunWeather.Common.Entities;
using System.Text.Json.Serialization;

namespace ISunWeather.Common.JsonContexts;


[JsonSerializable(typeof(WeatherReport))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
public partial class WeatherReportJsonContext : JsonSerializerContext { }
