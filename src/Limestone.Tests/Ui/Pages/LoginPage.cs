using Limestone.Tests.Core.Config;
using OpenQA.Selenium;

namespace Limestone.Tests.Ui.Pages;

public sealed class LoginPage : BasePage
{
    private static readonly By Username = By.Id("user-name");
    private static readonly By Password = By.Id("password");
    private static readonly By SubmitButton = By.Id("login-button");
    private static readonly By ErrorMessage = By.CssSelector("[data-test='error']");

    public LoginPage(IWebDriver driver) : base(driver) { }

    public LoginPage Open()
    {
        Driver.Navigate().GoToUrl(TestConfig.Settings.Ui.BaseUrl);
        WaitForVisible(Username);
        return this;
    }

    /// <summary>Happy path: returns the page the user lands on.</summary>
    public InventoryPage LogInAs(string user, string password)
    {
        SubmitCredentials(user, password);
        return new InventoryPage(Driver);
    }

    /// <summary>Negative path: stays on the login page so the error can be inspected.</summary>
    public LoginPage LogInExpectingFailure(string user, string password)
    {
        SubmitCredentials(user, password);
        return this;
    }

    public string ErrorText => TextOf(ErrorMessage);

    public bool IsErrorDisplayed => IsDisplayed(ErrorMessage);

    private void SubmitCredentials(string user, string password)
    {
        Type(Username, user);
        Type(Password, password);
        Click(SubmitButton);
    }
}
