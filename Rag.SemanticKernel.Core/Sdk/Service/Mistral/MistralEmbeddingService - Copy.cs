//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using Elastic.Clients.Elasticsearch;
//using Elastic.SemanticKernel.Connectors.Elasticsearch;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.SemanticKernel;

//namespace Rag.SemanticKernel.Core.Sdk.Service.Mistral;

//public class MistralEmbeddingService
//{
//    private readonly IConfiguration _configuration;
//    private readonly ILogger _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;
//    private readonly string _embeddingModel;
//    private readonly string _elasticsearchUrl;
//    private readonly string _elasticsearchIndexName;
//    private readonly ElasticsearchClient _elasticsearchClient;
//    private readonly ElasticsearchVectorStoreRecordCollection<Markdown> _markdownCollection;

//    public MistralEmbeddingService(IConfiguration configuration, ILogger logger)
//    {
//        try
//        {
//            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

//            // Load configuration values
//            _apiKey = _configuration["AiService:ApiKey"] ?? throw new InvalidOperationException("Missing AiService:ApiKey configuration");
//            _embeddingModel = _configuration["AiService:EmbeddingModel"] ?? "mistral-embed";
//            _elasticsearchUrl = _configuration["Elasticsearch:Url"] ?? throw new InvalidOperationException("Missing Elasticsearch:Url configuration");
//            _elasticsearchIndexName = _configuration["Elasticsearch:IndexName"] ?? "markdown";

//            // Initialize HTTP client for Mistral API
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

//            // Initialize Elasticsearch client
//            var settings = new ElasticsearchClientSettings(new Uri(_elasticsearchUrl));
//            settings.DefaultFieldNameInferrer(name => JsonNamingPolicy.SnakeCaseUpper.ConvertName(name));
//            _elasticsearchClient = new ElasticsearchClient(settings);

//            // Initialize vector store collection
//            _markdownCollection = new ElasticsearchVectorStoreRecordCollection<Markdown>(
//                _elasticsearchClient,
//                _elasticsearchIndexName);

//            _logger.Information("MistralEmbeddingService initialized successfully");
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Error initializing MistralEmbeddingService");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Generates embeddings for the specified text using the Mistral API
//    /// </summary>
//    /// <param name="text">The text to generate embeddings for</param>
//    /// <returns>A ReadOnlyMemory<float> containing the embedding vector</returns>
//    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string text)
//    {
//        try
//        {
//            _logger.Debug("Generating embeddings for text: {TextPreview}", text.Length > 50 ? $"{text.Substring(0, 50)}..." : text);

//            var requestBody = JsonSerializer.Serialize(new
//            {
//                model = _embeddingModel,
//                input = text
//            });

//            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
//            var response = await _httpClient.PostAsync("https://api.mistral.ai/v1/embeddings", content);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errorContent = await response.Content.ReadAsStringAsync();
//                _logger.Error("Error from Mistral API: {StatusCode}, {Response}", response.StatusCode, errorContent);
//                throw new HttpRequestException($"Mistral API error: {response.StatusCode}, {errorContent}");
//            }

//            var responseContent = await response.Content.ReadAsStringAsync();
//            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

//            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
//            {
//                _logger.Error("No embeddings returned from Mistral API");
//                throw new InvalidOperationException("No embeddings returned from Mistral API");
//            }

//            _logger.Debug("Successfully generated embeddings with {Dimensions} dimensions", embeddingResponse.Data[0].Embedding.Length);
//            return new ReadOnlyMemory<float>(embeddingResponse.Data[0].Embedding);
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Error generating embeddings");
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
//            _logger.Information("Storing markdown document: {FileName}, {Heading}", fileName, heading);

//            if (string.IsNullOrWhiteSpace(content))
//            {
//                _logger.Warning("Cannot generate embeddings for empty content");
//                throw new ArgumentException("Content cannot be empty or whitespace", nameof(content));
//            }

//            // Generate embeddings for the content
//            var contentEmbedding = await GenerateEmbeddingsAsync(content);

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

//            _logger.Information("Successfully stored markdown document with ID: {Id}", markdown.Id);
//            return markdown.Id;
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Error storing markdown with embeddings");
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
//            _logger.Information("Searching for documents similar to: {QueryPreview}",
//                query.Length > 50 ? $"{query.Substring(0, 50)}..." : query);

//            // Use configured maxResults if not specified
//            if (!maxResults.HasValue)
//            {
//                if (!int.TryParse(_configuration["SemanticSearch:MaxResults"], out int configMaxResults))
//                {
//                    configMaxResults = 5;
//                }
//                maxResults = configMaxResults;
//            }

//            // Generate embeddings for the query
//            var queryEmbedding = await GenerateEmbeddingsAsync(query);

//            // Search for similar documents using vector search
//            var searchResults = await _markdownCollection.SearchAsync(
//                queryEmbedding,
//                r => r.WithContentEmbedding(),
//                maxResults.Value);

//            _logger.Information("Found {Count} similar documents", searchResults.Count);
//            return searchResults;
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Error searching for similar documents");
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
//            _logger.Information("Initializing Elasticsearch index: {IndexName}", _elasticsearchIndexName);

//            // Check if index exists
//            var indexExists = await _elasticsearchClient.Indices.ExistsAsync(_elasticsearchIndexName);

//            if (!indexExists.Exists)
//            {
//                _logger.Information("Creating new index: {IndexName}", _elasticsearchIndexName);

//                // Create the index with appropriate settings for vector search
//                var kernel = Kernel.CreateBuilder()
//                    .AddElasticsearchVectorStore(new ElasticsearchClientSettings(new Uri(_elasticsearchUrl)))
//                    .Build();

//                // This will automatically create the index with the proper mappings
//                // based on the VectorStore attributes in the Markdown class
//                var serviceProvider = kernel.Services;
//                var vectorStoreService = serviceProvider.GetRequiredService<IElasticsearchVectorStoreService>();
//                await vectorStoreService.CreateIndexAsync<Markdown>(_elasticsearchIndexName);

//                _logger.Information("Index successfully created");
//            }
//            else
//            {
//                _logger.Information("Index already exists, skipping creation");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Error initializing Elasticsearch index");
//            throw;
//        }
//    }
//}
