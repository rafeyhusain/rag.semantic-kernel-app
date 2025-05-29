using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Rag.Rest;

public class RestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestService> _logger;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;

    public string BaseUrl { get; set; } = "";

    public RestService(HttpClient httpClient, IConfiguration configuration, ILogger<RestService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        int retryCount = configuration.GetValue("Polly:RetryPolicy:RetryCount", 5);
        double retryBaseDelay = configuration.GetValue("Polly:RetryPolicy:BaseDelaySeconds", 2.0);
        int circuitBreakerFailures = configuration.GetValue("Polly:CircuitBreaker:Failures", 5);
        double circuitBreakerDuration = configuration.GetValue("Polly:CircuitBreaker:DurationMinutes", 1.0);

        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryBaseDelay, retryAttempt)));

        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(circuitBreakerFailures, TimeSpan.FromMinutes(circuitBreakerDuration));

        _policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    public void SetApiKey(string? apiKey)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> GetAsync<T>(string endpoint, T requestModel)
    {
        string queryString = "";
        string url = "";

        try
        {
            queryString = ToQueryString(requestModel);
            url = $"{BaseUrl}/{endpoint}?{queryString}";

            var response = await _policy.ExecuteAsync(() => _httpClient.GetAsync(url));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (BrokenCircuitException)
        {
            throw new Exception($"Service {url} is temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while making GET request {url}: {ex.Message}", ex);
        }
    }

    public async Task<string> PostAsync<T>(string endpoint, T requestModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var x = _httpClient.PostAsync($"{BaseUrl}/{endpoint}", content);

            var response = await _policy.ExecuteAsync(() => _httpClient.PostAsync($"{BaseUrl}/{endpoint}", content));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (BrokenCircuitException)
        {
            throw new Exception("Service is temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while making POST request: {ex.Message}", ex);
        }
    }

    private static string ToQueryString<T>(T obj)
    {
        var properties = typeof(T).GetProperties();
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            if (value != null)
            {
                queryString[prop.Name] = value.ToString();
            }
        }

        return queryString.ToString() ?? string.Empty;
    }
}
