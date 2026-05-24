using AgenticDemo.Infrastructure.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;

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
#pragma warning disable SKEXP0070
            // Set the base address on the high-timeout HttpClient and use it to create the OllamaApiClient
            httpClient.BaseAddress = new Uri(ollamaEndpoint);
            var ollamaClient = new OllamaApiClient(httpClient);
            ollamaClient.SelectedModel = ollamaModel;
            
            builder.Services.AddKeyedSingleton<IChatCompletionService>(
                serviceKey: null, 
                implementationInstance: ollamaClient.AsChatCompletionService());
#pragma warning restore SKEXP0070
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

        kernel.Plugins.AddFromObject(weatherPlugin, "WeatherPlugin");
        kernel.Plugins.AddFromObject(emailPlugin, "EmailPlugin");
        kernel.Plugins.AddFromObject(actionHistoryPlugin, "ActionHistoryPlugin");
        kernel.Plugins.AddFromObject(searchPlugin, "SearchPlugin");
        kernel.Plugins.AddFromObject(fileSystemPlugin, "FileSystemPlugin");
        kernel.Plugins.AddFromObject(systemInfoPlugin, "SystemInfoPlugin");
        kernel.Plugins.AddFromObject(calculatorPlugin, "CalculatorPlugin");

        return kernel;
    }
}
