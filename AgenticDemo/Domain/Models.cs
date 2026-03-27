namespace AgenticDemo.Domain.Models;

public sealed class AgentRunRequest
{
    public string Prompt { get; init; } = string.Empty;
}

public sealed class AgentRunResponse
{
    public string Result { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Steps { get; init; } = Array.Empty<string>();
}

public sealed class McpToolDescriptor
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class McpToolInvokeRequest
{
    public string ToolName { get; init; } = string.Empty;
    public string Input { get; init; } = string.Empty;
}

public sealed class McpToolInvokeResponse
{
    public string Output { get; init; } = string.Empty;
}
