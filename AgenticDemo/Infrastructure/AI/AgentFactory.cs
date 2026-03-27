using AgenticDemo.Application.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgenticDemo.Infrastructure.AI;

public sealed class AgentFactory(Kernel kernel) : IAgentFactory
{
    public ChatCompletionAgent CreatePrimaryAgent()
    {
        return new ChatCompletionAgent
        {
            Name = "AssistantAgent",
            Instructions =
                "You are an autonomous assistant. Think step-by-step, use available tools when needed, and return concise action summaries.",
            Kernel = kernel.Clone(),
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };
    }
}
