using System.Net;
using Limestone.Tests.Api.Clients;
using Limestone.Tests.Core;
using Limestone.Tests.Core.Assertions;
using Limestone.Tests.Core.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Limestone.Tests.Api.Tests;

[Trait(TestCategories.Key, TestCategories.Api)]
public sealed class PostsApiTests : IDisposable
{
    private readonly PostsClient _posts;

    public PostsApiTests(ITestOutputHelper output) => _posts = new PostsClient(new XunitTestLog(output));

    public void Dispose() => _posts.Dispose();

    [Fact(DisplayName = "GET /posts?userId=1 returns 200 and only that user's posts, each with a full body")]
    [Trait(TestCategories.Key, TestCategories.Smoke)]
    public async Task GetPostsByUser_ReturnsOnlyThatUsersPosts()
    {
        const int userId = 1;

        var response = await _posts.GetByUserAsync(userId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var posts = response.Data;
        Assert.True(posts is { Count: > 0 }, "Expected at least one post for the user.");

        Verify.ForEach(posts!, (post, index) =>
        {
            Assert.True(post.UserId == userId,
                $"posts[{index}] (id {post.Id}) belongs to user {post.UserId}, but the filter asked for {userId}.");
            Assert.True(post.Id > 0, $"posts[{index}].id should be a positive integer.");
            Assert.False(string.IsNullOrWhiteSpace(post.Title), $"posts[{index}].title is empty.");
            Assert.False(string.IsNullOrWhiteSpace(post.Body), $"posts[{index}].body is empty.");
        });

        var ids = posts!.Select(post => post.Id).ToList();
        Assert.True(ids.Distinct().Count() == ids.Count, "Post ids should be unique.");
    }

    [Theory(DisplayName = "A single post can be fetched by id and its own id round-trips")]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetPostById_ReturnsMatchingPost(int id)
    {
        var response = await _posts.GetByIdAsync(id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Data is not null, "Response body could not be deserialised into a Post.");
        Assert.Equal(id, response.Data!.Id);
    }
}
