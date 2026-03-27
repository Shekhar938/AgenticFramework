using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class ActionHistoryPlugin
{
    private readonly ConcurrentQueue<string> _actions = new();

    [KernelFunction("record_action")]
    [Description("Record an action summary into short-term memory")]
    public string RecordAction([Description("Action text to save")] string action)
    {
        _actions.Enqueue($"{DateTimeOffset.UtcNow:u} - {action}");

        while (_actions.Count > 20 && _actions.TryDequeue(out _))
        {
        }

        return "Action recorded.";
    }

    [KernelFunction("get_last_actions")]
    [Description("Gets up to the latest N recorded actions")]
    public string GetLastActions([Description("Number of recent actions")] int count = 2)
    {
        var take = Math.Max(1, count);
        var recent = _actions.Reverse().Take(take).Reverse().ToArray();
        return recent.Length == 0 ? "No prior actions recorded." : string.Join(Environment.NewLine, recent);
    }
}
