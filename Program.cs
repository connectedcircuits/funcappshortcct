using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure comprehensive logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();

    // Console logging with JSON formatting for structured logs
    loggingBuilder.AddJsonConsole(options => {
        options.JsonWriterOptions = new JsonWriterOptions
        {
            Indented = true
        };
        options.IncludeScopes = true;
    });

    // Add debug output for local debugging
    loggingBuilder.AddDebug();

    // Configure minimum log levels
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});


builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
