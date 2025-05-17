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
