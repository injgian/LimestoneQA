using Limestone.Tests.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Limestone.Tests.Core.Drivers;

/// <summary>
/// The only place that knows how a browser is built. Local vs container vs grid
/// is a configuration difference, not a code difference: set Ui:RemoteUrl and the
/// same options object is shipped to a hub instead of a local binary.
/// Driver binaries are resolved by Selenium Manager, so there is no driver manager package.
/// </summary>
public static class DriverFactory
{
    public static IWebDriver Create(UiSettings settings)
    {
        var browser = Parse(settings.Browser);
        var options = BuildOptions(browser, settings.Headless);

        IWebDriver driver = string.IsNullOrWhiteSpace(settings.RemoteUrl)
            ? CreateLocal(browser, options)
            : new RemoteWebDriver(new Uri(settings.RemoteUrl), options);

        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(settings.PageLoadTimeoutSeconds);
        driver.Manage().Window.Maximize();
        return driver;
    }

    private static IWebDriver CreateLocal(BrowserType browser, DriverOptions options) => browser switch
    {
        BrowserType.Chrome => new ChromeDriver((ChromeOptions)options),
        BrowserType.Firefox => new FirefoxDriver((FirefoxOptions)options),
        _ => throw new NotSupportedException($"Browser '{browser}' is not supported.")
    };

    private static DriverOptions BuildOptions(BrowserType browser, bool headless)
    {
        switch (browser)
        {
            case BrowserType.Chrome:
                var chrome = new ChromeOptions();
                if (headless) chrome.AddArgument("--headless=new");
                chrome.AddArgument("--window-size=1920,1080");
                chrome.AddArgument("--no-sandbox");            // required inside containers
                chrome.AddArgument("--disable-dev-shm-usage");  // avoids /dev/shm exhaustion in Docker
                chrome.AddArgument("--disable-gpu");
                // SauceDemo triggers Chrome's leaked-password dialog on login; it steals focus.
                chrome.AddUserProfilePreference("credentials_enable_service", false);
                chrome.AddUserProfilePreference("profile.password_manager_enabled", false);
                chrome.AddArgument("--disable-search-engine-choice-screen");
                return chrome;

            case BrowserType.Firefox:
                var firefox = new FirefoxOptions();
                if (headless) firefox.AddArgument("-headless");
                return firefox;

            default:
                throw new NotSupportedException($"Browser '{browser}' is not supported.");
        }
    }

    private static BrowserType Parse(string value) =>
        Enum.TryParse<BrowserType>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new NotSupportedException($"Unknown browser '{value}'. Expected: chrome, firefox.");
}
