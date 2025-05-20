using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System;

namespace Rag.SemanticKernel.Model.Vector;

public class MarkdownDocument : IDocument
{
    public MarkdownDocument()
    {
        MarkdownId = string.Empty;
        FileName = string.Empty;
        Content = string.Empty;
        Text = string.Empty;
        Url = string.Empty;
    }

    [VectorStoreRecordKey]
    public string MarkdownId { get; set; }

    [TextSearchResultName]
    [VectorStoreRecordData(IsFilterable = true)]
    public string FileName { get; set; }

    [TextSearchResultValue]
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public string Content { get; set; }

    [VectorStoreRecordVector(Dimensions: 1024, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embeddings { get; set; }

    [TextSearchResultLink]
    [VectorStoreRecordData]
    public string Text { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string Url { get; set; }

    public Markdown Record => new Markdown
    {
        MarkdownId = MarkdownId,
        FileName = FileName,
        Url = Url,
        Text = Text,
        Content = Content,
        Embeddings = Embeddings
    };

    public MarkdownDocument(string markdownId, string fileName, string filePath, string text, string content, ReadOnlyMemory<float> embeddings)
    {
        MarkdownId = markdownId;
        FileName = fileName;
        Url = filePath;
        Text = text;
        Content = content;
        Embeddings = embeddings;
    }
}
