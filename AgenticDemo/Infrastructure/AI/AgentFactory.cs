using AgenticDemo.Domain.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace AgenticDemo.Infrastructure.AI;

public sealed class AgentFactory(Kernel kernel) : IAgentFactory
{
    public ChatCompletionAgent CreatePrimaryAgent()
    {
        return new ChatCompletionAgent
        {
            Name = "AssistantAgent",
            Instructions =
                "Tools: 'SearchPlugin_search_web', 'ExternalTools_puppeteer_browse', 'WeatherPlugin_get_weather', 'CalculatorPlugin_calculate'. " +
                "Use a tool for any factual query. Answer only after using a tool.",
            Kernel = kernel.Clone(),
            Arguments = new KernelArguments(new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0.0f
            })
        };
    }
}
