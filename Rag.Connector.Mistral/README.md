# Rag.Connector.Mistral

This folder contains the Mistral AI connector implementation for the RAG system.

## Purpose

The Rag.Connector.Mistral project provides:
- Mistral AI API integration
- Mistral-specific authentication
- Mistral model management
- Mistral-specific error handling

## Key Components

- Mistral API connectors
- Authentication handlers
- Model management utilities
- Mistral-specific configuration
- Error handling for Mistral services

## Usage

This project should be referenced by applications that need to interact with Mistral AI services. It provides a structured way to connect to and use Mistral AI services within the RAG system.

## Features

- Chat completion service for text generation
- Embedding generation for vector representations
- Question answering service
- Semantic service extensions for easy integration

## Configuration

The services can be configured through the following options:
- `QuestionServiceOptions`
- `EmbeddingGeneratorServiceOptions`

## Dependencies

- Microsoft.SemanticKernel
- Mistral AI API client 