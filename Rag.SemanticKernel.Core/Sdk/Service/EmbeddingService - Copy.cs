//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using Elastic.SemanticKernel.Connectors.Elasticsearch;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.VectorData;
//using Serilog;

//namespace Rag.SemanticKernel.Core.Sdk.Service;

//public class EmbeddingService
//{
//    private readonly IConfiguration _config;
//    private readonly ILogger _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _mistralApiKey;
//    private readonly string _mistralEndpoint;
//    private readonly string _elasticEndpoint;
//    private readonly string _indexName;

//    public EmbeddingService(IConfiguration config)
//    {
//        _config = config;
//        _logger = Log.ForContext<EmbeddingService>();
//        _httpClient = new HttpClient();

//        _mistralApiKey = _config["Mistral:ApiKey"];
//        _mistralEndpoint = _config["Mistral:Endpoint"];
//        _elasticEndpoint = _config["ElasticSearch:Endpoint"];
//        _indexName = _config["ElasticSearch:Index"];

//        if (string.IsNullOrWhiteSpace(_mistralApiKey) || string.IsNullOrWhiteSpace(_mistralEndpoint))
//        {
//            throw new Exception("Mistral API configuration missing.");
//        }
//    }

//    public async Task GenerateAndStoreEmbeddingAsync(string id, string fileName, string heading, string content)
//    {
//        try
//        {
//            var vector = await GetMistralEmbeddingAsync(content);

//            var client = new ElasticsearchVectorStoreClient(new Uri(_elasticEndpoint));
//            var store = new VectorStore<Markdown>(client, _indexName);

//            await store.CreateIndexAsync(); // Only call once per index lifetime (optional safeguard)
//            var record = new Markdown
//            {
//                Id = id,
//                FileName = fileName,
//                Heading = heading,
//                Content = content,
//                ContentEmbedding = new ReadOnlyMemory<float>(vector)
//            };

//            await store.UpsertAsync(record);

//            _logger.Information("Stored embedding in Elastic for ID: {Id}", id);
//        }
//        catch (Exception ex)
//        {
//            _logger.Error(ex, "Failed to process embedding.");
//        }
//    }

//    private async Task<float[]> GetMistralEmbeddingAsync(string input)
//    {
//        var request = new
//        {
//            model = "mistral-embed",
//            input = input
//        };

//        var json = JsonSerializer.Serialize(request);
//        var content = new StringContent(json, Encoding.UTF8, "application/json");
//        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mistralApiKey);

//        var response = await _httpClient.PostAsync(_mistralEndpoint, content);

//        if (!response.IsSuccessStatusCode)
//        {
//            var error = await response.Content.ReadAsStringAsync();
//            _logger.Error("Mistral API error: {Error}", error);
//            throw new Exception("Embedding API call failed.");
//        }

//        var body = await response.Content.ReadAsStringAsync();
//        using var doc = JsonDocument.Parse(body);
//        var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");

//        return embedding.EnumerateArray().Select(e => e.GetSingle()).ToArray();
//    }
//}
