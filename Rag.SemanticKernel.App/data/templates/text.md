---
prompt: |
  Please use this information to answer the question:
  {{#with (SearchPlugin-GetTextSearchResults question)}}
    {{#each this}}
      Name: {{Name}}
      Value: {{Value}}
      Source: {{Link}}
      -----------------
    {{/each}}
  {{/with}}

  Include the source of relevant information in the response.

  Question: {{question}}
---
