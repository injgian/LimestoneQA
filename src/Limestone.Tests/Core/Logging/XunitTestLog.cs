using Xunit.Abstractions;

namespace Limestone.Tests.Core.Logging;

/// <summary>The xUnit adapter. The only file in the framework that knows the runner exists.</summary>
public sealed class XunitTestLog : ITestLog
{
    private readonly ITestOutputHelper _output;

    public XunitTestLog(ITestOutputHelper output) => _output = output;

    public void Write(string message) => _output.WriteLine(message);
}
