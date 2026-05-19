using AgenticDemo.Api.Middleware;
using AgenticDemo.Domain.Interfaces;
using AgenticDemo.Application.Services;
using AgenticDemo.Infrastructure.AI;
using AgenticDemo.Infrastructure.Plugins;
using AgenticDemo.MCP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<McpOptions>(builder.Configuration.GetSection("Mcp"));
builder.Services.AddHttpClient<IMcpClientService, McpClientService>();

builder.Services.AddSingleton<WeatherPlugin>();
builder.Services.AddSingleton<EmailPlugin>();
builder.Services.AddSingleton<ActionHistoryPlugin>();

builder.Services.AddSingleton(serviceProvider =>
    KernelFactory.Build(
        builder.Configuration,
        serviceProvider.GetRequiredService<WeatherPlugin>(),
        serviceProvider.GetRequiredService<EmailPlugin>(),
        serviceProvider.GetRequiredService<ActionHistoryPlugin>()));

builder.Services.AddSingleton<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

var app = builder.Build();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
