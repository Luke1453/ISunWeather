using NUnit.Framework;
using System.Diagnostics;

namespace ISunWeather.Test;

// Integration tests using whole compiled exe
// Mainly used to check console output
public class ApplicationIntegrationTests
{
    private const string binDirectory = @"C:\Users\Luke\Desktop\git\ISunWeather\ISunWeather\bin\Debug\net8.0\";

    protected static Process StartApplication(string args)
    {
        File.Delete($"{binDirectory}log.txt");
        File.Delete($"{binDirectory}weatherReports.txt");

        ProcessStartInfo processStartInfo = new()
        {
            FileName = $"{binDirectory}ISun.exe",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        return Process.Start(processStartInfo) ?? throw new Exception("Failed to start ISun.exe process");
    }

    protected static Task<List<string>> WaitForResponse(Process process)
    {
        return Task.Run(() =>
        {
            var output = process.StandardOutput.ReadToEnd();
            return output.Split(Environment.NewLine)[..^1].ToList();
        });
    }

    [Test]
    public void RunApplication_NoArguments_PrintsError()
    {
        // Arrange
        var process = StartApplication("");

        // Act
        var outputTask = WaitForResponse(process);
        outputTask.Wait();
        var output = outputTask.Result;

        // Assert
        Assert.That(output.LastOrDefault(), Is.EqualTo("No arguments provided."));
    }

    [Test]
    public void RunApplication_InvalidArguments_PrintsError()
    {
        // Arrange
        var process = StartApplication("invalid args");

        // Act
        var outputTask = WaitForResponse(process);
        outputTask.Wait();
        var output = outputTask.Result;

        // Assert
        Assert.That(output.LastOrDefault(), Is.EqualTo("Invalid arguments provided."));
    }

    [Test]
    public void RunApplication_InvalidFirstArgument_PrintsError()
    {
        // Arrange
        var process = StartApplication("--Countries LT EN");

        // Act
        var outputTask = WaitForResponse(process);
        outputTask.Wait();
        var output = outputTask.Result;

        // Assert
        Assert.That(output.LastOrDefault(), Is.EqualTo("Invalid argument: --Countries"));
    }

    [Test, CancelAfter(10000)]
    public void RunApplication_ValidArguments_PrintsReport()
    {
        // Arrange
        var process = StartApplication("--Cities Vilnius");

        // Act
        using var stdout = process.StandardOutput;
        while (true)
        {
            var line = stdout.ReadLine();
            if (line?.Contains("--- Weather in Vilnius:") == true)
            {
                process.Kill();
                Assert.Pass();
            }
        }
    }

    [Test, CancelAfter(10000)]
    public void RunApplication_ValidArguments_CreatesFiles()
    {
        // Arrange
        var process = StartApplication("--Cities Vilnius");

        // Act
        using var stdout = process.StandardOutput;
        while (true)
        {
            var line = stdout.ReadLine();
            if (line?.Contains("--- Weather in Vilnius:") == true)
            {
                if (File.Exists($"{binDirectory}log.txt") && File.Exists($"{binDirectory}weatherReports.txt"))
                {
                    process.Kill();
                    Assert.Pass();
                }
            }
        }
    }
}
