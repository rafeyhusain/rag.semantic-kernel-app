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
- Build and Run [Semantic Search Console App](./Rag.SemanticKernel.App/Program.cs)

## Notes

`Elastic.SemanticKernel.Connectors.Elasticsearch` pre-release is not sending `num_candidates` when it builds `search` query. [This](https://github.com/marcarl/rag-elastic/blob/d1bfcf5537d3542166b09039f6b2aae5b962357f/semantic-kernel-net/Elastic.SemanticKernel.Connectors.Elasticsearch/ElasticsearchVectorStoreRecordCollection.cs#L347) line sends it.

```csharp
knnQuery.NumCandidates = 100;
```

In Elasticsearch, when doing vector search using the knn query with HNSW indexing, you must provide `num_candidates`. It is number of candidates to consider during the approximate search.


```bash
curl -X POST "http://localhost:9200/hotels/_search" -H "Content-Type: application/json" -d '
{
  "knn": {
    "field": "embeddings",
    "query_vector": [-0.0268249512, 0.123, ...], 
    "k": 5,
    "num_candidates": 100
  }
}'
```

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
