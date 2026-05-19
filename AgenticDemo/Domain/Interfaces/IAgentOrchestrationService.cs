using AgenticDemo.Domain.Models;

namespace AgenticDemo.Domain.Interfaces;

public interface IAgentOrchestrationService
{
    Task<AgentRunResponse> RunAsync(AgentRunRequest request, CancellationToken cancellationToken);
}
