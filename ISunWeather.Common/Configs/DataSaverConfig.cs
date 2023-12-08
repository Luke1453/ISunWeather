using ISunWeather.Common.Configs.Interfaces;

namespace ISunWeather.Common.Configs;

public class DataSaverConfig : IDataSaverConfig
{
    public required string WorkingDirectoryPath { get; set; }
    public required string Filename { get; set; }
}
