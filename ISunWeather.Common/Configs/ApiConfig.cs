using ISunWeather.Common.Configs.Interfaces;

namespace ISunWeather.Common.Configs
{
    public class ApiConfig : IApiConfig
    {
        public required string BaseUrl { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }

        public required int SessionExpiresInMin { get; set; }

        public required string AuthorizeRoute { get; set; }
        public required string CitiesRoute { get; set; }
        public required string WeathersRoute { get; set; }
    }
}
