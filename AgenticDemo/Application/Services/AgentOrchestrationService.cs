using System.Diagnostics;
using AgenticDemo.Domain.Interfaces;
using AgenticDemo.Domain.Models;
using AgenticDemo.MCP;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;

namespace AgenticDemo.Application.Services;

public sealed class AgentOrchestrationService(
    IAgentFactory agentFactory,
    IMcpClientService mcpClientService,
    ILogger<AgentOrchestrationService> logger) : IAgentOrchestrationService
{
    public async Task<AgentRunResponse> RunAsync(AgentRunRequest request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try 
        {
            var agent = agentFactory.CreatePrimaryAgent();

            logger.LogInformation("Prompt received: {Prompt}", request.Prompt);

            await mcpClientService.RegisterToolsAsync(agent.Kernel, cancellationToken);

            var history = new ChatHistory();
            history.AddUserMessage(request.Prompt);

            var messageLog = new List<string>();
            logger.LogInformation("Invoking agent...");
            
            bool toolCalled = true;
            int maxIterations = 5;
            int iteration = 0;

            bool contentReceived = false;
            while (toolCalled && iteration < maxIterations)
            {
                toolCalled = false;
                iteration++;
                logger.LogInformation("--- Agent Reasoning Step {Iteration} ---", iteration);

                await foreach (var response in agent.InvokeAsync(history, cancellationToken: cancellationToken))
                {
                    var content = response.Content ?? string.Empty;

                    if (!string.IsNullOrEmpty(content))
                    {
                        logger.LogInformation("Agent thought: {Content}", content);
                        contentReceived = true;
                        messageLog.Add($"🤔 Thinking: {content}");
                    }

                    if (response.Items != null)
                    {
                        foreach (var item in response.Items)
                        {
                            if (item is Microsoft.SemanticKernel.FunctionCallContent fcc)
                            {
                                logger.LogInformation("Decision: Agent decided to use tool [{Plugin}.{Function}]", fcc.PluginName, fcc.FunctionName);
                                
                                try 
                                {
                                    KernelFunction? targetFunction = null;
                                    string searchPluginName = fcc.PluginName ?? string.Empty;
                                    string searchFunctionName = fcc.FunctionName ?? string.Empty;

                                    if (string.IsNullOrEmpty(searchPluginName) && searchFunctionName.Contains('_'))
                                    {
                                        var parts = searchFunctionName.Split('_', 2);
                                        searchPluginName = parts[0];
                                        searchFunctionName = parts[1];
                                    }

                                    foreach (var plugin in agent.Kernel.Plugins)
                                    {
                                        if (string.Equals(plugin.Name, searchPluginName, StringComparison.OrdinalIgnoreCase) || 
                                            string.IsNullOrEmpty(searchPluginName))
                                        {
                                            foreach (var func in plugin)
                                            {
                                                if (string.Equals(func.Name, searchFunctionName, StringComparison.OrdinalIgnoreCase) ||
                                                    string.Equals(func.Name, fcc.FunctionName, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    targetFunction = func;
                                                    break;
                                                }
                                            }
                                        }
                                        if (targetFunction != null) break;
                                    }

                                    if (targetFunction == null)
                                    {
                                        throw new Exception($"Tool '{searchFunctionName}' not found.");
                                    }

                                    var toolDisplayName = targetFunction.Name == "search_web" ? "Web Search" : 
                                                         targetFunction.Name == "send_email" ? "Email" : targetFunction.Name;

                                    messageLog.Add($"🔍 Action: Decided to use {toolDisplayName}...");

                                    logger.LogInformation("Action: Executing {Plugin}.{Function} with input: {Args}", 
                                        targetFunction.PluginName, targetFunction.Name, fcc.Arguments?.ToString());
                                        
                                    var result = await agent.Kernel.InvokeAsync(targetFunction, fcc.Arguments, cancellationToken);
                                    var resultString = result?.ToString() ?? "Success";
                                    
                                    logger.LogInformation("Result: Tool returned {Length} characters of data.", resultString.Length);
                                    
                                    history.Add(response);
                                    history.Add(new ChatMessageContent(AuthorRole.Tool, resultString, metadata: new Dictionary<string, object?> { { "FunctionName", targetFunction.Name } })); 
                                    
                                    messageLog.Add($"✅ Success: {toolDisplayName} returned information.");
                                    toolCalled = true;
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "  - Tool Execution Failed for {Func}", fcc.FunctionName);
                                    messageLog.Add($"❌ Error: Failed to use tool. {ex.Message}");
                                }
                            }
                        }
                    }
                }

                if (!toolCalled) break;
            }

            if (!contentReceived && messageLog.Count == 0)
            {
                logger.LogWarning("Agent loop finished but no content was received from the model.");
                return new AgentRunResponse { Result = "The model returned an empty response. It might be struggling with the prompt or tool-calling logic.", Steps = new List<string> { "Check if the model supports the current tool-calling format." } };
            }

            var finalMessage = messageLog.LastOrDefault() ?? "No response generated.";

            logger.LogInformation("Agent completed flow in {Elapsed}ms. Final response: {Response}", sw.ElapsedMilliseconds, finalMessage);

            return new AgentRunResponse
            {
                Result = finalMessage,
                Steps = messageLog
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during agent execution after {Elapsed}ms: {Message}", sw.ElapsedMilliseconds, ex.Message);
            return new AgentRunResponse
            {
                Result = $"Error: {ex.Message} (after {sw.ElapsedMilliseconds}ms)",
                Steps = new List<string> { "An error occurred during reasoning. Check backend logs for timing." }
            };
        }
    }
}
