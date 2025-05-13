//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Elastic.Clients.Elasticsearch;
//using Elastic.SemanticKernel.Connectors.Elasticsearch;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.Embeddings;
//using Microsoft.SemanticKernel.TextGeneration;

//namespace Rag.SemanticKernel.Core.Sdk.Service;

//public class MistralEmbeddingService
//{
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<MistralEmbeddingService> _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;
//    private readonly string _embeddingModel;
//    private readonly string _completionModel;
//    private readonly string _elasticsearchUrl;
//    private readonly string _elasticsearchIndexName;
//    private readonly ElasticsearchClient _elasticsearchClient;
//    private readonly ElasticsearchVectorStoreRecordCollection<Markdown> _markdownCollection;
//    private readonly IKernel _kernel;

//    public MistralEmbeddingService(IConfiguration configuration, ILogger<MistralEmbeddingService> logger)
//    {
//        try
//        {
//            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

//            // Load configuration values
//            _apiKey = _configuration["Mistral:ApiKey"] ?? throw new InvalidOperationException("Missing Mistral:ApiKey configuration");
//            _embeddingModel = _configuration["Mistral:EmbeddingModel"] ?? "mistral-embed";
//            _completionModel = _configuration["Mistral:CompletionModel"] ?? "mistral-large-latest";
//            _elasticsearchUrl = _configuration["Elasticsearch:Url"] ?? throw new InvalidOperationException("Missing Elasticsearch:Url configuration");
//            _elasticsearchIndexName = _configuration["Elasticsearch:IndexName"] ?? "markdown";

//            // Initialize HTTP client for Mistral API
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            _httpClient.BaseAddress = new Uri("https://api.mistral.ai/v1/");

//            // Initialize Elasticsearch client
//            var settings = new ElasticsearchClientSettings(new Uri(_elasticsearchUrl));
//            _elasticsearchClient = new ElasticsearchClient(settings);

//            // Initialize Semantic Kernel with custom Mistral text embeddings
//            var kernelBuilder = Kernel.CreateBuilder()
//                .AddElasticsearchVectorStore(new ElasticsearchClientSettings(new Uri(_elasticsearchUrl)));

//            // Add custom Mistral text embedding generation
//            kernelBuilder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
//                new CustomMistralEmbeddingService(_httpClient, _embeddingModel, _logger));

//            // Add custom Mistral text completion service
//            kernelBuilder.Services.AddSingleton<ITextGenerationService>(sp =>
//                new CustomMistralCompletionService(_httpClient, _completionModel, _logger));

//            _kernel = kernelBuilder.Build();

//            // Initialize vector store collection
//            _markdownCollection = new ElasticsearchVectorStoreRecordCollection<Markdown>(
//                _elasticsearchClient,
//                _elasticsearchIndexName);

//            _logger.LogInformation("MistralEmbeddingService initialized successfully");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error initializing MistralEmbeddingService");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Stores a markdown document with its embeddings in Elasticsearch
//    /// </summary>
//    /// <param name="fileName">The file name</param>
//    /// <param name="heading">The heading/title of the markdown section</param>
//    /// <param name="content">The content of the markdown section</param>
//    /// <returns>The ID of the stored document</returns>
//    public async Task<string> StoreMarkdownWithEmbeddingsAsync(string fileName, string heading, string content)
//    {
//        try
//        {
//            _logger.LogInformation("Storing markdown document: {FileName}, {Heading}", fileName, heading);

//            if (string.IsNullOrWhiteSpace(content))
//            {
//                _logger.LogWarning("Cannot generate embeddings for empty content");
//                throw new ArgumentException("Content cannot be empty or whitespace", nameof(content));
//            }

//            // Get the text embedding service
//            var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

//            // Generate embeddings for the content
//            var embeddings = await embeddingService.GenerateEmbeddingsAsync(content);
//            var contentEmbedding = embeddings.ToMemory();

//            // Create the markdown record
//            var markdown = new Markdown
//            {
//                Id = Guid.NewGuid().ToString(),
//                FileName = fileName,
//                Heading = heading,
//                Content = content,
//                ContentEmbedding = contentEmbedding
//            };

//            // Store the record in Elasticsearch
//            await _markdownCollection.UpsertAsync(markdown);

//            _logger.LogInformation("Successfully stored markdown document with ID: {Id}", markdown.Id);
//            return markdown.Id;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error storing markdown with embeddings");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Searches for similar markdown documents based on a query
//    /// </summary>
//    /// <param name="query">The search query</param>
//    /// <param name="maxResults">Maximum number of results to return</param>
//    /// <returns>A list of matching markdown documents</returns>
//    public async Task<IEnumerable<Markdown>> SearchSimilarDocumentsAsync(string query, int? maxResults = null)
//    {
//        try
//        {
//            _logger.LogInformation("Searching for documents similar to: {QueryPreview}",
//                query.Length > 50 ? $"{query[..50]}..." : query);

//            // Use configured maxResults if not specified
//            if (!maxResults.HasValue)
//            {
//                if (!int.TryParse(_configuration["SemanticSearch:MaxResults"], out int configMaxResults))
//                {
//                    configMaxResults = 5;
//                }
//                maxResults = configMaxResults;
//            }

//            // Get the text embedding service
//            var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

//            // Generate embeddings for the query
//            var embeddings = await embeddingService.GenerateEmbeddingsAsync(query);
//            var queryEmbedding = embeddings.ToMemory();

//            // Search for similar documents using vector search
//            var searchResults = await _markdownCollection.SearchAsync(
//                queryEmbedding,
//                r => r.WithContentEmbedding(),
//                maxResults.Value);

//            _logger.LogInformation("Found {Count} similar documents", searchResults.Count);
//            return searchResults;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error searching for similar documents");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Initializes the Elasticsearch index with the proper mapping for vector search
//    /// </summary>
//    public async Task InitializeIndexAsync()
//    {
//        try
//        {
//            _logger.LogInformation("Initializing Elasticsearch index: {IndexName}", _elasticsearchIndexName);

//            // Check if index exists
//            var indexExists = await _elasticsearchClient.Indices.ExistsAsync(_elasticsearchIndexName);

//            if (!indexExists.Exists)
//            {
//                _logger.LogInformation("Creating new index: {IndexName}", _elasticsearchIndexName);

//                // Get the vector store service
//                var vectorStoreService = _kernel.GetRequiredService<IElasticsearchVectorStoreService>();

//                // Create the index with appropriate settings for vector search
//                await vectorStoreService.CreateIndexAsync<Markdown>(_elasticsearchIndexName);

//                _logger.LogInformation("Index successfully created");
//            }
//            else
//            {
//                _logger.LogInformation("Index already exists, skipping creation");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error initializing Elasticsearch index");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Generate text completions based on a prompt
//    /// </summary>
//    /// <param name="prompt">The prompt text</param>
//    /// <returns>The generated completion</returns>
//    public async Task<string> GenerateCompletionAsync(string prompt)
//    {
//        try
//        {
//            _logger.LogInformation("Generating completion for prompt: {PromptPreview}",
//                prompt.Length > 50 ? $"{prompt[..50]}..." : prompt);

//            // Get the text generation service
//            var textGenerationService = _kernel.GetRequiredService<ITextGenerationService>();

//            // Generate completion
//            var result = await textGenerationService.GetTextContentAsync(prompt);

//            _logger.LogInformation("Generated completion of length {Length}", result.Length);
//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating completion");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Performs RAG (Retrieval-Augmented Generation) using Elasticsearch and Mistral
//    /// </summary>
//    /// <param name="query">The user query</param>
//    /// <returns>Generated response based on retrieved context</returns>
//    public async Task<string> PerformRagAsync(string query)
//    {
//        try
//        {
//            _logger.LogInformation("Performing RAG for query: {QueryPreview}",
//                query.Length > 50 ? $"{query[..50]}..." : query);

//            // Retrieve relevant documents
//            var similarDocuments = await SearchSimilarDocumentsAsync(query, 3);

//            // Build context from retrieved documents
//            var contextBuilder = new StringBuilder();
//            foreach (var doc in similarDocuments)
//            {
//                contextBuilder.AppendLine($"## {doc.Heading} (from {doc.FileName})");
//                contextBuilder.AppendLine(doc.Content);
//                contextBuilder.AppendLine();
//            }

//            // Create RAG prompt
//            var prompt = $@"You are a helpful AI assistant with knowledge about various topics.
//Based on the following context information, please answer the user's question.
//If you don't know the answer or can't find it in the context, say so honestly.

//CONTEXT:
//{contextBuilder}

//USER QUESTION: {query}

//ANSWER:";

//            // Generate completion with context
//            var response = await GenerateCompletionAsync(prompt);

//            _logger.LogInformation("Generated RAG response of length {Length}", response.Length);
//            return response;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error performing RAG");
//            throw;
//        }
//    }
//}

///// <summary>
///// Custom implementation of ITextEmbeddingGenerationService for Mistral embeddings
///// </summary>
//public class CustomMistralEmbeddingService : ITextEmbeddingGenerationService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _embeddingModel;
//    private readonly ILogger _logger;

//    public CustomMistralEmbeddingService(HttpClient httpClient, string embeddingModel, ILogger logger)
//    {
//        _httpClient = httpClient;
//        _embeddingModel = embeddingModel;
//        _logger = logger;
//    }

//    public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

//    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts, CancellationToken cancellationToken = default)
//    {
//        var result = new List<ReadOnlyMemory<float>>();

//        foreach (var text in texts)
//        {
//            var embedding = await GenerateEmbeddingsAsync(text, cancellationToken);
//            result.Add(embedding);
//        }

//        return result;
//    }

//    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogDebug("Generating embeddings for text: {TextPreview}", text.Length > 50 ? $"{text[..50]}..." : text);

//            var requestBody = JsonSerializer.Serialize(new
//            {
//                model = _embeddingModel,
//                input = text
//            });

//            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
//            var response = await _httpClient.PostAsync("embeddings", content, cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
//                _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
//                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
//            }

//            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
//            var embeddingResponse = JsonSerializer.Deserialize<MistralEmbeddingResponse>(responseContent);

//            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
//            {
//                _logger.LogError("No embeddings returned from Mistral API");
//                throw new InvalidOperationException("No embeddings returned from Mistral API");
//            }

//            _logger.LogDebug("Successfully generated embeddings with {Dimensions} dimensions", embeddingResponse.Data[0].Embedding.Length);
//            return new ReadOnlyMemory<float>(embeddingResponse.Data[0].Embedding);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating embeddings");
//            throw;
//        }
//    }

//    public Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Custom implementation of ITextGenerationService for Mistral completions
///// </summary>
//public class CustomMistralCompletionService : ITextGenerationService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _completionModel;
//    private readonly ILogger _logger;

//    public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

//    public CustomMistralCompletionService(HttpClient httpClient, string completionModel, ILogger logger)
//    {
//        _httpClient = httpClient;
//        _completionModel = completionModel;
//        _logger = logger;
//    }

//    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
//        string prompt,
//        PromptExecutionSettings? executionSettings = null,
//        CancellationToken cancellationToken = default)
//    {
//        var result = await GetTextContentAsync(prompt, executionSettings, cancellationToken);
//        return new List<TextContent> { result }.AsReadOnly();
//    }

//    public async Task<TextContent> GetTextContentAsync(
//        string prompt,
//        PromptExecutionSettings? executionSettings = null,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogDebug("Generating completion for prompt: {PromptPreview}", prompt.Length > 50 ? $"{prompt[..50]}..." : prompt);

//            var requestBody = JsonSerializer.Serialize(new
//            {
//                model = _completionModel,
//                messages = new[]
//                {
//                    new { role = "user", content = prompt }
//                },
//                temperature = executionSettings?.ExtensionData?.TryGetValue("temperature", out var temp) ?? false
//                    ? Convert.ToDouble(temp)
//                    : 0.7,
//                max_tokens = executionSettings?.ExtensionData?.TryGetValue("max_tokens", out var maxTokens) ?? false
//                    ? Convert.ToInt32(maxTokens)
//                    : 1000
//            });

//            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
//            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
//                _logger.LogError("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
//                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
//            }

//            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
//            var completionResponse = JsonSerializer.Deserialize<MistralCompletionResponse>(responseContent);

//            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
//            {
//                _logger.LogError("No completion returned from Mistral API");
//                throw new InvalidOperationException("No completion returned from Mistral API");
//            }

//            var completionText = completionResponse.Choices[0].Message.Content;
//            _logger.LogDebug("Successfully generated completion of length {Length}", completionText.Length);

//            return new TextContent(completionText);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating completion");
//            throw;
//        }
//    }

//    public Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Response model for Mistral embeddings API
///// </summary>
//public class MistralEmbeddingResponse
//{
//    [JsonPropertyName("data")]
//    public List<MistralEmbeddingData> Data { get; set; } = new List<MistralEmbeddingData>();
//}

///// <summary>
///// Data model for Mistral embeddings
///// </summary>
//public class MistralEmbeddingData
//{
//    [JsonPropertyName("embedding")]
//    public float[] Embedding { get; set; } = Array.Empty<float>();
//}

///// <summary>
///// Response model for Mistral chat completions API
///// </summary>
//public class MistralCompletionResponse
//{
//    [JsonPropertyName("id")]
//    public string Id { get; set; } = string.Empty;

//    [JsonPropertyName("object")]
//    public string Object { get; set; } = string.Empty;

//    [JsonPropertyName("created")]
//    public long Created { get; set; }

//    [JsonPropertyName("model")]
//    public string Model { get; set; } = string.Empty;

//    [JsonPropertyName("choices")]
//    public List<MistralCompletionChoice> Choices { get; set; } = new List<MistralCompletionChoice>();
//}

///// <summary>
///// Choice model for Mistral chat completions
///// </summary>
//public class MistralCompletionChoice
//{
//    [JsonPropertyName("index")]
//    public int Index { get; set; }

//    [JsonPropertyName("message")]
//    public MistralCompletionMessage Message { get; set; } = new MistralCompletionMessage();

//    [JsonPropertyName("finish_reason")]
//    public string FinishReason { get; set; } = string.Empty;
//}

///// <summary>
///// Message model for Mistral chat completions
///// </summary>
//public class MistralCompletionMessage
//{
//    [JsonPropertyName("role")]
//    public string Role { get; set; } = string.Empty;

//    [JsonPropertyName("content")]
//    public string Content { get; set; } = string.Empty;
//}