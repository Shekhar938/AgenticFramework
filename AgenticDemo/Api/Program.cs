using AgenticDemo.Api.Middleware;
using AgenticDemo.Domain.Interfaces;
using AgenticDemo.Application.Services;
using AgenticDemo.Infrastructure.AI;
using AgenticDemo.Infrastructure.Plugins;
using AgenticDemo.MCP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("Default", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.Configure<McpOptions>(builder.Configuration.GetSection("Mcp"));
builder.Services.AddHttpClient<IMcpClientService, McpClientService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddSingleton<WeatherPlugin>();
builder.Services.AddSingleton<EmailPlugin>();
builder.Services.AddSingleton<ActionHistoryPlugin>();
builder.Services.AddSingleton<SearchPlugin>();
builder.Services.AddSingleton<FileSystemPlugin>();
builder.Services.AddSingleton<SystemInfoPlugin>();
builder.Services.AddSingleton<CalculatorPlugin>();

builder.Services.AddSingleton(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Default");
    
    return KernelFactory.Build(
        builder.Configuration,
        serviceProvider.GetRequiredService<WeatherPlugin>(),
        serviceProvider.GetRequiredService<EmailPlugin>(),
        serviceProvider.GetRequiredService<ActionHistoryPlugin>(),
        serviceProvider.GetRequiredService<SearchPlugin>(),
        serviceProvider.GetRequiredService<FileSystemPlugin>(),
        serviceProvider.GetRequiredService<SystemInfoPlugin>(),
        serviceProvider.GetRequiredService<CalculatorPlugin>(),
        httpClient);
});

builder.Services.AddSingleton<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

var app = builder.Build();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.MapControllers();

app.Run();
