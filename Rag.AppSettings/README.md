# Rag.AppSettings

This folder contains the application settings and configuration management functionality for the RAG system.

## Purpose

The Rag.AppSettings project manages all configuration-related aspects of the application, including:
- Loading and parsing configuration files
- Managing environment-specific settings
- Providing strongly-typed configuration objects
- Handling configuration validation

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

## Key Components

- Configuration models and DTOs
- Configuration loading and parsing logic
- Environment-specific configuration handling
- Configuration validation rules
- Default configuration values

## Usage

This project should be referenced by other projects that need to access application settings and configuration values. It provides a centralized way to manage and access configuration throughout the application.

## Dependencies

- Microsoft.Extensions.Configuration 