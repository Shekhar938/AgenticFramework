using AgenticDemo.Infrastructure.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.AI;

public static class KernelFactory
{
    public static Kernel Build(
        IConfiguration configuration,
        WeatherPlugin weatherPlugin,
        EmailPlugin emailPlugin,
        ActionHistoryPlugin actionHistoryPlugin)
    {
        var builder = Kernel.CreateBuilder();

        var azureEndpoint = configuration["AZURE_OPENAI_ENDPOINT"];
        var azureApiKey = configuration["AZURE_OPENAI_API_KEY"];
        var azureDeployment = configuration["AZURE_OPENAI_DEPLOYMENT"];

        if (!string.IsNullOrWhiteSpace(azureEndpoint) &&
            !string.IsNullOrWhiteSpace(azureApiKey) &&
            !string.IsNullOrWhiteSpace(azureDeployment))
        {
            builder.AddAzureOpenAIChatCompletion(
                azureDeployment,
                azureEndpoint,
                azureApiKey);
        }
        else
        {
            var modelId = configuration["OPENAI_MODEL"] ?? "gpt-4o-mini";
            var openAiApiKey = configuration["OPENAI_API_KEY"];

            if (string.IsNullOrWhiteSpace(openAiApiKey))
            {
                throw new InvalidOperationException("Set either Azure OpenAI settings or OPENAI_API_KEY.");
            }

            builder.AddOpenAIChatCompletion(modelId, openAiApiKey);
        }

        var kernel = builder.Build();

        kernel.Plugins.AddFromObject(weatherPlugin, "WeatherPlugin");
        kernel.Plugins.AddFromObject(emailPlugin, "EmailPlugin");
        kernel.Plugins.AddFromObject(actionHistoryPlugin, "ActionHistoryPlugin");

        return kernel;
    }
}
