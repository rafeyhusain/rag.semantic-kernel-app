using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rag.SemanticKernel.Llm.Mistral;

/// <summary>
/// Chat Completion Service for Mistral
/// </summary>
public class ChatCompletionService : IChatCompletionService, ITextGenerationService
{
    private readonly ILogger<ChatCompletionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ModelSettings _model;

    public ChatCompletionService(ILogger<ChatCompletionService> logger, ModelSettings model)
    {
        _logger = Guard.ThrowIfNull(logger);
        _model = Guard.ThrowIfNull(model);

        // Initialize HTTP client for Mistral API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _model.ApiKey);
    }

    public IReadOnlyDictionary<string, object> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = "Mistral",
        ["Endpoint"] = _model.Endpoint,
        ["CompletionModel"] = _model.CompletionModel,
        ["SupportsStreaming"] = true,
        ["SupportsFunctionCalling"] = false,
        ["SupportsEmbeddings"] = true
    };

    /// <summary>
    /// Gets chat message contents from the Mistral API
    /// </summary>
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating chat completion with model: {Model}", _model.CompletionModel);

            // Convert ChatHistory to Mistral's message format
            var messages = new List<object>();
            foreach (var message in chatHistory)
            {
                string role = message.Role.Label.ToLowerInvariant() switch
                {
                    "user" => "user",
                    "assistant" => "assistant",
                    "system" => "system",
                    _ => "user" // Default to user for unknown roles
                };

                messages.Add(new
                {
                    role,
                    content = message.Content
                });
            }

            // Create request payload
            var requestBody = JsonSerializer.Serialize(new
            {
                model = _model.CompletionModel,
                messages,
                temperature = 0.7,
                max_tokens = 4096,
                top_p = 1.0,
                stream = false
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_model.Endpoint}/chat/completions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var completionResponse = JsonSerializer.Deserialize<MistralChatCompletionResponse>(responseContent);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                _logger.LogError("No completion returned from Mistral API");
                throw new InvalidOperationException("No completion returned from Mistral API");
            }

            var result = new List<ChatMessageContent>
            {
                new ChatMessageContent(
                    AuthorRole.Assistant,
                    completionResponse.Choices[0].Message.Content,
                    metadata: new Dictionary<string, object>
                    {
                        ["Usage"] = completionResponse.Usage,
                        ["Model"] = completionResponse.Model,
                        ["Id"] = completionResponse.Id
                    }
                )
            };

            _logger.LogDebug("Successfully generated chat completion");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat completion");
            throw;
        }
    }

    /// <summary>
    /// Gets streaming chat message contents from the Mistral API
    /// </summary>
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings executionSettings = null,
        Kernel kernel = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        yield break;

        throw new NotImplementedException();

        //try
        //{
        //    _logger.LogDebug("Generating streaming chat completion with model: {Model}", _completionModel);

        //    // Convert ChatHistory to Mistral's message format
        //    var messages = new List<object>();
        //    foreach (var message in chatHistory)
        //    {
        //        string role = message.Role.Label.ToLowerInvariant() switch
        //        {
        //            "user" => "user",
        //            "assistant" => "assistant",
        //            "system" => "system",
        //            _ => "user" // Default to user for unknown roles
        //        };

        //        messages.Add(new
        //        {
        //            role,
        //            content = message.Content
        //        });
        //    }

        //    // Create request payload
        //    var requestBody = JsonSerializer.Serialize(new
        //    {
        //        model = _completionModel,
        //        messages,
        //        temperature = 0.7,
        //        max_tokens = 4096,
        //        top_p = 1.0,
        //        stream = true
        //    });

        //    var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions")
        //    {
        //        Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        //    };

        //    var response = await _httpClient.SendAsync(
        //        request,
        //        HttpCompletionOption.ResponseHeadersRead,
        //        cancellationToken);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        //        _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
        //        throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
        //    }

        //    var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        //    using var reader = new System.IO.StreamReader(stream);

        //    string fullContent = "";

        //    while (!reader.EndOfStream)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        var line = await reader.ReadLineAsync();
        //        if (string.IsNullOrEmpty(line)) continue;
        //        if (!line.StartsWith("data:")) continue;

        //        var json = line.Substring(5).Trim();
        //        if (json == "[DONE]") break;

        //        var streamResponse = JsonSerializer.Deserialize<MistralStreamingChatCompletionResponse>(json);
        //        if (streamResponse?.Choices == null || streamResponse.Choices.Count == 0) continue;

        //        var content = streamResponse.Choices[0].Delta?.Content;
        //        if (string.IsNullOrEmpty(content)) continue;

        //        fullContent += content;

        //        yield return new StreamingChatMessageContent(
        //            AuthorRole.Assistant,
        //            fullContent,
        //            content,
        //            metadata: new Dictionary<string, object>
        //            {
        //                ["Model"] = _completionModel,
        //                ["IsFinal"] = streamResponse.Choices[0].FinishReason != null
        //            });
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Error generating streaming chat completion");
        //    throw;
        //}
    }

    /// <summary>
    /// Gets text contents from the Mistral API
    /// </summary>
    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings executionSettings = null,
        Kernel kernel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating text completion with model: {Model}", _model.CompletionModel);

            // Create a ChatHistory with the prompt as a user message
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            // Use the chat completions endpoint since Mistral mainly works with chat
            var requestBody = JsonSerializer.Serialize(new
            {
                model = _model.CompletionModel,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 4096,
                top_p = 1.0,
                stream = false
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_model.Endpoint}/chat/completions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var completionResponse = JsonSerializer.Deserialize<MistralChatCompletionResponse>(responseContent);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                _logger.LogError("No completion returned from Mistral API");
                throw new InvalidOperationException("No completion returned from Mistral API");
            }

            var result = new List<TextContent>
            {
                new TextContent(
                    completionResponse.Choices[0].Message.Content,
                    metadata: new Dictionary<string, object>
                    {
                        ["Usage"] = completionResponse.Usage,
                        ["Model"] = completionResponse.Model,
                        ["Id"] = completionResponse.Id
                    }
                )
            };

            _logger.LogDebug("Successfully generated text completion");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text completion");
            throw;
        }
    }

    /// <summary>
    /// Gets streaming text contents from the Mistral API
    /// </summary>
    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings executionSettings = null,
        Kernel kernel = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        yield break;

        throw new NotImplementedException();
        //try
        //{
        //    _logger.LogDebug("Generating streaming text completion with model: {Model}", _completionModel);

        //    // Create request payload
        //    var requestBody = JsonSerializer.Serialize(new
        //    {
        //        model = _completionModel,
        //        messages = new[]
        //        {
        //            new { role = "user", content = prompt }
        //        },
        //        temperature = 0.7,
        //        max_tokens = 4096,
        //        top_p = 1.0,
        //        stream = true
        //    });

        //    var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions")
        //    {
        //        Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        //    };

        //    var response = await _httpClient.SendAsync(
        //        request,
        //        HttpCompletionOption.ResponseHeadersRead,
        //        cancellationToken);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        //        _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
        //        throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
        //    }

        //    var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        //    using var reader = new System.IO.StreamReader(stream);

        //    string fullContent = "";

        //    while (!reader.EndOfStream)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        var line = await reader.ReadLineAsync();
        //        if (string.IsNullOrEmpty(line)) continue;
        //        if (!line.StartsWith("data:")) continue;

        //        var json = line.Substring(5).Trim();
        //        if (json == "[DONE]") break;

        //        var streamResponse = JsonSerializer.Deserialize<MistralStreamingChatCompletionResponse>(json);
        //        if (streamResponse?.Choices == null || streamResponse.Choices.Count == 0) continue;

        //        var content = streamResponse.Choices[0].Delta?.Content;
        //        if (string.IsNullOrEmpty(content)) continue;

        //        fullContent += content;

        //        yield return new StreamingTextContent(
        //            fullContent,
        //            content,
        //            metadata: new Dictionary<string, object>
        //            {
        //                ["Model"] = _completionModel,
        //                ["IsFinal"] = streamResponse.Choices[0].FinishReason != null
        //            });
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Error generating streaming text completion");
        //    throw;
        //}
    }
}

// Response classes for Mistral API
public class MistralChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<MistralChatChoice> Choices { get; set; }

    [JsonPropertyName("usage")]
    public MistralUsage Usage { get; set; }
}

public class MistralChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public MistralMessage Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class MistralMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class MistralStreamingChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<MistralStreamingChoice> Choices { get; set; }
}

public class MistralStreamingChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public MistralDelta Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class MistralDelta
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class MistralUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}