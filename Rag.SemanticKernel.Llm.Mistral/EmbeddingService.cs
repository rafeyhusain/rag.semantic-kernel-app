using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Rag.SemanticKernel.Abstractions.Parser;
using Rag.SemanticKernel.AppSettings;
using Rag.SemanticKernel.Model.Vector;

namespace Rag.SemanticKernel.Llm.Mistral;

/// <summary>
/// Embedding Service for Mistral embeddings
/// </summary>
public class EmbeddingService<T, TRecord> : Core.Embedding.EmbeddingService<T, TRecord>
    where T : class, IDocument, new()
    where TRecord : class
{
    public EmbeddingService(
        ILogger<EmbeddingService<T, TRecord>> logger,
        Kernel kernel,
        IVectorStoreRecordCollection<string, TRecord> vectorStoreCollection,
        IFileParser parser,
        ModelSettings model
    ) : base(logger, kernel, vectorStoreCollection, parser, model)
    {

    }
}