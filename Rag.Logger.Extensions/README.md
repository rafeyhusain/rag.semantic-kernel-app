# Rag.Logger.Extensions

This folder contains logging extensions and utilities for the RAG system.

## Purpose

The Rag.Logger.Extensions project provides:
- Logging extensions
- Custom logging utilities
- Log formatting
- Log filtering
- Log enrichment

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


## Key Components

- Logging extension methods
- Custom log formatters
- Log filters
- Log enrichers
- Logging configuration

## Usage

This project should be referenced by other projects that need enhanced logging capabilities. It provides a structured way to extend and customize logging within the RAG system.

## Dependencies

- Microsoft.Extensions.Logging 
