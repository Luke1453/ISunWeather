using ISunWeather.Common.Entities;
using System.Text.Json.Serialization;

namespace ISunWeather.Common.JsonContexts;


[JsonSerializable(typeof(LoginBody))]
[JsonSerializable(typeof(string))]
public partial class LoginBodyJsonContext : JsonSerializerContext { }
