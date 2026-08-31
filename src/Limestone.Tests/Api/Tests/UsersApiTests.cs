using System.Net;
using System.Text.Json;
using Limestone.Tests.Api.Clients;
using Limestone.Tests.Core;
using Limestone.Tests.Core.Assertions;
using Limestone.Tests.Core.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Limestone.Tests.Api.Tests;

[Trait(TestCategories.Key, TestCategories.Api)]
public sealed class UsersApiTests : IDisposable
{
    private readonly UsersClient _users;

    // One instance per test: the client is created and disposed around each one.
    public UsersApiTests(ITestOutputHelper output) => _users = new UsersClient(new XunitTestLog(output));

    public void Dispose() => _users.Dispose();

    [Fact(DisplayName = "GET /users/1 returns 200 and a body that satisfies the user contract")]
    [Trait(TestCategories.Key, TestCategories.Smoke)]
    public async Task GetUser_ReturnsExpectedContract()
    {
        var response = await _users.GetByIdAsync(1);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.ContentType ?? string.Empty);

        var user = response.Data;
        Assert.True(user is not null, "Response body could not be deserialised into a User.");

        Verify.All(
            () => Assert.Equal(1, user!.Id),
            () => Assert.False(string.IsNullOrWhiteSpace(user!.Name), "name is empty."),
            () => Assert.False(string.IsNullOrWhiteSpace(user!.Username), "username is empty."),
            () => Assert.Contains("@", user!.Email ?? string.Empty),
            () => Assert.True(user!.Address is not null, "address object is missing."),
            () => Assert.False(string.IsNullOrWhiteSpace(user!.Address?.City), "address.city is empty."),
            () => Assert.True(user!.Address?.Geo is not null, "address.geo object is missing."),
            () => Assert.True(user!.Company is not null, "company object is missing."),
            () => Assert.False(string.IsNullOrWhiteSpace(user!.Company?.Name), "company.name is empty."));
    }

    [Fact(DisplayName = "GET /users/999 returns 404 rather than an empty 200")]
    public async Task GetUnknownUser_Returns404()
    {
        var response = await _users.GetByIdRawAsync(999);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"An unknown id should be a 404, not {(int)response.StatusCode}.");
    }

    /// <summary>
    /// Shape check done against the raw JSON rather than the model: deserialisation
    /// silently tolerates a missing field, so a contract test has to look at what
    /// actually came back over the wire.
    /// </summary>
    [Fact(DisplayName = "GET /users returns an array whose every item has non-empty key properties")]
    public async Task GetUsers_EveryItemHasRequiredKeys()
    {
        var response = await _users.GetAllAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(response.Content!);
        var root = document.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0, "The users list should not be empty.");

        string[] requiredKeys = ["id", "name", "username", "email", "address", "company"];

        // Every item is checked before anything is thrown, so one run reports
        // every broken field rather than only the first.
        Verify.ForEach(root.EnumerateArray(), (item, index) =>
        {
            foreach (var key in requiredKeys)
            {
                Assert.True(item.TryGetProperty(key, out var value), $"users[{index}] is missing '{key}'.");
                Assert.True(value.ValueKind != JsonValueKind.Null, $"users[{index}].{key} is null.");

                if (value.ValueKind == JsonValueKind.String)
                    Assert.False(string.IsNullOrWhiteSpace(value.GetString()), $"users[{index}].{key} is empty.");
            }
        });

        var ids = root.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();
        Assert.True(ids.Distinct().Count() == ids.Count, "User ids should be unique.");
    }
}
