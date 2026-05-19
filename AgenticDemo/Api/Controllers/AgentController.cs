using AgenticDemo.Domain.Interfaces;
using AgenticDemo.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgenticDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AgentController(IAgentOrchestrationService orchestrationService) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(typeof(AgentRunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunAsync([FromBody] AgentRunRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt is required.");
        }

        var result = await orchestrationService.RunAsync(request, cancellationToken);
        return Ok(result);
    }
}
