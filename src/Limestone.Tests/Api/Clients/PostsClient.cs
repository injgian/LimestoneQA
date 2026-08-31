using Limestone.Tests.Api.Models;
using Limestone.Tests.Core.Api;
using Limestone.Tests.Core.Logging;
using RestSharp;

namespace Limestone.Tests.Api.Clients;

/// <summary>Owns the /posts endpoints.</summary>
public sealed class PostsClient : ApiClientBase
{
    public PostsClient(ITestLog? log = null) : base(log) { }

    public Task<RestResponse<List<Post>>> GetByUserAsync(int userId) =>
        ExecuteAsync<List<Post>>(new RestRequest("posts").AddQueryParameter("userId", userId.ToString()));

    public Task<RestResponse<Post>> GetByIdAsync(int id) =>
        ExecuteAsync<Post>(new RestRequest("posts/{id}").AddUrlSegment("id", id));
}
