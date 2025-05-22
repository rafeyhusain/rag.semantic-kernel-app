using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Guards;
using Rag.SemanticKernel.Model.Llm.ChatCompletion;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rag.SemanticKernel.Llm.Core.ChatCompletion;

/// <summary>
/// Chat Completion Service for 
/// </summary>
public class ChatCompletionService : IChatCompletionService, ITextGenerationService
{
    private readonly ILogger<ChatCompletionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ModelSettings _model;
    private readonly Kernel _kernel;

    public ChatCompletionService(
        Kernel kernel, 
        ILogger<ChatCompletionService> logger, 
        ModelSettings model
        )
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _logger = Guard.ThrowIfNull(logger);
        _model = Guard.ThrowIfNull(model);

        // Initialize HTTP client for  API
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _model.ApiKey);
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = _model.Name,
        ["Endpoint"] = _model.Endpoint,
        ["EmbeddingModel"] = _model.CompletionModel,
        ["SupportsStreaming"] = true,
        ["SupportsFunctionCalling"] = false,
        ["SupportsEmbeddings"] = true
    };

    public async Task<string> Ask(string question, ChatCompletionServiceOptions options)
    {
        try
        {
            return await AskWithRetry(question, options);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in QuestionService.Ask");
            throw;
        }
    }

    public async Task<string> AskWithRetry(string question, ChatCompletionServiceOptions options, int maxRetries = 5)
    {
        int delay = 1000;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var response = await _kernel.InvokePromptAsync(
                        promptTemplate: options.Template.Prompt,
                        arguments: new KernelArguments
                        {
                            { "question", question }
                        },
                        templateFormat: "handlebars",
                        promptTemplateFactory: new HandlebarsPromptTemplateFactory());

                var result = response.ToString();

                return result;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("TooManyRequests"))
            {
                _logger.LogWarning("Rate limited. Retrying in {Delay}ms (Attempt {Attempt}/{Max})", delay, attempt + 1, maxRetries);
                await Task.Delay(delay);
                delay *= 2;
            }
        }

        throw new Exception("Failed to generate embeddings after retries.");
    }

    /// <summary>
    /// Gets chat message contents from the  API
    /// </summary>
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings executionSettings = null, Kernel kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating chat completion with model: {Model}", _model.CompletionModel);

            // Convert ChatHistory to 's message format
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
                _logger.LogError("Error from  API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($" API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                _logger.LogError("No completion returned from  API");
                throw new InvalidOperationException("No completion returned from  API");
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
    /// Gets streaming chat message contents from the  API
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

        //    // Convert ChatHistory to 's message format
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
        //        _logger.LogError("Error from  API: {StatusCode}, {Response}", response.StatusCode, errorContent);
        //        throw new HttpRequestException($" API error: {response.StatusCode}, {errorContent}");
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

        //        var streamResponse = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(json);
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
    /// Gets text contents from the  API
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

            // Use the chat completions endpoint since  mainly works with chat
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
                _logger.LogError("Error from  API: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($" API error: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                _logger.LogError("No completion returned from  API");
                throw new InvalidOperationException("No completion returned from  API");
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
    /// Gets streaming text contents from the  API
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
        //        _logger.LogError("Error from  API: {StatusCode}, {Response}", response.StatusCode, errorContent);
        //        throw new HttpRequestException($" API error: {response.StatusCode}, {errorContent}");
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

        //        var streamResponse = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(json);
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
