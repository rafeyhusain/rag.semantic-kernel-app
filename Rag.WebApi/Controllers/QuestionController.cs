using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.Connector.Mistral;
using Rag.LlmRouter;
using Rag.Guards;

#pragma warning disable SKEXP0001 // Some SK methods are still experimental

namespace Rag.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class QuestionController : ControllerBase
{
    private readonly ILogger<QuestionController> _logger;
    private readonly Router _router;

    public QuestionController(
        ILogger<QuestionController> logger,
        Router router)
    {
        _logger = Guard.ThrowIfNull(logger);
        _router = Guard.ThrowIfNull(router);
    }


    [HttpPost("ask")]
    public async Task<ActionResult<Model.Api.AskResponse>> Ask([FromBody] Model.Api.AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PairName) || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var answer = await _router.AskModel(request);

        return Ok(answer);
    }
}
