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
        if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
        {
            logger.LogDebug("MCP base URL not configured. Skipping external tool registration.");
            return;
        }

        var baseUrl = options.Value.BaseUrl.TrimEnd('/');
        var listUrl = $"{baseUrl}/tools";

        IReadOnlyList<McpToolDescriptor>? tools;
        try
        {
            tools = await httpClient.GetFromJsonAsync<IReadOnlyList<McpToolDescriptor>>(listUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to load MCP tools from {ListUrl}", listUrl);
            return;
        }

        if (tools is null || tools.Count == 0)
        {
            logger.LogInformation("MCP returned no tools.");
            return;
        }

        var functions = tools.Select(tool => KernelFunctionFactory.CreateFromMethod(
            method: (string input) => InvokeToolAsync(baseUrl, tool.Name, input, cancellationToken),
            functionName: tool.Name,
            description: tool.Description ?? "External MCP tool"));

        kernel.Plugins.AddFromFunctions("ExternalTools", functions);
        logger.LogInformation("Registered {Count} MCP tools.", tools.Count);
    }

    private async Task<string> InvokeToolAsync(string baseUrl, string toolName, string input, CancellationToken cancellationToken)
    {
        var payload = new McpToolInvokeRequest { ToolName = toolName, Input = input };
        var response = await httpClient.PostAsJsonAsync($"{baseUrl}/invoke", payload, cancellationToken);

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
