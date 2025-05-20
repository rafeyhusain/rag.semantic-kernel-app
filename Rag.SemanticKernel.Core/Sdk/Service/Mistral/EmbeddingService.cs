using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

/// <summary>
/// Embedding Service for Mistral embeddings
/// </summary>
public class EmbeddingService : ITextEmbeddingGenerationService
{
    private readonly ILogger<EmbeddingService> _logger;
    private readonly ModelSettings _model;
    private readonly HttpClient _httpClient;

    public EmbeddingService(ILogger<EmbeddingService> logger, ModelSettings model)
    {
        _logger = Guard.ThrowIfNull(logger);
        _model = Guard.ThrowIfNull(model);

        // Initialize HTTP client for Mistral API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _model.ApiKey);
    }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();

    public IReadOnlyDictionary<string, object> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = "Mistral",
        ["Endpoint"] = _model.Endpoint,
        ["EmbeddingModel"] = _model.EmbeddingModel,
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
            _logger.LogDebug("Generating embeddings for {Count} items", data.Count);

            var requestBody = JsonSerializer.Serialize(new
            {
                model = _model.EmbeddingModel,
                input = data  // Send full list
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_model.Endpoint}/embeddings", content, cancellationToken);

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

            _logger.LogDebug("Successfully generated {Count} embeddings", embeddingResponse.Data.Count);

            return embeddingResponse.Data
                .Select(d => new ReadOnlyMemory<float>(d.Embedding))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            throw;
        }
    }

}