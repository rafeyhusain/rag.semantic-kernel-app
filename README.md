# Semantic Search App

Dotnet C# application with Semantic Memory framework and Elasticsearch as vector storage.

## Details

Here are the details of Dotnet application in C# with Semantic Memory framework and Elasticsearch as vector storage:

- Dotnet application with Semantic Kernel framework
- Elasticsearch setup
- Read Markdown files from directory
- Parse headers in Markdown content and convert to chunks for the embedding
- Generate embeddings with Mistral [mistral-embed](https://docs.mistral.ai/capabilities/embeddings/) and store in Elasticsearch index
- API endpoint to get answer using [mistral api](https://api.mistral.ai/v1) and citations from prompt input

## Setup Guide

- Install [ElasticSearch](./ElasticSearch/README.md)

## Generate Embeddings

- See [Generate Embeddings](./Rag.SemanticKernel.App/README.md)

## Run Web API

- See [Run Web API](./Rag.SemanticKernel.WebApi/README.md)

## Semantic Kernel Notes

See pre-release fix notes at [semantic-kernel-net notes](./semantic-kernel-net/NOTES.md)

## Folders

| Folder                                                   | Purpose                                                                    |
| -------------------------------------------------------- | -------------------------------------------------------------------------- |
| [Rag.SemanticKernel.App](./Rag.SemanticKernel.App)       | Main console application that implements the semantic search functionality |
| [Rag.SemanticKernel.Core](./Rag.SemanticKernel.Core)     | Core library containing shared business logic and models                   |
| [Rag.SemanticKernel.WebApi](./Rag.SemanticKernel.WebApi) | Web API project exposing the semantic search functionality                 |
| [ElasticSearch](./ElasticSearch)                         | Contains Docker configuration and setup instructions for Elasticsearch     |
| [semantic-kernel-net](./semantic-kernel-net)             | Contains the Elasticsearch connector implementation for Semantic Kernel    |
| [docs](./docs)                                           | Project documentation and additional resources                             |

## Settings

| Name                                 | Purpose                                   | Example Value             |
| ------------------------------------ | ----------------------------------------- | ------------------------- |
| Elasticsearch.Url                    | URL of the Elasticsearch server           | http://localhost:9200     |
| Elasticsearch.User                   | Username for Elasticsearch authentication | ""                        |
| Elasticsearch.Password               | Password for Elasticsearch authentication | ""                        |
| Elasticsearch.Index                  | Name of the Elasticsearch index to use    | hotels                    |
| Mistral.Endpoint                     | Mistral AI API endpoint                   | https://api.mistral.ai/v1 |
| Mistral.ApiKey                       | API key for Mistral AI authentication     | absed3                    |
| Mistral.EmbeddingModel               | Model to use for generating embeddings    | mistral-embed             |
| Mistral.CompletionModel              | Model to use for text completion          | mistral-large-latest      |
| Serilog.MinimumLevel.Default         | Default minimum logging level             | Debug                     |
| Serilog.WriteTo.File.path            | Path for log file output                  | logs/log-.txt             |
| Serilog.WriteTo.File.rollingInterval | How often to create new log files         | Day                       |
| Serilog.Properties.Application       | Application name for logging              | RagSemanticKernel         |

## Logs

The application uses `Serilog` for logging with the following configuration:

- Log files are stored in the `logs` directory with the naming pattern `log-YYYYMMDD.txt`
- Logs are rolled over daily
- Log level is set to Debug by default
- Logs include timestamp, log level, message, and exception details
- Console output is also enabled with a simplified format
- Logs are enriched with machine name and thread ID
- Application name is set to "RagSemanticKernel"

The log file path is configured in `appsettings.json` under the `Serilog` section.
