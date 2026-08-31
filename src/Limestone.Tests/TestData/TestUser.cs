using Limestone.Tests.Core.Config;

namespace Limestone.Tests.TestData;

public sealed record TestUser(string Username, string Password);

/// <summary>
/// A minimal builder. On a real product this is where a user would be created
/// through an API or a factory and torn down afterwards, so that tests do not
/// share state. Here it only reads credentials from configuration.
/// </summary>
public sealed class TestUserBuilder
{
    private string _username = TestConfig.Settings.Credentials.StandardUser;
    private string _password = TestConfig.Settings.Credentials.Password;

    public static TestUserBuilder A() => new();

    public TestUserBuilder Standard()
    {
        _username = TestConfig.Settings.Credentials.StandardUser;
        return this;
    }

    public TestUserBuilder LockedOut()
    {
        _username = TestConfig.Settings.Credentials.LockedOutUser;
        return this;
    }

    public TestUserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public TestUserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public TestUser Build() => new(_username, _password);
}
