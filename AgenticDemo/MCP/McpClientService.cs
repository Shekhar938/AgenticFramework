using System.Net.Http.Json;
using AgenticDemo.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace AgenticDemo.MCP;

public interface IMcpClientService
{
    Task RegisterToolsAsync(Kernel kernel, CancellationToken cancellationToken);
}

public sealed class McpClientService(
    HttpClient httpClient,
    IOptions<McpOptions> options,
    ILogger<McpClientService> logger) : IMcpClientService
{
    public async Task RegisterToolsAsync(Kernel kernel, CancellationToken cancellationToken)
    {
        var rawUrl = options.Value.BaseUrl;
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            logger.LogDebug("MCP base URL not configured. Skipping external tool registration.");
            return;
        }

        logger.LogInformation("Attempting to connect to MCP: {Url}", rawUrl);

        // For direct remote MCP links like Tavily's, we might need specific handling
        // But for now, we follow the bridge pattern. If it's a direct URL, we adjust.
        var baseUrl = rawUrl.Contains("?") ? rawUrl.Split('?')[0].TrimEnd('/') : rawUrl.TrimEnd('/');
        var listUrl = rawUrl.Contains("?") ? rawUrl : $"{baseUrl}/tools";

        try
        {
            // First try the bridge pattern (tools endpoint)
            var response = await httpClient.GetAsync(listUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                 logger.LogWarning("MCP tools listing failed with status {Status}", response.StatusCode);
                 return;
            }

            var tools = await response.Content.ReadFromJsonAsync<IReadOnlyList<McpToolDescriptor>>(cancellationToken: cancellationToken);

            if (tools is null || tools.Count == 0)
            {
                logger.LogInformation("MCP returned no tools.");
                return;
            }

            foreach (var tool in tools)
            {
                logger.LogInformation("Registering MCP Tool: {Name}", tool.Name);
                
                kernel.Plugins.AddFromFunctions("ExternalTools", new[] {
                    KernelFunctionFactory.CreateFromMethod(
                        method: (string input) => InvokeToolAsync(baseUrl, tool.Name, input, cancellationToken),
                        functionName: tool.Name,
                        description: tool.Description ?? "External MCP tool")
                });
            }
            
            logger.LogInformation("Successfully registered {Count} MCP tools.", tools.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to load MCP tools from {ListUrl}", listUrl);
        }
    }

    private async Task<string> InvokeToolAsync(string baseUrl, string toolName, string input, CancellationToken cancellationToken)
    {
        var payload = new McpToolInvokeRequest { ToolName = toolName, Input = input };
        
        // Note: For remote MCP, the invoke URL might also need the API key from the query string
        var invokeUrl = options.Value.BaseUrl?.Contains("?") == true 
            ? options.Value.BaseUrl.Replace("?", "invoke?") // Rough approximation
            : $"{baseUrl}/invoke";

        logger.LogInformation("Invoking MCP tool {Tool} at {Url}", toolName, invokeUrl);

        var response = await httpClient.PostAsJsonAsync(invokeUrl, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return $"MCP tool '{toolName}' failed with status {(int)response.StatusCode}.";
        }

        var body = await response.Content.ReadFromJsonAsync<McpToolInvokeResponse>(cancellationToken: cancellationToken);
        return body?.Output ?? $"MCP tool '{toolName}' returned no output.";
    }
}

public sealed class McpOptions
{
    public string? BaseUrl { get; init; }
}
