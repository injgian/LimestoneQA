using Limestone.Tests.Api.Models;
using Limestone.Tests.Core.Api;
using Limestone.Tests.Core.Logging;
using RestSharp;

namespace Limestone.Tests.Api.Clients;

/// <summary>Owns the /users endpoints. One method per operation, no assertions.</summary>
public sealed class UsersClient : ApiClientBase
{
    public UsersClient(ITestLog? log = null) : base(log) { }

    public Task<RestResponse<List<User>>> GetAllAsync() =>
        ExecuteAsync<List<User>>(new RestRequest("users"));

    public Task<RestResponse<User>> GetByIdAsync(int id) =>
        ExecuteAsync<User>(new RestRequest("users/{id}").AddUrlSegment("id", id));

    /// <summary>Raw response, for contract checks that inspect the JSON directly.</summary>
    public Task<RestResponse> GetByIdRawAsync(int id) =>
        ExecuteAsync(new RestRequest("users/{id}").AddUrlSegment("id", id));
}
