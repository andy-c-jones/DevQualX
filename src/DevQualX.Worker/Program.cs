using DevQualX.Application;
using DevQualX.Infrastructure;
using DevQualX.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Azure Service Bus client with OpenTelemetry
builder.AddAzureServiceBusClient("messaging");

// Add application, domain, and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();
builder.Services.AddInfrastructureServices();

// Add background service
builder.Services.AddHostedService<ReportProcessorService>();

var host = builder.Build();
host.Run();
