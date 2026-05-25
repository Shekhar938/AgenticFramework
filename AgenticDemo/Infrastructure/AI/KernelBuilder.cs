using AgenticDemo.Infrastructure.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace AgenticDemo.Infrastructure.AI;

public static class KernelFactory
{
    public static Kernel Build(
        IConfiguration configuration,
        WeatherPlugin weatherPlugin,
        EmailPlugin emailPlugin,
        ActionHistoryPlugin actionHistoryPlugin,
        SearchPlugin searchPlugin,
        FileSystemPlugin fileSystemPlugin,
        SystemInfoPlugin systemInfoPlugin,
        CalculatorPlugin calculatorPlugin,
        HttpClient httpClient)
    {
        var builder = Kernel.CreateBuilder();

        var ollamaEndpoint = configuration["OLLAMA_ENDPOINT"];
        var ollamaModel = configuration["OLLAMA_MODEL"];

        if (!string.IsNullOrWhiteSpace(ollamaEndpoint) && !string.IsNullOrWhiteSpace(ollamaModel))
        {
            httpClient.BaseAddress = new Uri(ollamaEndpoint);
            builder.AddOllamaChatCompletion(
                modelId: ollamaModel,
                httpClient: httpClient);
        }
        else
        {
            var azureEndpoint = configuration["AZURE_OPENAI_ENDPOINT"];
            var azureApiKey = configuration["AZURE_OPENAI_API_KEY"];
            var azureDeployment = configuration["AZURE_OPENAI_DEPLOYMENT"];

            if (!string.IsNullOrWhiteSpace(azureEndpoint) &&
                !string.IsNullOrWhiteSpace(azureApiKey) &&
                !string.IsNullOrWhiteSpace(azureDeployment))
            {
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: azureDeployment,
                    endpoint: azureEndpoint,
                    apiKey: azureApiKey,
                    httpClient: httpClient);
            }
            else
            {
                var modelId = configuration["OPENAI_MODEL"] ?? "gpt-4o-mini";
                var openAiApiKey = configuration["OPENAI_API_KEY"];

                if (string.IsNullOrWhiteSpace(openAiApiKey))
                {
                    throw new InvalidOperationException("Set either Ollama, Azure OpenAI, or OPENAI_API_KEY.");
                }

                builder.AddOpenAIChatCompletion(
                    modelId: modelId, 
                    apiKey: openAiApiKey, 
                    httpClient: httpClient);
            }
        }

        var kernel = builder.Build();

        if (weatherPlugin != null) kernel.Plugins.AddFromObject(weatherPlugin, "WeatherPlugin");
        if (emailPlugin != null) kernel.Plugins.AddFromObject(emailPlugin, "EmailPlugin");
        if (actionHistoryPlugin != null) kernel.Plugins.AddFromObject(actionHistoryPlugin, "ActionHistoryPlugin");
        if (searchPlugin != null) kernel.Plugins.AddFromObject(searchPlugin, "SearchPlugin");
        if (fileSystemPlugin != null) kernel.Plugins.AddFromObject(fileSystemPlugin, "FileSystemPlugin");
        if (systemInfoPlugin != null) kernel.Plugins.AddFromObject(systemInfoPlugin, "SystemInfoPlugin");
        if (calculatorPlugin != null) kernel.Plugins.AddFromObject(calculatorPlugin, "CalculatorPlugin");

        return kernel;
    }
}
