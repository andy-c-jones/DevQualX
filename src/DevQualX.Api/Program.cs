using DevQualX.Api.Extensions;
using DevQualX.Application;
using DevQualX.Application.Authorization;
using DevQualX.Application.Reports;
using DevQualX.Application.Weather;
using DevQualX.Data;
using DevQualX.Domain.Models;
using DevQualX.Functional;
using DevQualX.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Azure clients with OpenTelemetry
builder.AddAzureBlobServiceClient("blobs");
builder.AddAzureServiceBusClient("messaging");

// Add services to the container.
builder.Services.AddProblemDetails();

// Add application, domain, data, and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();
builder.Services.AddDataServices();
builder.Services.AddInfrastructureServices();

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", async (IGetWeatherForecast getWeatherForecast) =>
{
    var result = await getWeatherForecast.ExecuteAsync(maxItems: 5);
    return result.ToHttpResult();  // Converts Result to 200 OK or ProblemDetails
})
.WithName("GetWeatherForecast");

app.MapPost("/reports", [Authorize] async (
    IFormFile file,
    [FromForm] string organisation,
    [FromForm] string project,
    [FromForm] int installationId,
    IUploadReport uploadReport,
    ICheckUserRole checkUserRole,
    HttpContext httpContext,
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
    
    if (installationId <= 0)
    {
        return Results.BadRequest("Valid installation ID is required");
    }

    // Get user ID from claims
    var userIdClaim = httpContext.User.FindFirst("github_user_id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    // Check if user has permission to upload to this installation
    var authResult = await checkUserRole.ExecuteAsync(
        (int)userId,
        installationId,
        Role.Reader,
        RoleScope.Organization,
        resourceId: null,
        cancellationToken);

    if (authResult is Failure<bool, Error> authFailure)
    {
        return Results.Problem(
            title: "Authorization failed",
            detail: authFailure.Error.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var hasPermission = ((Success<bool, Error>)authResult).Value;
    if (!hasPermission)
    {
        return Results.Problem(
            title: "Insufficient permissions",
            detail: $"User does not have permission to upload reports for installation {installationId}",
            statusCode: StatusCodes.Status403Forbidden);
    }

    await using var stream = file.OpenReadStream();
    
    var request = new ReportUploadRequest(
        userId,
        installationId,
        organisation,
        project,
        file.FileName,
        file.ContentType,
        file.Length,
        stream);

    var result = await uploadReport.ExecuteAsync(request, cancellationToken);
    
    return result.ToHttpResult();
})
.WithName("UploadReport")
.DisableAntiforgery() // For form file upload
.RequireAuthorization();

app.MapDefaultEndpoints();

app.Run();

// Make Program class accessible to tests
public partial class Program { }

