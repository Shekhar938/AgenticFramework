using AgenticDemo.Domain.Models;

namespace AgenticDemo.Application.Interfaces;

public interface IAgentOrchestrationService
{
    Task<AgentRunResponse> RunAsync(AgentRunRequest request, CancellationToken cancellationToken);
}
