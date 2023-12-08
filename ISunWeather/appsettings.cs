namespace ISunWeather;

// Currently configuration is done like this (to have only executable file)
// in the future if application will become web or desktop app i would move this config
// to appsettings.json and store it together with executable
internal static class Appsettings
{
    public static string config = @"
{
  ""ApiConfig"": {
    ""BaseUrl"": ""https://weather-api.isun.ch"",
    ""Username"": ""isun"",
    ""Password"": ""passwrod"",
    ""SessionExpiresInMin"": 5,
    ""CitiesRoute"": ""/api/cities"",
    ""AuthorizeRoute"": ""/api/authorize"",
    ""WeathersRoute"": ""/api/weathers/""
  },
  // Working directory can be changed during compilation 
  // it defaults to directory containing the ISun.exe
  ""DataSaverConfig"": {
    ""WorkingDirectoryPath"": """",
    ""Filename"": ""weatherReports.txt""
  }
}";
}
