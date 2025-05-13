using Microsoft.SemanticKernel;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Rag.SemanticKernel.Core.Sdk.Service;

internal class MemoryService
{
    public MemoryService(IConfiguration configuration) 
    {
        ElasticsearchUrl = configuration["Elasticsearch:Url"]!;
    }

    public string ElasticsearchUrl { get; private set; }

    public void Init() 
    {
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddElasticsearchVectorStore(new ElasticsearchClientSettings(new Uri(ElasticsearchUrl)));
    }
}
