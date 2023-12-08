using ISunWeather.BLL.Interfaces;
using ISunWeather.Common.Configs.Interfaces;
using Microsoft.Extensions.Logging;

namespace ISunWeather.BLL.Services
{
    // Made it IDisposable because didn't see a need
    // to open and close report file everytime I write report to it
    public class DataSaverService : IDataSaverService, IDisposable
    {
        private readonly ILogger<DataSaverService> _logger;
        private readonly string _reportFile;
        private readonly StreamWriter _writer;

        public DataSaverService(
            ILogger<DataSaverService> logger,
            IDataSaverConfig dataSaverConfig)
        {
            _logger = logger;
            _reportFile = GetPath(dataSaverConfig);

            _logger.LogInformation("Weather report file: {filepath}", _reportFile);
            _writer = File.AppendText(_reportFile);
            //_writer = new StreamWriter(new FileStream(_reportFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        public async Task SaveWeatherReport(string wr)
        {
            _logger.LogInformation("Writing weather report to file");
            _writer.WriteLine(wr);
            await _writer.FlushAsync();
        }

        private static string GetPath(IDataSaverConfig dataSaverConfig)
        {
            string directory = string.IsNullOrWhiteSpace(dataSaverConfig.WorkingDirectoryPath)
                ? AppDomain.CurrentDomain.BaseDirectory : dataSaverConfig.WorkingDirectoryPath;

            // check if directory exists
            if (!Directory.Exists(directory))
            {
                throw new Exception($"The directory ({directory}) - does not exist");
            }

            return Path.Combine(directory, dataSaverConfig.Filename);
        }


        #region Disposing Logic

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_writer != null)
                {
                    _writer.Close();
                    _writer.Dispose();
                }
            }
        }

        // Destructor
        ~DataSaverService()
        {
            Dispose(false);
        }

        #endregion
    }
}
