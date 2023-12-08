using System.Text.Json.Serialization;

namespace ISunWeather.Common.JsonContexts;


[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
public partial class CitiesJsonContext : JsonSerializerContext { }
