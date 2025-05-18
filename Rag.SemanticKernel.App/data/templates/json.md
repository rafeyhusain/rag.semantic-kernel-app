---
prompt: |
  Please use this information to answer the question:
  {
    "answer": "{{answer}}",
    "refs": [
    {{#each (SearchPlugin-GetTextSearchResults question)}}
      {
        "fileName": "{{Name}}",
        "mdHeader": "{{Link}}"
      }{{#unless @last}},{{/unless}}
    {{/each}}
    ]
  }

  Only provide above json in response.
  Provide reponse text in json field "answer"

  Question: {{question}}
---
