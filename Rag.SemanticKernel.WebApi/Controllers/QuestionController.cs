using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.Connector.Mistral;

#pragma warning disable SKEXP0001 // Some SK methods are still experimental

namespace Rag.SemanticKernel.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class QuestionController : ControllerBase
{
    private readonly ILogger<QuestionController> _logger;
    private readonly SemanticService _semanticService;
    private readonly Kernel _kernel;
    private readonly VectorStoreTextSearch<Markdown> _searchService;

    public QuestionController(
        ILogger<QuestionController> logger,
        Kernel kernel,
        VectorStoreTextSearch<Markdown> searchService,
        SemanticService semanticService)
    {
        _kernel = kernel;
        _searchService = searchService;
        _logger = logger;
        _semanticService = semanticService;

        _kernel.Plugins.Add(searchService.CreateWithGetTextSearchResults("SearchPlugin"));
    }


    [HttpPost("ask")]
    public async Task<ActionResult<Model.Api.AskResponse>> Ask([FromBody] Model.Api.AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var answer = await _semanticService.AskModel(request.Question);

        return Ok(answer);
    }
}
