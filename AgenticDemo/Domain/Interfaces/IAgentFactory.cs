using Microsoft.SemanticKernel.Agents;

namespace AgenticDemo.Domain.Interfaces;

public interface IAgentFactory
{
    ChatCompletionAgent CreatePrimaryAgent();
}
