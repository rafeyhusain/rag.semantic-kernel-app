# Web API

To test Web API:

- Install VS Code with [RESTClient extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
- Run [Rag.SemanticKernel.WebApi](./Rag.SemanticKernel.WebApi.csproj) in `Debug` mode
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
