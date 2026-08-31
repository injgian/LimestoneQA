using System.Runtime.CompilerServices;
using Limestone.Tests.Core.Config;
using Limestone.Tests.Core.Drivers;
using Limestone.Tests.Core.Logging;
using Limestone.Tests.Ui.Pages;
using OpenQA.Selenium;
using Xunit.Abstractions;

namespace Limestone.Tests.Ui.Tests;

/// <summary>
/// Owns the driver lifecycle. xUnit constructs a new instance of the test class
/// for every test and disposes it afterwards, so the constructor is the setup,
/// Dispose is the teardown, and the driver field is per-test by construction —
/// there is no shared instance state to guard against.
///
/// Failure evidence is the one place xUnit is weaker than NUnit: Dispose is not
/// told whether the test passed, and there is no attachment API. So the test body
/// is run through Execute(), which catches, captures the evidence and rethrows.
/// The alternative — screenshotting unconditionally in Dispose — costs a second
/// per green test and buries the useful images among hundreds of useless ones.
/// </summary>
public abstract class UiTestBase : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly LoginPage LoginPage;
    protected readonly ITestLog Log;

    protected UiTestBase(ITestOutputHelper output)
    {
        Log = new XunitTestLog(output);
        Driver = DriverFactory.Create(TestConfig.Settings.Ui);
        LoginPage = new LoginPage(Driver);
    }

    /// <summary>
    /// Wraps a test body so a failure carries a screenshot and the failing URL.
    /// The test name is captured by the compiler, so nothing has to be passed in.
    /// </summary>
    protected void Execute(Action body, [CallerMemberName] string testName = "")
    {
        try
        {
            body();
        }
        catch
        {
            CaptureFailureEvidence(testName);
            throw;
        }
    }

    private void CaptureFailureEvidence(string testName)
    {
        try
        {
            Log.Write($"[UI] failing url: {Driver.Url}");

            var directory = Path.Combine(ArtifactsRoot, "screenshots");
            Directory.CreateDirectory(directory);

            var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
            var file = Path.Combine(directory, $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png");

            ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(file);

            // xUnit v2 has no test-attachment API, so the path goes into the test
            // output instead and CI picks the directory up as a build artifact.
            Log.Write($"[UI] screenshot: {file}");
        }
        catch (Exception ex)
        {
            // Evidence capture must never turn a product bug into a framework error.
            Log.Write($"[UI] could not capture screenshot: {ex.Message}");
        }
    }

    private static string ArtifactsRoot =>
        Environment.GetEnvironmentVariable("TEST_ARTIFACTS_DIR")
        ?? Path.Combine(AppContext.BaseDirectory, "TestResults");

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
        GC.SuppressFinalize(this);
    }
}
