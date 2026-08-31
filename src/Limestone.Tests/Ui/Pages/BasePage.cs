using Limestone.Tests.Core.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Limestone.Tests.Ui.Pages;

/// <summary>
/// Shared waiting and interaction primitives for page objects.
/// Page objects expose intent ("LogIn", "AddToCart") and return state or the next
/// page. They never assert and never reference the test runner — that keeps them reusable
/// from any runner and stops test logic leaking into the page layer.
/// </summary>
public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.Settings.Ui.ElementTimeoutSeconds));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    protected IWebElement WaitForVisible(By locator) =>
        Wait.Until(_ =>
        {
            var element = Driver.FindElement(locator);
            return element.Displayed ? element : null;
        })!;

    protected IWebElement WaitForClickable(By locator) =>
        Wait.Until(_ =>
        {
            var element = Driver.FindElement(locator);
            return element.Displayed && element.Enabled ? element : null;
        })!;

    protected IReadOnlyCollection<IWebElement> WaitForAll(By locator) =>
        Wait.Until(_ =>
        {
            var elements = Driver.FindElements(locator);
            return elements.Count > 0 ? elements : null;
        })!;

    protected void Click(By locator) => WaitForClickable(locator).Click();

    protected void Type(By locator, string text)
    {
        var element = WaitForVisible(locator);
        element.Clear();
        element.SendKeys(text);
    }

    protected string TextOf(By locator) => WaitForVisible(locator).Text.Trim();

    protected bool IsDisplayed(By locator)
    {
        try
        {
            return Driver.FindElement(locator).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}
