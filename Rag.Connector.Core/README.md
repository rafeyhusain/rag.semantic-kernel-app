# Rag.Connector.Core

This folder contains the core connector functionality and base classes for the RAG system.

## Purpose

The Rag.Connector.Core project provides:
- Base connector interfaces and abstract classes
- Common connector functionality
- Shared utilities and helpers
- Core connector types and models

## Key Components

- Base connector interfaces
- Abstract connector classes
- Common connector utilities
- Shared configuration models
- Error handling base classes

## Usage

This project should be referenced by all connector implementations. It provides the foundation and common functionality that all connectors should implement and use.

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

## Dependencies

- Microsoft.SemanticKernel
- Microsoft.Extensions.DependencyInjection 