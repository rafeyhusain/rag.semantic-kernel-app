# Rag.WebApi

This folder contains the Web API implementation for the RAG system.

## Purpose

The Rag.WebApi project provides:
- Web API endpoints
- API controllers
- Request/response handling
- API documentation
- API versioning

## Key Components

- API controllers
- Request handlers
- Response formatters
- API documentation
- Versioning logic

## Usage

This project should be referenced by applications that need to expose Web API endpoints. It provides a structured way to implement and manage Web APIs within the RAG system.

To test Web API:

- Install VS Code with [RESTClient extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
- Run [Rag.WebApi](./Rag.WebApi.csproj) in `Debug` mode
- Use [Question.http](./Tests/Question.http) to send `http` and `https` requests

A **POST** endpoint named `Ask` is available that accepts a string `question` in the request body and returns an `Answer` object.

## Sample JSON Request

```json
{
  "question": "Do the requirements meet the criteria?"
}
```

## Sample JSON Response

```json
{
  "answerText": "My assessment is that the requirements meet the criteria described in sections 1.2 and 3.4.",
  "refs": [
    {
      "fileName": "testing.md",
      "mdHeader": "1.2"
    },
    {
      "fileName": "req3.md",
      "mdHeader": "3.4"
    }
  ]
}
```
