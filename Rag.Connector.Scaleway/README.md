# Rag.Connector.Scaleway

This project provides integration with Scaleway AI's language models for the Semantic Kernel framework. It includes services for chat completions, embeddings, and question answering capabilities.

## Features

- Chat completion service for text generation
- Embedding generation for vector representations
- Question answering service
- Semantic service extensions for easy integration

## Configuration

The services can be configured through the following options:
- `QuestionServiceOptions`
- `EmbeddingGeneratorServiceOptions`

## Usage

Add the following to your service collection:

```csharp
services.AddScalewayServices(configuration);
```

## Dependencies

- Microsoft.SemanticKernel
- Scaleway AI API client 