namespace Limestone.Tests.Core.Logging;

/// <summary>
/// The framework core logs through this, not through xUnit directly.
/// xUnit writes per-test output via ITestOutputHelper, which is injected into a
/// test class constructor and is not reachable from a static context — so the
/// core would otherwise have to take a dependency on the runner. One small
/// interface keeps Core/ runner-agnostic: swapping xUnit for anything else means
/// writing one adapter, not touching the clients or pages.
/// </summary>
public interface ITestLog
{
    void Write(string message);
}

/// <summary>Used when no logger was supplied, e.g. a client built outside a test.</summary>
public sealed class NullTestLog : ITestLog
{
    public static readonly ITestLog Instance = new NullTestLog();
    private NullTestLog() { }
    public void Write(string message) { }
}
