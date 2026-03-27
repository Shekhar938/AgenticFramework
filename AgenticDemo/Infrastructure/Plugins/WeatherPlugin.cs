using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class WeatherPlugin(ILogger<WeatherPlugin> logger)
{
    [KernelFunction("get_weather")]
    [Description("Gets weather details for a city")]
    public string GetWeather([Description("City name to look up")] string city)
    {
        var report = $"Weather in {city} is sunny, 30°C (demo data).";
        logger.LogInformation("WeatherPlugin.get_weather called for city={City}", city);
        return report;
    }
}
