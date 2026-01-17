using DevQualX.Application;
using DevQualX.Application.Reports;
using DevQualX.Application.Weather;
using DevQualX.Domain.Models;
using DevQualX.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Azure clients with OpenTelemetry
builder.AddAzureBlobServiceClient("blobs");
builder.AddAzureServiceBusClient("messaging");

// Add services to the container.
builder.Services.AddProblemDetails();

// Add application, domain, and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();
builder.Services.AddInfrastructureServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", async (IGetWeatherForecast getWeatherForecast) =>
{
    return await getWeatherForecast.ExecuteAsync(maxItems: 5);
})
.WithName("GetWeatherForecast");

app.MapPost("/reports", async (
    IFormFile file,
    [FromForm] string organisation,
    [FromForm] string project,
    IUploadReport uploadReport,
    CancellationToken cancellationToken) =>
{
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded");
    }

    if (string.IsNullOrWhiteSpace(organisation) || string.IsNullOrWhiteSpace(project))
    {
        return Results.BadRequest("Organisation and project are required");
    }

    await using var stream = file.OpenReadStream();
    
    var request = new ReportUploadRequest(
        organisation,
        project,
        file.FileName,
        file.ContentType,
        file.Length,
        stream);

    var metadata = await uploadReport.ExecuteAsync(request, cancellationToken);
    
    return Results.Ok(metadata);
})
.WithName("UploadReport")
.DisableAntiforgery(); // Anonymous access

app.MapDefaultEndpoints();

app.Run();

// Make Program class accessible to tests
public partial class Program { }

