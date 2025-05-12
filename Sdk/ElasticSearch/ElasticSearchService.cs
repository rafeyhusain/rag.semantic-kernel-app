namespace Rag.SemanticKernel.Core.Sdk.ElasticSearch;

using System;
using System.Threading.Tasks;
using Nest;

public class ElasticSearchService
{
    private readonly ElasticClient _client;
    private readonly string _indexName;

    public ElasticSearchService(string indexName = "my-index")
    {
        _indexName = indexName;

        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex(_indexName);

        _client = new ElasticClient(settings);
    }

    public async Task<bool> CreateIndexAsync()
    {
        var exists = await _client.Indices.ExistsAsync(_indexName);
        if (exists.Exists)
            return false;

        var createIndexResponse = await _client.Indices.CreateAsync(_indexName, c => c
            .Map<DocumentModel>(m => m
                .AutoMap()
            )
        );

        return createIndexResponse.IsValid;
    }

    public async Task<bool> WriteDocumentAsync(DocumentModel doc)
    {
        var response = await _client.IndexDocumentAsync(doc);
        return response.IsValid;
    }

    public async Task<DocumentModel?> ReadDocumentAsync(string id)
    {
        var response = await _client.GetAsync<DocumentModel>(id, g => g.Index(_indexName));
        return response.Found ? response.Source : null;
    }

    public async Task<ISearchResponse<DocumentModel>> SearchAsync(string keyword)
    {
        return await _client.SearchAsync<DocumentModel>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Content)
                    .Query(keyword)
                )
            )
        );
    }

    public async Task<bool> UpdateDocumentAsync(string id, DocumentModel doc)
    {
        var response = await _client.UpdateAsync<DocumentModel>(id, u => u
            .Doc(doc)
        );
        return response.IsValid;
    }

    public async Task<bool> DeleteDocumentAsync(string id)
    {
        var response = await _client.DeleteAsync<DocumentModel>(id);
        return response.IsValid;
    }

    public async Task<bool> DeleteIndexAsync()
    {
        var response = await _client.Indices.DeleteAsync(_indexName);
        return response.IsValid;
    }
}

public class DocumentModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
