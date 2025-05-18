using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

/// <summary>
/// Embedding Service for Mistral embeddings
/// </summary>
public class EmbeddingService : ITextEmbeddingGenerationService
{
    private readonly ILogger<EmbeddingService> _logger;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _embeddingModel;
    private readonly HttpClient _httpClient;

    public EmbeddingService(ILogger<EmbeddingService> logger, string endpoint, string apiKey, string embeddingModel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _endpoint = endpoint ?? "https://api.mistral.ai/v1";
        _apiKey = apiKey ?? throw new InvalidOperationException("Missing Mistral:ApiKey configuration");
        _embeddingModel = embeddingModel ?? throw new InvalidOperationException("Missing Mistral:EmbeddingModel configuration");

        // Initialize HTTP client for Mistral API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();

    public IReadOnlyDictionary<string, object> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = "Mistral",
        ["Endpoint"] = _endpoint,
        ["EmbeddingModel"] = _embeddingModel,
        ["SupportsStreaming"] = true,
        ["SupportsFunctionCalling"] = false,
        ["SupportsEmbeddings"] = true
    };

    /// <summary>
    //    /// Generates embeddings for the specified text using the Mistral API
    //    /// </summary>
    //    /// <param name="text">The text to generate embeddings for</param>
    //    /// <returns>A ReadOnlyMemory<float> containing the embedding vector</returns>
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string text = data[0];

            _logger.LogDebug("Generating embeddings for text: {TextPreview}", text.Length > 50 ? $"{text.Substring(0, 50)}..." : text);

            var requestBody = JsonSerializer.Serialize(new
            {
                model = _embeddingModel,
                input = text
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_endpoint}/embeddings", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
            {
                _logger.LogError("No embeddings returned from Mistral API");
                throw new InvalidOperationException("No embeddings returned from Mistral API");
            }

            _logger.LogDebug("Successfully generated embeddings with {Dimensions} dimensions", embeddingResponse.Data[0].Embedding.Length);
            return [new ReadOnlyMemory<float>(embeddingResponse.Data[0].Embedding)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            throw;
        }
    }
}