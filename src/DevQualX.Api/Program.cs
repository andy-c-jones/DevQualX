using DevQualX.Application;
using DevQualX.Application.Weather;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add application and domain services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();

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

app.MapDefaultEndpoints();

app.Run();

// Make Program class accessible to tests
public partial class Program { }
