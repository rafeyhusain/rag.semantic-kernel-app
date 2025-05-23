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

  Only return above json in response without any ```json or ```.
  Json field "answer" must have following two parts:
  
  1- A fixed text: My assessment is that the requirements meet the criteria described in sections 

  2- A comma separated list of {{Link}}

  Question: {{question}}
---
