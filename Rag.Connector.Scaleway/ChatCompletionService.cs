﻿using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.AppSettings;
using Rag.Rest;

namespace Rag.Connector.Scaleway;

/// <summary>
/// Chat Completion Service for Scaleway
/// </summary>
public class ChatCompletionService : Core.ChatCompletion.ChatCompletionService<Markdown>
{
    public ChatCompletionService(
        Kernel kernel,
        ILogger<ChatCompletionService> logger,
        RestService restService,
        VectorStoreTextSearch<Markdown> searchService,
        ModelPairSettings pairSettings) : base(kernel, logger, restService, searchService, pairSettings)
    {
        RefreshModelPair();
    }

    public override string PairName => Abstractions.Pairs.ModelPairs.Scaleway;
}