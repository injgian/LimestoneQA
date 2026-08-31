using Microsoft.Extensions.Configuration;

namespace Limestone.Tests.Core.Config;

/// <summary>
/// Single entry point for configuration. Sources, last one wins:
///   1. appsettings.json                       (defaults, committed)
///   2. appsettings.{TEST_ENV}.json            (per-environment overrides, optional)
///   3. environment variables                  (CI and secrets, e.g. UI__HEADLESS=false)
/// </summary>
public static class TestConfig
{
    private static readonly Lazy<TestSettings> Instance = new(Build);

    public static TestSettings Settings => Instance.Value;

    /// <summary>Environment the suite is pointed at. Defaults to "local".</summary>
    public static string Environment =>
        System.Environment.GetEnvironmentVariable("TEST_ENV") ?? "local";

    private static TestSettings Build()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return configuration.Get<TestSettings>()
               ?? throw new InvalidOperationException("Unable to bind TestSettings from configuration.");
    }
}
