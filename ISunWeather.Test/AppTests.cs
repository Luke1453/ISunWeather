using ISunWeather.BLL;
using ISunWeather.Common.Entities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ISunWeather.Test
{
    public class AppTests
    {
        [TestCase(new string[] { }, new string[] { })]
        [TestCase(new string[] { "a,", "b,", "c" }, new string[] { "a", "b", "c" })]
        [TestCase(new string[] { "a,", "b,", "c," }, new string[] { "a", "b", "c" })]
        public void TruncateCities_SouldTrimComma(string[] input, string[] result)
        {
            // Act
            var output = App.TruncateCities(input);

            // Assert
            Assert.That(output, Is.EqualTo(result));
        }

        [Test]
        public void BuildWeatherReport_ShouldReturnCorrectResult()
        {
            // Arrange
            var input = new WeatherReport
            {
                City = "Vilnius",
                Summary = "Cool",
                Precipitation = 84,
                WindSpeed = 6,
                Temperature = 0
            };
            var input2 = new WeatherReport
            {
                City = "Kaunas",
                Summary = "Warm",
                Precipitation = 77,
                WindSpeed = 6,
                Temperature = 18
            };


            // Act
            var output = App.BuildWeatherReport(input);
            var output2 = App.BuildWeatherReport(input2);

            // Assert
            StringAssert.Contains("--- Weather in Vilnius:", output);
            StringAssert.Contains("Temperature: 0°C\tPrecipitation: 84mm\tWind Speed: 6m/s\tIn summary - Cool", output);

            StringAssert.Contains("--- Weather in Kaunas:", output2);
            StringAssert.Contains("Temperature: 18°C\tPrecipitation: 77mm\tWind Speed: 6m/s\tIn summary - Warm", output2);
        }
    }
}
