using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.TextGeneration;
using Rag.AppSettings;
using Rag.Connector.Core.Plugins;
using Rag.Guards;
using Rag.Model.Llm.ChatCompletion;
using Rag.Rest;
using System.Text.Json;

namespace Rag.Connector.Core.ChatCompletion;

/// <summary>
/// Chat Completion Service for 
/// </summary>
public class ChatCompletionService<TRecord> : IChatCompletionService, ITextGenerationService
    where TRecord : class
{
    private readonly ILogger<ChatCompletionService<TRecord>> _logger;
    protected ModelPairSettings _pairSettings;
    private readonly Kernel _kernel;
    private readonly RestService _restService;
    private readonly VectorStoreTextSearch<TRecord> _searchService;

    public ChatCompletionService(
        Kernel kernel,
        ILogger<ChatCompletionService<TRecord>> logger,
        RestService restService,
        VectorStoreTextSearch<TRecord> searchService,
        ModelPairSettings pairSettings
        )
    {
        _kernel = Guard.ThrowIfNull(kernel);
        _logger = Guard.ThrowIfNull(logger);
        _restService = Guard.ThrowIfNull(restService);
        _pairSettings = Guard.ThrowIfNull(pairSettings);
        _searchService = Guard.ThrowIfNull(searchService);

        KernelPluginInjector<TRecord>.InjectPlugins(_kernel, _searchService);
    }

    public virtual string PairName => "";

    public void RefreshModelPair()
    {
        if (_pairSettings.Settings != null)
        {
            if (!string.IsNullOrEmpty(_pairSettings.Settings.CurrentPairName))
            {
                _pairSettings = _pairSettings.Settings.CurrentPairSetting;

            }
            else if (!string.IsNullOrEmpty(PairName))
            {
                _pairSettings = _pairSettings.Settings[PairName];
            }
        }

        _restService.BaseUrl = _pairSettings.Endpoint;
        _restService.SetApiKey(_pairSettings.ApiKey);
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
    {
        ["Provider"] = _pairSettings.Name,
        ["Endpoint"] = _pairSettings.Endpoint,
        ["EmbeddingModel"] = _pairSettings.CompletionModel,
        ["SupportsStreaming"] = true,
        ["SupportsFunctionCalling"] = false,
        ["SupportsEmbeddings"] = true
    };

    public async Task<string> Ask(string question, ChatCompletionServiceOptions options)
    {
        try
        {
            _pairSettings.Settings.CurrentPairName = PairName; // This will help SK DI

            var arguments = new KernelArguments
            {
                ["question"] = question
            };

            var response = await _kernel.InvokePromptAsync(
                    promptTemplate: options.Template.Prompt!,
                    arguments: arguments,
                    templateFormat: "handlebars",
                    promptTemplateFactory: new HandlebarsPromptTemplateFactory());

            var result = response.ToString();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ChatCompletion.Ask");
            throw;
        }
    }

    /// <summary>
    /// Gets chat message contents from the  API
    /// </summary>
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating chat completion with model: {Model}", _pairSettings.CompletionModel);

            RefreshModelPair();

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
            var request = new ChatCompletionRequest
                (
                    _pairSettings.CompletionModel,
                    messages,
                    0.7,
                    4096,
                    1.0,
                    false
                );

            var response = await _restService.PostAsync($"chat/completions", request);
            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(response);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                var message = $"No completion returned from {_pairSettings.Endpoint}/chat/completions API";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            var result = new List<ChatMessageContent>
            {
                new(
                    AuthorRole.Assistant,
                    completionResponse.Choices[0].Message.Content,
                    metadata: new Dictionary<string, object?>
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
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
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

        //    var response = await _restService.SendAsync(
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
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating text completion with model: {Model}", _pairSettings.CompletionModel);

            // Create a ChatHistory with the prompt as a user message
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var messages = new List<object>
            {
                new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            // Create request payload
            var request = new ChatCompletionRequest
                (
                    _pairSettings.CompletionModel,
                    messages,
                    0.7,
                    4096,
                    1.0,
                    false
                );

            var response = await _restService.PostAsync($"chat/completions", request);
            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(response);

            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                var message = $"No completion returned from {_pairSettings.Endpoint}/chat/completions API";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            var result = new List<TextContent>
            {
                new(
                    completionResponse.Choices[0].Message.Content,
                    metadata: new Dictionary<string, object?>
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
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
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

        //    var response = await _restService.SendAsync(
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
