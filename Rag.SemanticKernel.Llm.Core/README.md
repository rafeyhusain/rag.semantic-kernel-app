# Rag.SemanticKernel.Llm.Core

This project provides core LLM (Large Language Model) functionality for the Semantic Kernel application. It includes base implementations for chat completions, embeddings, and plugins.

## Structure

- `ChatCompletion/`: Core chat completion implementations
- `Embedding/`: Core embedding generation functionality
- `Plugins/`: Base plugin implementations
- `Service/`: Core service implementations

## Features

- Base LLM service implementations
- Core chat completion functionality
- Embedding generation capabilities
- Plugin system foundation

## Usage

This project serves as the foundation for LLM-related functionality in the Semantic Kernel application. 
Other LLM-specific projects (like Azure and Mistral) build upon these core implementations.

## Dependencies

- Microsoft.SemanticKernel
- Microsoft.Extensions.DependencyInjection 