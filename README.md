# Semantic Search App

Dotnet C# application with Semantic Memory framework and Elasticsearch as vector storage.

## Details

This is a .Net Console App and Web API. It uses Microsoft Semantic Kernel framework and Elasticsearch as vector storage.

### Console App

- The Console App reads Markdown files from `input` directory.
- Parse headers in Markdown content and convert to chunks for the embedding using models.
- It generate embeddings with [mistral](https://docs.mistral.ai/capabilities/embeddings/) and other models 
- The embeddings are  stored in Elasticsearch indexes

### Web API

- Web API endpoint `/ask` can be used to get answer using [mistral](https://api.mistral.ai/v1) and other models
- It provides citations from prompt input

## Setup Guide

- Install [ElasticSearch](./ElasticSearch/README.md)

## Generate Embeddings

- See [Generate Embeddings](./Rag.App/README.md)

## Run Web API

- See [Run Web API](./Rag.WebApi/README.md)

## Semantic Kernel Notes

See pre-release fix notes at [semantic-kernel-net notes](./semantic-kernel-net/NOTES.md)

## Folders

| No  | Folder                                                                             | Purpose                                             |
| --- | ---------------------------------------------------------------------------------- | --------------------------------------------------- |
| 1   | [Rag.Abstractions](./Rag.Abstractions/README.md)                                   | Core abstractions and interfaces for the RAG system |
| 2   | [Rag.AppSettings](./Rag.AppSettings/README.md)                                     | Application settings and configuration management   |
| 3   | [Rag.CommandLine](./Rag.CommandLine/README.md)                                     | Command-line interface and argument parsing         |
| 4   | [Rag.ConsoleApp.EmbeddingGenerator](./Rag.ConsoleApp.EmbeddingGenerator/README.md) | Console application for generating embeddings       |
| 5   | [Rag.Connector.Azure](./Rag.Connector.Azure/README.md)                             | Azure-specific connector implementations            |
| 6   | [Rag.Connector.Berget](./Rag.Connector.Berget/README.md)                           | Berget-specific connector implementations           |
| 7   | [Rag.Connector.Core](./Rag.Connector.Core/README.md)                               | Core connector functionality and base classes       |
| 8   | [Rag.Connector.Mistral](./Rag.Connector.Mistral/README.md)                         | Mistral AI connector implementation                 |
| 9   | [Rag.Connector.OpenAi](./Rag.Connector.OpenAi/README.md)                           | OpenAI connector implementation                     |
| 10  | [Rag.Connector.Scaleway](./Rag.Connector.Scaleway/README.md)                       | Scaleway-specific connector implementations         |
| 11  | [Rag.Guards](./Rag.Guards/README.md)                                               | Input validation and guard clauses                  |
| 12  | [Rag.Llm.Core](./Rag.Llm.Core/README.md)                                           | Core LLM functionality and abstractions             |
| 13  | [Rag.Llm.Mistral](./Rag.Llm.Mistral/README.md)                                     | Mistral AI LLM implementation                       |
| 14  | [Rag.LlmRouter](./Rag.LlmRouter/README.md)                                         | LLM routing and selection logic                     |
| 15  | [Rag.Logger.Extensions](./Rag.Logger.Extensions/README.md)                         | Logging extensions and utilities                    |
| 16  | [Rag.Model](./Rag.Model/README.md)                                                 | Data models and DTOs                                |
| 17  | [Rag.Parser.Markdown](./Rag.Parser.Markdown/README.md)                             | Markdown parsing and processing                     |
| 18  | [Rag.Rest](./Rag.Rest/README.md)                                                   | REST API implementations                            |
| 19  | [Rag.Startup.ConsoleApp](./Rag.Startup.ConsoleApp/README.md)                       | Console application startup configuration           |
| 20  | [Rag.Startup.WebApp](./Rag.Startup.WebApp/README.md)                               | Web application startup configuration               |
| 21  | [Rag.Template.Handlebar](./Rag.Template.Handlebar/README.md)                       | Handlebars template processing                      |
| 22  | [Rag.WebApi](./Rag.WebApi/README.md)                                               | Web API implementation                              |
