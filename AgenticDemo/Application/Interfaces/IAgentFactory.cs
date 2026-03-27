using Microsoft.SemanticKernel.Agents;

namespace AgenticDemo.Application.Interfaces;

public interface IAgentFactory
{
    ChatCompletionAgent CreatePrimaryAgent();
}
