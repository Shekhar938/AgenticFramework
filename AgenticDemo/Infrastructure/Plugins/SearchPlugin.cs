using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class SearchPlugin(
    HttpClient httpClient, 
    IConfiguration configuration, 
    ILogger<SearchPlugin> logger)
{
    private readonly string? _apiKey = configuration["TAVILY_API_KEY"];

    [KernelFunction("search_web")]
    [Description("Searches the web for real-time information using Tavily API")]
    public async Task<string> SearchWebAsync(
        [Description("The search query")] string query)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "Search failed: TAVILY_API_KEY is not configured.";
        }

        logger.LogInformation("Searching web for: {Query}", query);

        try
        {
            var request = new
            {
                api_key = _apiKey,
                query = query,
                search_depth = "basic",
                max_results = 3
            };

            var response = await httpClient.PostAsJsonAsync("https://api.tavily.com/search", request);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"Search failed with status code: {response.StatusCode}";
            }

            var result = await response.Content.ReadFromJsonAsync<TavilyResponse>();
            
            if (result?.Results == null || result.Results.Count == 0)
            {
                return "No results found for your query.";
            }

            var summary = string.Join("\n\n", result.Results.Select(r => $"Source: {r.Url}\nContent: {r.Content}"));
            return summary;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during web search");
            return $"Error performing search: {ex.Message}";
        }
    }

    private sealed class TavilyResponse
    {
        public List<TavilyResult>? Results { get; set; }
    }

    private sealed class TavilyResult
    {
        public string Url { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
