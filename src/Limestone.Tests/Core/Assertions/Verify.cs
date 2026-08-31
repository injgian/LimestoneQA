using Xunit.Sdk;

namespace Limestone.Tests.Core.Assertions;

/// <summary>
/// xUnit has no equivalent of NUnit's Assert.Multiple: the first failed assertion
/// ends the test, so a run tells you about one broken field at a time. Verify.All
/// evaluates every assertion, then reports all the failures together — which
/// matters most for the contract checks, where knowing that six fields are missing
/// rather than one is the difference between a diagnosis and another run.
/// </summary>
public static class Verify
{
    public static void All(params Action[] assertions)
    {
        var failures = new List<string>();

        foreach (var assertion in assertions)
        {
            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                failures.Add(ex.Message);
            }
        }

        if (failures.Count == 0) return;

        var report = string.Join(Environment.NewLine,
            failures.Select((message, index) => $"  [{index + 1}] {message.Replace(Environment.NewLine, " ")}"));

        throw new XunitException($"{failures.Count} assertion(s) failed:{Environment.NewLine}{report}");
    }

    /// <summary>Same idea over a collection: every item is checked before anything is thrown.</summary>
    public static void ForEach<T>(IEnumerable<T> items, Action<T, int> assertion)
    {
        var actions = items.Select((item, index) => new Action(() => assertion(item, index)));
        All(actions.ToArray());
    }
}
