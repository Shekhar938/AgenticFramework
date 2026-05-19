using AgenticDemo.Domain.Interfaces;
using AgenticDemo.Domain.Models;
using AgenticDemo.MCP;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;

namespace AgenticDemo.Application.Services;

public sealed class AgentOrchestrationService(
    IAgentFactory agentFactory,
    IMcpClientService mcpClientService,
    ILogger<AgentOrchestrationService> logger) : IAgentOrchestrationService
{
    public async Task<AgentRunResponse> RunAsync(AgentRunRequest request, CancellationToken cancellationToken)
    {
        var agent = agentFactory.CreatePrimaryAgent();

        logger.LogInformation("Prompt received: {Prompt}", request.Prompt);

        await mcpClientService.RegisterToolsAsync(agent.Kernel, cancellationToken);

        var history = new ChatHistory();
        history.AddUserMessage(request.Prompt);

        var messageLog = new List<string>();
        await foreach (var response in agent.InvokeAsync(history, cancellationToken: cancellationToken))
        {
            messageLog.Add(response.Content ?? string.Empty);
        }

        var finalMessage = messageLog.LastOrDefault() ?? "No response generated.";

        logger.LogInformation("Agent completed flow with final response: {Response}", finalMessage);

        return new AgentRunResponse
        {
            Result = finalMessage,
            Steps = messageLog
        };
    }
}
