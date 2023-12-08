namespace ISunWeather.Common.Configs.Interfaces;

public interface IDataSaverConfig
{
    public string WorkingDirectoryPath { get; }
    public string Filename { get; }
}
