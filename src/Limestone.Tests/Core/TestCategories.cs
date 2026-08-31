namespace Limestone.Tests.Core;

/// <summary>
/// Trait keys and values as constants, so a filter string and a test attribute
/// cannot drift apart. CI filters on these: dotnet test --filter "Category=Smoke".
/// </summary>
public static class TestCategories
{
    public const string Key = "Category";

    public const string Ui = "UI";
    public const string Api = "API";
    public const string Smoke = "Smoke";
}
