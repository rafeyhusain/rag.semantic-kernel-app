# Overview

This document lists milestone delivery plan for *Dotnet application in C# with Semantic Memory framework and Elasticsearch as vector storage*.

## Deliverables Milestone 1 - Build Dotnet app with Semantic Kernel and setup Elasticsearch

### 1. README.md Guide

- How to set up Elasticsearch (local docker)

- How to verify connection between .NET app and Elasticsearch

- How to install and run the app

- Elasticsearch Setup, a working local docker instance of Elasticsearch

- Prerequisites (e.g., .NET version, Elasticsearch version)

### 2. A .NET Core SDK library project

- Semantic Kernel package installed

- Configuration files (like appsettings.json)

- Core packages installed (Serilog, Configuration, NEST)

- Initialized Semantic Kernel (With a placeholder planner function)

### 3. A working .NET Core Console App to test SDK classes

- Connection from .NET SDK to Elasticsearch using a proper .NET client (NEST)

- A test index created via code to prove the app can talk to Elasticsearch

- Project structure laid out (folder structure for services, controllers, etc.)

## Deliverables Milestone 2 - Process Markdown Files and Chunk Content for Embeddings

### 1. Directory Reader Module

- Reads all `.md` files from a specified directory
- Ignores non-Markdown files

1. Markdown Parsing

   - Parses content and headers (e.g., `#`, `##`, `###`) from each Markdown file
   - Metadata extraction: file name, header text, section hierarchy

2. Chunking Strategy

   - Splits content into chunks (by heading or tokens)
   - Keeps reference to the original header and file for citation

3. Semantic Kernel Integration for Chunk Processing

   - Prepares content in a format ready for embedding generation
   - Unit-tested service class or function that can be reused

4. Test Console Output or Logging

   - Shows sample parsed and chunked Markdown content for verification
   - Logged using Serilog

## Deliverables Milestone 3 – Generate Embeddings, Store in Elasticsearch, and Build API

### 1. Embedding Generation

- Integrate Mistral embedding model via Semantic Kernel or API
- Generate vector embeddings for each Markdown chunk

#### 1. Elasticsearch Indexing

- Create a new index (or reuse existing) for storing:
  - Embedding vector
  - Original chunk text
  - Metadata (file name, header, etc.)

- Store vectors using Elasticsearch dense vector fields

1. Web API Project Setup
   - ASP.NET Core Web API with:

     - Controller for answering user prompt
     - Endpoint: `/api/query?prompt=...`

2. Prompt Handling & Semantic Search

   - Accepts a prompt, generates its embedding
   - Performs a vector similarity search in Elasticsearch
   - Returns top-k matching chunks

3. Citation Support

   - Each result includes:

     - Matching chunk
     - File name / header (for source traceability)

4. Basic Swagger UI

   - Testable endpoint from browser or Postman
