using ISunWeather.BLL.Interfaces;
using ISunWeather.Common.Configs.Interfaces;
using ISunWeather.Common.Entities;
using ISunWeather.Common.JsonContexts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace ISunWeather.BLL.Services;

public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;
    private readonly IApiConfig _apiConfig;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<WeatherApiService> _logger;

    private const string CACHE_KEY = "WeatcherApiAuthorization";
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public WeatherApiService(
        IApiConfig apiConfig,
        IMemoryCache memoryCache,
        ILogger<WeatherApiService> logger,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _memoryCache = memoryCache;
        _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(apiConfig.SessionExpiresInMin));

        _httpClient = clientFactory.CreateClient("WeatherApiClient");

        _httpClient.BaseAddress = new Uri(_apiConfig.BaseUrl);
    }


    public async Task<string[]> GetValidCities()
    {
        _logger.LogInformation("Requesting ISun Weather API for valid cities\nGET:{address}",
            _apiConfig.BaseUrl + _apiConfig.CitiesRoute);

        var message = new HttpRequestMessage(HttpMethod.Get, _apiConfig.CitiesRoute);

        var httpResponseMessage = await MakeAuthorizedRequest(message);
        var responseAsJsonString = await httpResponseMessage.Content.ReadAsStringAsync();
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new Exception($"Get {_apiConfig.BaseUrl + _apiConfig.CitiesRoute}: " +
                $"Returned {httpResponseMessage.StatusCode}");
        }

        var response = JsonSerializer.Deserialize(responseAsJsonString, CitiesJsonContext.Default.StringArray);
        return response
            ?? throw new Exception($"POST {_apiConfig.BaseUrl + _apiConfig.CitiesRoute}: " +
                $"Response does't contain string array");
    }

    public async Task<WeatherReport> GetCityWeather(string city)
    {
        var route = _apiConfig.WeathersRoute + city;
        _logger.LogInformation("Requesting ISun Weather API for weather report in {city}\nGET:{address}",
            city, _apiConfig.BaseUrl + route);
        var message = new HttpRequestMessage(HttpMethod.Get, route);

        var httpResponseMessage = await MakeAuthorizedRequest(message);
        var responseAsJsonString = await httpResponseMessage.Content.ReadAsStringAsync();
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new Exception($"Get {_apiConfig.BaseUrl + route}: " +
                $"Returned {httpResponseMessage.StatusCode}");
        }

        var response = JsonSerializer.Deserialize(responseAsJsonString, WeatherReportJsonContext.Default.WeatherReport);
        return response
            ?? throw new Exception($"POST {_apiConfig.BaseUrl + route}: " +
                $"Response does't contain weather report data");
    }

    private async Task<Cookie> GetApiToken()
    {
        var loginBody = new LoginBody
        {
            Username = _apiConfig.Username,
            Password = _apiConfig.Password
        };
        var bodyContent = new StringContent(
              JsonSerializer.Serialize(loginBody, LoginBodyJsonContext.Default.LoginBody),
              Encoding.UTF8,
              MediaTypeNames.Application.Json);

        _logger.LogInformation("Authenticating into ISun Weather API\nPOST:{address}",
            _apiConfig.BaseUrl + _apiConfig.AuthorizeRoute);
        var httpResponseMessage = await _httpClient.PostAsync(_apiConfig.AuthorizeRoute, bodyContent);
        var responseAsJsonString = await httpResponseMessage.Content.ReadAsStringAsync();
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new Exception($"POST {_apiConfig.BaseUrl + _apiConfig.AuthorizeRoute}: " +
                $"Returned {httpResponseMessage.StatusCode}");
        }

        // Parse response
        JsonDocument jsonDocument = JsonDocument.Parse(responseAsJsonString);
        JsonElement root = jsonDocument.RootElement;
        string token = root.GetProperty("token").GetString()
            ?? throw new Exception($"POST {_apiConfig.BaseUrl + _apiConfig.AuthorizeRoute}: " +
                $"Response does't have field named \"token\"");

        return new Cookie()
        {
            Expires = DateTime.Now.AddMinutes(_apiConfig.SessionExpiresInMin),
            Name = "Authorization",
            Value = token
        };
    }


    #region Token caching methods
    // This logic can be later rewritten to extend IAsyncActionFilter
    // when converting this app to web api

    private async Task<HttpResponseMessage> MakeAuthorizedRequest(HttpRequestMessage message)
    {
        var token = await GetOrSetWeatherApiToken();
        message.Headers.Add("Authorization", $"Bearer {token.Value}");
        return await _httpClient.SendAsync(message);
    }

    private async Task<Cookie> GetOrSetWeatherApiToken()
    {
        _memoryCache.TryGetValue<Cookie>(CACHE_KEY, out var cachedToken);
        if ((cachedToken == null)
            || (cachedToken.Expires < DateTime.Now.AddSeconds(30)))
        {
            var newToken = await GetApiToken();

            _logger.LogInformation("Cashing ISun Weather API Token: {token}",
                newToken.Value);
            _memoryCache.Set(CACHE_KEY, newToken, _cacheEntryOptions);
            return newToken;
        }
        return cachedToken;
    }

    #endregion
}
