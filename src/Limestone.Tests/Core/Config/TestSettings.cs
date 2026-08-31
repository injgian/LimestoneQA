namespace Limestone.Tests.Core.Config;

/// <summary>
/// Strongly typed view of appsettings.json. Tests never read raw keys —
/// they ask this object, so a rename breaks the build instead of a run.
/// </summary>
public sealed class TestSettings
{
    public UiSettings Ui { get; init; } = new();
    public ApiSettings Api { get; init; } = new();
    public CredentialSettings Credentials { get; init; } = new();
}

public sealed class UiSettings
{
    public string BaseUrl { get; init; } = "https://www.saucedemo.com/";
    public string Browser { get; init; } = "chrome";
    public bool Headless { get; init; } = true;

    /// <summary>Empty = run a local browser. Set to a Grid/Selenoid hub URL to run remotely.</summary>
    public string RemoteUrl { get; init; } = string.Empty;

    public int PageLoadTimeoutSeconds { get; init; } = 30;
    public int ElementTimeoutSeconds { get; init; } = 10;
}

public sealed class ApiSettings
{
    public string BaseUrl { get; init; } = "https://jsonplaceholder.typicode.com/";
    public int TimeoutSeconds { get; init; } = 30;
}

/// <summary>
/// Demo credentials only. Anything real is supplied by environment variables
/// (CREDENTIALS__PASSWORD) and never committed — see README, Configuration and secrets.
/// </summary>
public sealed class CredentialSettings
{
    public string StandardUser { get; init; } = string.Empty;
    public string LockedOutUser { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
