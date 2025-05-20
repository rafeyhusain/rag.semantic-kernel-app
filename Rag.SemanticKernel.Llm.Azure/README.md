# Rag.SemanticKernel.Llm.Azure

This project provides integration with Azure OpenAI services for the Semantic Kernel framework. It includes services for semantic operations and Azure-specific configurations.

## Features

- Azure OpenAI service integration
- Semantic service implementation
- Service extensions for easy configuration

## Configuration

The services can be configured through Azure OpenAI settings in your application configuration.

## Usage

Add the following to your service collection:

```csharp
services.AddAzureSemanticServices(configuration);
```

## Dependencies

- Microsoft.SemanticKernel
- Azure OpenAI SDK 