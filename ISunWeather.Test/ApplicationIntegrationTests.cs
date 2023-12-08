using NUnit.Framework;
using System.Diagnostics;

namespace ISunWeather.Test;

// Integration tests using whole compiled exe
// Mainly used to check console output
public class ApplicationIntegrationTests
{
    private const string binDirectory = @"C:\Users\Luke\Desktop\git\ISunWeather\ISunWeather\bin\Release\net8.0\win-x64\publish\";
    private static int processID = -1;

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

        var process = Process.Start(processStartInfo) ?? throw new Exception("Failed to start ISun.exe process");
        processID = process.Id;
        return process;
    }

    protected static Task<List<string>> WaitForResponse(Process process)
    {
        return Task.Run(() =>
        {
            var output = process.StandardOutput.ReadToEnd();
            return output.Split(Environment.NewLine)[..^1].ToList();
        });
    }


    [TearDown]
    public void Kill_ISunEXE_Process()
    {
        try
        {
            Process process = Process.GetProcessById(processID);
            if (!process.HasExited)
            {
                try
                {
                    // Kill the process
                    Console.WriteLine($"Killing process with ID {process.Id} and name {process.ProcessName}");
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error killing process: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Process with ID {processID} has already exited.");
            }
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"Process with ID {processID} does not exist.");
        }
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
    public void RunApplication_ValidArguments_PrintsReport(CancellationToken token)
    {
        // Arrange
        var process = StartApplication("--Cities Vilnius");

        // Act
        using var stdout = process.StandardOutput;
        while (!token.IsCancellationRequested)
        {
            var line = stdout.ReadLine();
            if (line?.Contains("--- Weather in Vilnius:") == true)
            {
                Assert.Pass();
            }
        }
    }

    [Test, CancelAfter(10000)]
    public void RunApplication_ValidArguments_CreatesFiles(CancellationToken token)
    {
        // Arrange
        StartApplication("--Cities Vilnius");

        // Act
        while (!token.IsCancellationRequested)
        {
            if (File.Exists($"{binDirectory}log.txt") && File.Exists($"{binDirectory}weatherReports.txt"))
            {
                Assert.Pass();
            }
        }
    }

    [Test, CancelAfter(10000)]
    public void RunApplication_ValidAndInvalidArguments_SortsArguments(CancellationToken token)
    {
        // Arrange
        var process = StartApplication("--Cities Vilnius Kaunas africa asia");
        bool nextValid = false;
        bool nextInvalid = false;

        bool validPassed = false;
        bool invalidPassed = false;

        // Act
        using var stdout = process.StandardOutput;
        while (!token.IsCancellationRequested)
        {
            var line = stdout.ReadLine();
            if (line?.Contains("We have found some invalid cities:") == true)
            {
                nextInvalid = true;
            }
            else if (line?.Contains("Reporting weather using cities:") == true)
            {
                nextValid = true;
            }
            else if (nextInvalid)
            {
                if (line?.Contains("africa, asia") == true)
                {
                    invalidPassed = true;
                }
                nextInvalid = false;
            }
            else if (nextValid)
            {
                if (line?.Contains("Vilnius, Kaunas") == true)
                {
                    validPassed = true;
                }
                nextValid = false;
            }

            if (validPassed && invalidPassed)
            {
                Assert.Pass();
            }
        }
    }

    [Test, CancelAfter(30000)]
    public async Task RunApplication_ValidAndInvalidArguments_TextFileContainsOnlyValid(CancellationToken token)
    {
        // Arrange
        StartApplication("--Cities Vilnius africa");

        // Act
        // wait until file exists 
        while (!token.IsCancellationRequested)
        {
            if (File.Exists($"{binDirectory}weatherReports.txt"))
            {
                break;
            }
        }

        // check file contents
        while (!token.IsCancellationRequested)
        {
            using StreamReader reader = new(
                new FileStream($"{binDirectory}weatherReports.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)


                );
            string file = reader.ReadToEnd();
            var vilniusReportCount = CountOccurrences(file, "--- Weather in Vilnius:", StringComparison.Ordinal);
            bool invalidReportExists = file.Contains("africa");

            if (vilniusReportCount >= 2 && !invalidReportExists)
            {
                // waiting for 2 Vilnius reports
                // to make sure that invalid report doesn't appear
                Assert.Pass();
            }
            if (invalidReportExists)
            {
                Assert.Fail();
            }
            // wait 5 sec and read again
            await Task.Delay(5000, token);
        }
    }

    // Utility Methods
    private static int CountOccurrences(string text, string pattern, StringComparison comparisonType)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(pattern, index, comparisonType)) != -1)
        {
            index += pattern.Length;
            count++;
        }

        return count;
    }
}
