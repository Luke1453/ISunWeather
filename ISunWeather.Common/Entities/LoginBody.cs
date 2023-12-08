using System.Text.Json.Serialization;

namespace ISunWeather.Common.Entities;

public class LoginBody
{
    [JsonPropertyName("Username")]
    public required string Username { get; set; }

    [JsonPropertyName("Password")]
    public required string Password { get; set; }
}
