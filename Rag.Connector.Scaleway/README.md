# Rag.Connector.Scaleway

This folder contains Scaleway-specific connector implementations for the RAG system.

## Purpose

The Rag.Connector.Scaleway project provides:
- Scaleway API integration
- Scaleway-specific authentication
- Scaleway resource management
- Scaleway-specific error handling

## Key Components

- Scaleway API connectors
- Authentication handlers
- Resource management utilities
- Scaleway-specific configuration
- Error handling for Scaleway services

## Usage

This project should be referenced by applications that need to interact with Scaleway services. It provides a structured way to connect to and use Scaleway services within the RAG system.

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
- Scaleway AI API client 