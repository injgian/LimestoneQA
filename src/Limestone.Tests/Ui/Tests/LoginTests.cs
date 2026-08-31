using Limestone.Tests.Core;
using Limestone.Tests.Core.Assertions;
using Limestone.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Limestone.Tests.Ui.Tests;

[Trait(TestCategories.Key, TestCategories.Ui)]
public sealed class LoginTests : UiTestBase
{
    public LoginTests(ITestOutputHelper output) : base(output) { }

    [Fact(DisplayName = "A locked out user is rejected with a message rather than let in")]
    [Trait(TestCategories.Key, TestCategories.Smoke)]
    public void LockedOutUser_IsRejected() => Execute(() =>
    {
        var user = TestUserBuilder.A().LockedOut().Build();

        var page = LoginPage.Open().LogInExpectingFailure(user.Username, user.Password);

        Verify.All(
            () => Assert.True(page.IsErrorDisplayed, "No error was shown for a locked out user."),
            () => Assert.Contains("locked out", page.ErrorText, StringComparison.OrdinalIgnoreCase));
    });

    [Fact(DisplayName = "A wrong password is rejected and the user stays on the login page")]
    public void WrongPassword_IsRejected() => Execute(() =>
    {
        var user = TestUserBuilder.A().Standard().WithPassword("definitely_not_the_password").Build();

        var page = LoginPage.Open().LogInExpectingFailure(user.Username, user.Password);

        Assert.Contains("Username and password do not match", page.ErrorText, StringComparison.OrdinalIgnoreCase);
    });
}
