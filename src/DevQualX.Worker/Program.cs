using DevQualX.Application;
using DevQualX.Data;
using DevQualX.Infrastructure;
using DevQualX.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add Azure clients with OpenTelemetry
builder.AddAzureBlobServiceClient("blobs");
builder.AddAzureServiceBusClient("messaging");

// Add application, domain, data, and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();
builder.Services.AddDataServices();
builder.Services.AddInfrastructureServices();

// Add background service
builder.Services.AddHostedService<ReportProcessorService>();

var host = builder.Build();
host.Run();
