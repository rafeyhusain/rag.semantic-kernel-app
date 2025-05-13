using Microsoft.SemanticKernel;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using System.Text.Json;

namespace Rag.SemanticKernel.Core.Sdk.Service;

internal class MemoryService
{
    public MemoryService(IConfiguration configuration)
    {
        ElasticsearchUrl = configuration["Elasticsearch:Url"]!;
        ElasticsearchIndexName = configuration["Elasticsearch:IndexName"]!;
    }

    public string ElasticsearchUrl { get; private set; }
    public string ElasticsearchIndexName { get; private set; }

    public void Init()
    {
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddElasticsearchVectorStore(new ElasticsearchClientSettings(new Uri(ElasticsearchUrl)));


        var settings = new ElasticsearchClientSettings(new Uri(ElasticsearchUrl));
        settings.DefaultFieldNameInferrer(name => JsonNamingPolicy.SnakeCaseUpper.ConvertName(name));
        var client = new ElasticsearchClient(settings);

        var collection = new ElasticsearchVectorStoreRecordCollection<Markdown>(
            client,
            ElasticsearchIndexName);
    }
}
