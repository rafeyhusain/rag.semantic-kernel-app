# Rag.Connector.Berget

This folder contains Berget-specific connector implementations for the RAG system.

## Purpose

The Rag.Connector.Berget project provides:
- Berget API integration
- Berget-specific authentication
- Berget resource management
- Berget-specific error handling

## Key Components

- Berget API connectors
- Authentication handlers
- Resource management utilities
- Berget-specific configuration
- Error handling for Berget services

## Usage

This project should be referenced by applications that need to interact with Berget services. It provides a structured way to connect to and use Berget services within the RAG system.

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
- Berget AI API client 