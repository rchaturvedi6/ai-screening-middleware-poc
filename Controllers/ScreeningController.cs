using AiScreeningMiddleware.Demo.Models;
using AiScreeningMiddleware.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiScreeningMiddleware.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningController : ControllerBase
{
    private static readonly string[] ValidModes = ["DocVerification", "Screening"];
    private readonly ScreeningOrchestrator _orchestrator;
    private readonly ICommentsPersistenceService _comments;

    public ScreeningController(
        ScreeningOrchestrator orchestrator,
        ICommentsPersistenceService comments)
    {
        _orchestrator = orchestrator;
        _comments = comments;
    }

    /// <summary>Runs the synchronous AI middleware flow.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScreeningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ScreeningResult> Post([FromBody] ScreeningRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApplicationId))
        {
            return BadRequest("ApplicationId is required.");
        }

        var normalizedMode = ValidModes.FirstOrDefault(mode =>
            mode.Equals(request.Mode, StringComparison.OrdinalIgnoreCase));

        if (normalizedMode is null)
        {
            return BadRequest("Mode must be either 'DocVerification' or 'Screening'.");
        }

        request.Mode = normalizedMode;
        return Ok(_orchestrator.Process(request));
    }

    /// <summary>Returns all in-memory AI-tagged comments and proves write-back.</summary>
    [HttpGet("comments")]
    public ActionResult<IReadOnlyList<ScreeningComment>> Comments() => Ok(_comments.All());
}
