# Rag.ConsoleApp.EmbeddingGenerator

This folder contains the console application for generating embeddings in the RAG system.

## Purpose

The Rag.ConsoleApp.EmbeddingGenerator project provides:
- Command-line interface for generating embeddings
- Batch processing of documents
- Progress tracking and reporting
- Error handling and logging

## Key Components

- Embedding generation pipeline
- Document processing logic
- Progress reporting
- Error handling
- Logging configuration

## Usage

This console application can be used to generate embeddings for documents that will be used in the RAG system. It provides a command-line interface for specifying input documents and configuration options.

# Generate Embeddings

Follow steps below to generate embeddings with different model pairs.

## `Pairs` Setting

In `appsettings.Development.json`, setup `Pairs` to define various `model-pairs` to generate embeddings.

```json
"Pairs": [
    {
      "Name": "mistral",
      "Endpoint": "https://api.mistral.ai/v1",
      "ApiKey": "your_api_key_here",
      "EmbeddingModel": "mistral-embed",
      "CompletionModel": "mistral-large-latest"
    },
    {
      "Name": "openai",
      "Endpoint": "https://api.openai.com/v1",
      "ApiKey": "your_api_key_here",
      "EmbeddingModel": "text-embedding-3-large",
      "CompletionModel": "gpt-4.1"
    },
    {
      "Name": "scaleway",
      "Endpoint": "https://api.scaleway.ai/v1",
      "ApiKey": "your_api_key_here",
      "EmbeddingModel": "bge-multilingual-gemma2",
      "CompletionModel": "llama-3.1-8b-instruct"
    },
    {
      "Name": "berget",
      "Endpoint": "https://api.berget.ai/v1",
      "ApiKey": "your_api_key_here",
      "EmbeddingModel": "berget-embed",
      "CompletionModel": "berget-chat"
    },
    {
      "Name": "meta",
      "Endpoint": "https://api.meta.ai/v1",
      "ApiKey": "your_api_key_here",
      "EmbeddingModel": "ge-multilingual-gemma2",
      "CompletionModel": "llama-3.3-70b-instruct"
    },
    {
      "Name": "azure",
      "Endpoint": "https://my-service.openai.azure.com",
      "ApiKey": "my_token",
      "CompletionModel": "gpt-4o",
      "EmbeddingModel": "ada-002"
    }
  ],
```


## `model-pair` option

Setup `model-pair` in `Properties\launchSettings.json` to use a different model pair for embedding generation. 
e.g. `mistral` is set below

```
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Development": {
      "commandName": "Project",
      "commandLineArgs": "model-pair=mistral",
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## Run

- Place `.md` files in folder `data\input`, relative to `Rag.App.exe`
- Run `Rag.App.exe`
- A folder `completed` is created and `.md` files will be moved, once embeddings are generated.
- If you run program in debug mode, `.md` files are always copied in folder `data\input`, if newer
