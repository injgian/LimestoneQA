using System.Text.Json;
using Limestone.Tests.Core.Config;
using Limestone.Tests.Core.Logging;
using RestSharp;

namespace Limestone.Tests.Core.Api;

/// <summary>
/// Transport concerns only: base address, timeout, logging, deserialisation.
/// Endpoint knowledge lives in the typed clients under Api/Clients; assertions
/// live in the tests. Nothing here knows what a "post" is.
/// </summary>
public abstract class ApiClientBase : IDisposable
{
    private readonly RestClient _client;
    private readonly ITestLog _log;

    protected ApiClientBase(ITestLog? log = null, ApiSettings? settings = null)
    {
        _log = log ?? NullTestLog.Instance;
        settings ??= TestConfig.Settings.Api;

        var options = new RestClientOptions(settings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
            ThrowOnAnyError = false // transport failures become assertable results, not exceptions
        };

        _client = new RestClient(options);
    }

    protected async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request)
    {
        var response = await _client.ExecuteAsync<T>(request);
        Log(request, response);
        return response;
    }

    protected async Task<RestResponse> ExecuteAsync(RestRequest request)
    {
        var response = await _client.ExecuteAsync(request);
        Log(request, response);
        return response;
    }

    /// <summary>
    /// Written to the per-test output so a red API test carries its own evidence:
    /// request, status and body are in the failure report, no re-run needed.
    /// </summary>
    private void Log(RestRequest request, RestResponse response)
    {
        var status = response.StatusCode == 0 ? "NO RESPONSE" : $"{(int)response.StatusCode} {response.StatusCode}";
        _log.Write($"[API] {request.Method} {request.Resource} -> {status}");

        if (!string.IsNullOrEmpty(response.ErrorMessage))
            _log.Write($"[API] transport error: {response.ErrorMessage}");

        if (!string.IsNullOrEmpty(response.Content))
            _log.Write($"[API] body: {Truncate(response.Content, 2000)}");
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + $"... [{value.Length - max} more chars]";

    /// <summary>Raw JSON access for contract checks that go beyond the typed model.</summary>
    protected static JsonElement AsJson(RestResponse response) =>
        JsonDocument.Parse(response.Content ?? "{}").RootElement.Clone();

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
