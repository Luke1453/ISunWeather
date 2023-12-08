namespace ISunWeather.Common.Configs.Interfaces
{
    public interface IApiConfig
    {
        public string BaseUrl { get; }
        public string Username { get; }
        public string Password { get; }

        public int SessionExpiresInMin { get; }

        public string AuthorizeRoute { get; }
        public string CitiesRoute { get; }
        public string WeathersRoute { get; }
    }
}
