using AgenticDemo.Domain.Interfaces;
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
                "You are an autonomous AI assistant with access to tools. " +
                "EXCLUSIVE TOOL RULES: " +
                "1. WEB SEARCH: Use 'SearchPlugin-search_web' for facts and news. " +
                "2. WEB BROWSER (MCP): Use 'puppeteer_browse' if you need to visit a specific URL to read its content. " +
                "3. WEATHER: Use 'WeatherPlugin-get_weather' for ALL temperature data. " +
                "4. MATH: You MUST use 'CalculatorPlugin-calculate'. " +
                "5. FILES: Use 'FileSystemPlugin' to save results on the desktop. " +
                "Be concise, use emojis, and solve the user's request step-by-step.",
            Kernel = kernel.Clone(),
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0.0 // Set back to 0.0 for maximum accuracy and tool adherence
            })
        };
    }
}
