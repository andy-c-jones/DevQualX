# Agent Guidelines for DevQualX

This document provides essential information for AI coding agents working in the DevQualX codebase.

## Project Overview

DevQualX is a .NET Aspire application built with:
- .NET 10.0 (net10.0)
- ASP.NET Core Web API (ApiService)
- Blazor Server Web UI (Web)
- .NET Aspire AppHost for orchestration
- Implicit usings and nullable reference types enabled

## Project Structure

```
src/
├── DevQualX.ApiService/       # REST API backend service
├── DevQualX.Web/              # Blazor Server frontend
│   └── Components/            # Razor components
│       ├── Pages/             # Routable page components
│       └── Layout/            # Layout components
├── DevQualX.AppHost/          # Aspire orchestration host
├── DevQualX.ServiceDefaults/  # Shared service configuration
├── DevQualX.Application/      # Application layer (IDD services)
│   └── Weather/               # Feature-based folders
├── DevQualX.Domain/           # Domain layer (services, DTOs, interfaces)
│   ├── Models/                # DTOs and data models
│   ├── Services/              # Domain service interfaces and implementations
│   ├── Data/                  # Data layer interfaces (ports)
│   └── Infrastructure/        # Infrastructure layer interfaces (ports)
├── DevQualX.Data/             # Data layer (database implementations)
│   └── Repositories/          # Repository implementations
└── DevQualX.Infrastructure/   # Infrastructure layer (third-party adapters)
    └── Adapters/              # Third-party service adapters
```

## Architecture

### Clean Architecture with Interaction-Driven Design (IDD)

DevQualX follows a clean architecture pattern with Interaction-Driven Design principles:

**Layer Dependencies (dependencies flow inward):**
```
ApiService/Web → Application → Domain ← Data
                                    ← Infrastructure
```

**Key Principles:**

1. **Application Layer (IDD Services)**
   - Contains application services organized by feature/interaction
   - Application services should **never** call other application services
   - Each application service represents a single use case or interaction
   - Application services coordinate domain services to fulfill use cases
   - Each application service should have its own interface (e.g., `IGetWeatherForecast` for `GetWeatherForecast`)
   - Use primary constructors for dependency injection
   - Example: `GetWeatherForecast.ExecuteAsync()`

2. **Domain Layer**
   - Contains domain services, DTOs, and interfaces
   - Domain services contain business logic and can be reused by multiple application services
   - DTOs (Data Transfer Objects) are defined as records
   - Interfaces for Data and Infrastructure layers are defined here (ports)
   - No dependencies on other layers

3. **Data Layer**
   - Implements domain interfaces for database access
   - Contains repository implementations
   - Currently no database configured (uses in-memory implementations)

4. **Infrastructure Layer**
   - Implements domain interfaces for third-party integrations
   - Contains adapters to external services
   - Follows the ports & adapters pattern

**Example Implementation:**

```csharp
// Domain Layer: DTO (DevQualX.Domain/Models/WeatherForecast.cs)
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Domain Layer: Service Interface (DevQualX.Domain/Services/IWeatherService.cs)
public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}

// Domain Layer: Service Implementation (DevQualX.Domain/Services/WeatherService.cs)
public class WeatherService : IWeatherService
{
    public Task<WeatherForecast[]> GetForecastAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        // Business logic here
    }
}

// Application Layer: IDD Service Interface (DevQualX.Application/Weather/IGetWeatherForecast.cs)
public interface IGetWeatherForecast
{
    Task<WeatherForecast[]> ExecuteAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}

// Application Layer: IDD Service Implementation (DevQualX.Application/Weather/GetWeatherForecast.cs)
public class GetWeatherForecast(IWeatherService weatherService) : IGetWeatherForecast
{
    public async Task<WeatherForecast[]> ExecuteAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        return await weatherService.GetForecastAsync(maxItems, cancellationToken);
    }
}

// API/Web: Usage
app.MapGet("/weatherforecast", async (IGetWeatherForecast getWeatherForecast) =>
{
    return await getWeatherForecast.ExecuteAsync(maxItems: 5);
});
```

**Service Registration:**

```csharp
// In Program.cs for both ApiService and Web
builder.Services.AddApplicationServices();  // Registers application services (scoped) with their interfaces
builder.Services.AddDomainServices();       // Registers domain services

// In ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddScoped<IGetWeatherForecast, GetWeatherForecast>();
    return services;
}
```

### Architecture Enforcement with NsDepCop

DevQualX uses **NsDepCop 2.7.0** to enforce clean architecture rules at build time. NsDepCop is configured with warnings treated as errors, ensuring architectural violations prevent compilation.

**Configuration Files:**
- `/config.nsdepcop` - Master configuration at solution root with `InheritanceDepth="2"`
- `src/<ProjectName>/config.nsdepcop` - Project-specific configs that inherit from master

**Key Rules Enforced:**

1. **Layer Dependencies** (Assembly-level):
   - ✅ Application → Domain (allowed)
   - ✅ Data → Domain (allowed)
   - ✅ Infrastructure → Domain (allowed)
   - ✅ ApiService/Web → Application + Domain (allowed transitively)
   - ❌ Application → Data (disallowed)
   - ❌ Application → Infrastructure (disallowed)
   - ❌ Data ↔ Infrastructure (disallowed)
   - ❌ Domain → any other layer (disallowed)

2. **Namespace Dependencies**:
   - Child namespaces can depend on parents implicitly (`ChildCanDependOnParentImplicitly="true"`)
   - Sibling namespaces require explicit `<Allowed>` rules
   - Global namespace (top-level statements) can reference DevQualX.*, Aspire.*, and Projects namespaces

3. **Framework and Third-Party Assemblies**:
   - All projects can reference: System.*, Microsoft.*, Aspire.*, OpenTelemetry.*, Polly.*, and common packages
   - Assembly-level checking enabled (`CheckAssemblyDependencies="true"`)

**Common NsDepCop Violations:**

```csharp
// ❌ VIOLATION: Application layer referencing Data layer
// DevQualX.Application/Weather/GetWeatherForecast.cs
using DevQualX.Data.Repositories;  // Error: NSDEPCOP07

// ✅ CORRECT: Application layer using Domain interfaces
// DevQualX.Application/Weather/GetWeatherForecast.cs
using DevQualX.Domain.Services;    // OK - Application can reference Domain

// ❌ VIOLATION: Domain layer depending on Infrastructure
// DevQualX.Domain/Services/WeatherService.cs
using DevQualX.Infrastructure.Adapters;  // Error: NSDEPCOP01

// ✅ CORRECT: Domain defines interfaces, Infrastructure implements them
// DevQualX.Domain/Services/IExternalWeatherService.cs (interface)
// DevQualX.Infrastructure/Adapters/ExternalWeatherService.cs (implementation)
```

**Build Behavior:**
- All NsDepCop warnings are treated as errors (`TreatWarningsAsErrors="true"`)
- Build fails immediately on any architectural violation
- Violations include both namespace (NSDEPCOP01) and assembly (NSDEPCOP07) reference errors

**When Adding New Dependencies:**
1. Ensure the dependency follows clean architecture principles
2. Update `config.nsdepcop` if adding new allowed patterns
3. Never disable NsDepCop or use `MaxIssueCount` workarounds
4. Build must succeed with 0 errors before committing

## Blazor Server

The Blazor frontend uses **static server-side rendering (SSR) by default** to maximize performance and simplify development:

- Components without `@rendermode` directive default to static SSR
- Use `@attribute [StreamRendering]` for streaming SSR with progressive enhancement
- Interactive mode is available via explicit opt-in with `@rendermode InteractiveServer`
- Prefer form posts and plain JavaScript for progressive enhancement over interactive components
- Only use interactive components when truly necessary (real-time updates, complex client-side logic)

**Static SSR Example:**
```razor
@page "/weather"
@attribute [StreamRendering]
@using DevQualX.Application.Weather
@inject GetWeatherForecast GetWeatherForecastService

<h1>Weather</h1>
@if (forecasts == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table>...</table>
}

@code {
    private WeatherForecast[]? forecasts;
    
    protected override async Task OnInitializedAsync()
    {
        forecasts = await GetWeatherForecastService.ExecuteAsync();
    }
}
```

## Build, Test, and Run Commands

### Building
```bash
# Build entire solution
dotnet build DevQualX.slnx

# Build specific project
dotnet build src/DevQualX.ApiService/DevQualX.ApiService.csproj
dotnet build src/DevQualX.Web/DevQualX.Web.csproj

# Clean and rebuild
dotnet clean && dotnet build
```

### Running
```bash
# Run the Aspire AppHost (starts all services)
dotnet run --project src/DevQualX.AppHost/DevQualX.AppHost.csproj

# Run individual services (for development)
dotnet run --project src/DevQualX.ApiService/DevQualX.ApiService.csproj
dotnet run --project src/DevQualX.Web/DevQualX.Web.csproj
```

### Testing
```bash
# Run all tests (when test projects are added)
dotnet test

# Run tests in specific project
dotnet test src/ProjectName.Tests/ProjectName.Tests.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName=Namespace.ClassName.TestMethodName"

# Run tests matching a name pattern
dotnet test --filter "FullyQualifiedName~WeatherForecast"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Code Quality
```bash
# Format code
dotnet format

# Format with verification (dry run)
dotnet format --verify-no-changes

# Restore dependencies
dotnet restore
```

## Code Style Guidelines

### General Principles
- Use C# 13 features and modern syntax
- Enable nullable reference types in all projects
- Use implicit usings (configured at project level)
- Follow async/await patterns throughout

### File Organization
- One public type per file
- File name matches the primary type name
- Use namespaces matching folder structure
- Place `using` directives at the top (implicit usings reduce need for explicit ones)

### Naming Conventions
- **PascalCase**: Classes, methods, properties, public fields, namespaces
- **camelCase**: Local variables, parameters, private fields
- **_camelCase**: Private fields (with underscore prefix) - not used in this codebase
- **Interface names**: Prefix with `I` (e.g., `IWeatherService`)
- **Async methods**: Suffix with `Async` (e.g., `GetWeatherAsync`)

### Type Usage
- Use `var` for local variables when type is obvious
- Prefer explicit types when it improves clarity
- Use nullable reference types (`string?`) for nullable values
- Use records for DTOs and immutable data models
- Use primary constructors for simple classes (C# 12+)

### Records and Data Models
```csharp
// Use records for DTOs
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Use primary constructors for services
public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<Data> GetAsync() { /* ... */ }
}
```

### Async/Await Patterns
- Always use `async`/`await` for I/O operations
- Include `CancellationToken` parameter with default value
- Return `Task<T>` or `Task` for async methods
- Use `ConfigureAwait(false)` in library code (not necessary in ASP.NET Core)

```csharp
public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### Error Handling
- Use problem details for API errors (configured via `AddProblemDetails()`)
- Use exception handling middleware (`UseExceptionHandler()`)
- Throw specific exceptions, not generic `Exception`
- Use nullable types instead of null checks where possible
- Use `??=` for lazy initialization: `forecasts ??= [];`

### Dependency Injection
- Use primary constructors for DI in services
- Register services in `Program.cs` using `builder.Services`
- Use `HttpClient` with typed clients
- Use service discovery for inter-service communication

```csharp
// Typed HTTP client registration
builder.Services.AddHttpClient<WeatherApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
});
```

### Blazor Components
- Use `.razor` files for components
- Use `@page` directive for routable components
- Use `@code` blocks for component logic
- Use `@inject` for dependency injection
- Use `@rendermode` for interactivity settings
- Use `@attribute` for component attributes

```razor
@page "/weather"
@attribute [StreamRendering(true)]
@attribute [OutputCache(Duration = 5)]
@inject WeatherApiClient WeatherApi
```

### API Endpoints
- Use minimal APIs with `MapGet`, `MapPost`, etc.
- Use top-level statements in `Program.cs`
- Include `.WithName()` for endpoint naming
- Use `MapDefaultEndpoints()` for health checks

### Configuration and Service Defaults
- Use `AddServiceDefaults()` extension method for common services
- Configure OpenTelemetry, health checks, and service discovery
- Use `appsettings.json` and `appsettings.Development.json` for configuration
- Use user secrets for sensitive data in development

### Comments
- Use XML documentation comments for public APIs
- Avoid obvious comments; code should be self-documenting
- Use comments to explain "why", not "what"
- Include links to documentation where helpful

## Testing

DevQualX uses **TUnit 0.6.0** as the primary testing framework with **FakeItEasy 8.3.0** for mocking. The test infrastructure follows clean architecture principles with dedicated test projects for each layer.

### Test Project Structure

```
tests/
├── DevQualX.Domain.Tests/         # Unit tests for domain layer
├── DevQualX.Application.Tests/    # Unit tests for application services (IDD)
├── DevQualX.Data.Tests/           # Integration tests with SQL Server
├── DevQualX.Infrastructure.Tests/ # Unit tests for third-party adapters
├── DevQualX.ApiService.Tests/     # Unit + minimal service tests for API
└── DevQualX.Web.Tests/            # bUnit tests for Blazor components
```

### Testing Framework: TUnit

TUnit is a modern .NET testing framework with excellent performance and native async support.

**Test Structure:**
- **Test files**: Named `[TypeUnderTest]Should.cs` (e.g., `WeatherServiceShould.cs`)
- **Test methods**: Start with verbs describing behavior (e.g., `Return_requested_number_of_forecasts`)
- **NO** "Test" suffix in method names
- Use `[Test]` attribute on test methods
- Use `await Assert.That(value).Condition()` for assertions

**Example Unit Test:**
```csharp
public class WeatherServiceShould
{
    [Test]
    public async Task Return_requested_number_of_forecasts()
    {
        // Arrange
        var service = new WeatherService();
        const int expectedCount = 5;

        // Act
        var result = await service.GetForecastAsync(expectedCount);

        // Assert
        await Assert.That(result).HasCount().EqualTo(expectedCount);
    }
}
```

### Mocking: FakeItEasy

Use **FakeItEasy** for mocking dependencies in unit tests. FakeItEasy provides a fluent, readable API.

**Common Patterns:**
```csharp
// Create a fake
var fakeService = A.Fake<IWeatherService>();

// Configure return value
A.CallTo(() => fakeService.GetForecastAsync(A<int>._, A<CancellationToken>._))
    .Returns(expectedData);

// Verify method was called
A.CallTo(() => fakeService.GetForecastAsync(5, A<CancellationToken>._))
    .MustHaveHappenedOnceExactly();
```

**Important**: Only interfaces and virtual/abstract members can be faked. Mock at boundaries (e.g., mock `IWeatherService`, not concrete `WeatherService`).

### Test Types

**1. Unit Tests** (Domain, Application, Infrastructure)
- Test single components in isolation
- Mock all dependencies using FakeItEasy
- Fast execution, run frequently
- Located in: `DevQualX.Domain.Tests`, `DevQualX.Application.Tests`, `DevQualX.Infrastructure.Tests`

**Example:**
```csharp
public class GetWeatherForecastShould
{
    [Test]
    public async Task Call_weather_service_with_correct_parameters()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        var expectedData = new[] { /* test data */ };
        A.CallTo(() => fakeWeatherService.GetForecastAsync(A<int>._, A<CancellationToken>._))
            .Returns(expectedData);
        var service = new GetWeatherForecast(fakeWeatherService);
        
        // Act
        var result = await service.ExecuteAsync(maxItems: 5);
        
        // Assert
        await Assert.That(result).IsEqualTo(expectedData);
        A.CallTo(() => fakeWeatherService.GetForecastAsync(5, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
```

**2. Service Tests** (ApiService, Web - MINIMAL USE ONLY)
- Test HTTP pipeline with real dependency injection using `WebApplicationFactory<Program>`
- Validate DI configuration and endpoint routing
- **Much slower than unit tests** - use sparingly (only for happy path DI validation)
- Called "service tests" not "integration tests" (no external processes like databases)
- Located in: `DevQualX.ApiService.Tests`, `DevQualX.Web.Tests`

**Example:**
```csharp
public class WeatherEndpointServiceShould : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeatherEndpointServiceShould()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task Return_success_status_code()
    {
        var response = await _client.GetAsync("/weatherforecast");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
```

**Important**: For `WebApplicationFactory` to work, the API/Web project's `Program.cs` must expose the `Program` class:
```csharp
// At the end of Program.cs
public partial class Program { }
```

**3. Integration Tests** (Data Layer)
- Test against **real SQL Server database**
- **NO mocking** - use real database connections
- Use Dapper for data access in tests
- Use transactions for test isolation
- Located in: `DevQualX.Data.Tests`

**Example:**
```csharp
public class WeatherRepositoryShould
{
    [Test]
    public async Task Insert_and_retrieve_weather_forecast()
    {
        // Arrange
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        var repository = new WeatherRepository(connection);
        
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 20, "Mild");
        
        // Act
        await repository.InsertAsync(forecast);
        var result = await repository.GetByDateAsync(forecast.Date);
        
        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.TemperatureC).IsEqualTo(20);
        
        // Cleanup
        transaction.Rollback();
    }
}
```

### Blazor Component Testing: bUnit

Use **bUnit 1.35.3** for testing Blazor components.

**Key Points:**
- Extend `Bunit.TestContext` (NOT `TUnit.Core.TestContext`)
- Use `RenderComponent<T>()` to render components
- Mock injected services with FakeItEasy
- Use `Services.AddSingleton()` to register test dependencies

**Example:**
```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;

public class WeatherPageShould : Bunit.TestContext
{
    [Test]
    public void Display_loading_message_initially()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        Services.AddSingleton(fakeWeatherService);
        Services.AddSingleton<GetWeatherForecast>();

        // Act
        var cut = RenderComponent<Weather>();

        // Assert
        cut.MarkupMatches("<h1>Weather</h1><p><em>Loading...</em></p>");
    }

    [Test]
    public async Task Display_weather_forecasts_after_loading()
    {
        // Arrange
        var fakeWeatherService = A.Fake<IWeatherService>();
        var testData = new[] { /* test forecasts */ };
        A.CallTo(() => fakeWeatherService.GetForecastAsync(A<int>._, A<CancellationToken>._))
            .Returns(testData);
        
        Services.AddSingleton(fakeWeatherService);
        Services.AddSingleton<GetWeatherForecast>();

        // Act
        var cut = RenderComponent<Weather>();
        await Task.Delay(600); // Wait for component async operations
        
        // Assert
        var tableRows = cut.FindAll("tbody tr");
        await Assert.That(tableRows.Count).IsEqualTo(2);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests in specific project
dotnet test tests/DevQualX.Domain.Tests/DevQualX.Domain.Tests.csproj

# Run tests matching a name pattern
dotnet test --filter "FullyQualifiedName~WeatherForecast"

# Run a specific test
dotnet test --filter "FullyQualifiedName=DevQualX.Domain.Tests.Services.WeatherServiceShould.Return_requested_number_of_forecasts"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Build and test
dotnet build DevQualX.slnx && dotnet test DevQualX.slnx
```

### Test Project Dependencies (NsDepCop Rules)

Test projects have strict architectural boundaries enforced by NsDepCop:

- Test projects can **ONLY** reference:
  - The project they test (and its transitive dependencies)
  - Test framework assemblies (TUnit, FakeItEasy, bUnit, etc.)
- Test projects **CANNOT** reference other test projects
- All test projects have `TreatWarningsAsErrors="true"` - fix warnings immediately

**Example `config.nsdepcop` for Domain.Tests:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<NsDepCopConfig InheritanceDepth="2">
    <!-- Domain.Tests can only reference Domain -->
    <!-- Test projects should not reference other test projects -->
    
    <!-- Allow test framework assemblies -->
    <AllowedAssembly From="DevQualX.Domain.Tests" To="TUnit" />
    <AllowedAssembly From="DevQualX.Domain.Tests" To="TUnit.*" />
    <AllowedAssembly From="DevQualX.Domain.Tests" To="FakeItEasy" />
    <AllowedAssembly From="DevQualX.Domain.Tests" To="DevQualX.Domain" />
    
    <!-- Allow test framework namespaces -->
    <Allowed From="DevQualX.Domain.Tests.*" To="TUnit.*" />
    <Allowed From="DevQualX.Domain.Tests.*" To="FakeItEasy.*" />
    <Allowed From="DevQualX.Domain.Tests.*" To="DevQualX.Domain.*" />
</NsDepCopConfig>
```

### Testing Guidelines

1. **Prefer unit tests over service tests**: Unit tests are faster and more focused
2. **Use service tests sparingly**: Only for validating DI configuration (happy path only)
3. **Data tests are integration tests**: Test against real SQL Server, no mocking
4. **Test naming conventions**:
   - Files: `[TypeUnderTest]Should.cs`
   - Methods: Start with verbs, use underscores (e.g., `Return_requested_number_of_forecasts`)
5. **Always use async/await**: TUnit has native async support, use `await Assert.That()`
6. **Mock at boundaries**: Mock interfaces (e.g., `IWeatherService`), not concrete classes
7. **One assertion focus per test**: Keep tests focused on single behaviors
8. **Arrange-Act-Assert**: Follow AAA pattern consistently
9. **Test isolation**: Each test should be independent and not rely on other tests
10. **Avoid testing framework code**: Don't test ASP.NET Core, Entity Framework, or other framework behavior

### Test Project Configuration

All test projects should include these settings in their `.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
  <GenerateProgramFile>false</GenerateProgramFile>
</PropertyGroup>
```

**Key packages** (versions as of January 2026):
- TUnit: 0.6.0
- TUnit.Assertions: 0.6.0
- FakeItEasy: 8.3.0
- FakeItEasy.Analyzer.CSharp: 6.1.1
- bUnit: 1.35.3 (Web.Tests only)
- Microsoft.AspNetCore.Mvc.Testing: 10.0.1 (ApiService.Tests only)
- Dapper: 2.1.35 (Data.Tests only)
- Microsoft.Data.SqlClient: 5.2.2 (Data.Tests only)

## Aspire-Specific Guidelines

### Service Configuration
- Always call `AddServiceDefaults()` in service builders
- Use `MapDefaultEndpoints()` to register health checks
- Configure HTTP clients with service discovery

### AppHost Configuration
- Define services with `AddProject<T>()`
- Use health checks: `.WithHttpHealthCheck("/health")`
- Use `.WithReference()` for service dependencies
- Use `.WaitFor()` for startup ordering
- Use `.WithExternalHttpEndpoints()` for public-facing services

## Common Patterns

### Minimal API Endpoints
```csharp
app.MapGet("/weatherforecast", () =>
{
    // Implementation
})
.WithName("GetWeatherForecast");
```

### Async Enumerable Processing
```csharp
await foreach (var item in httpClient.GetFromJsonAsAsyncEnumerable<T>("/endpoint", cancellationToken))
{
    // Process item
}
```

### Collection Expressions (C# 12+)
```csharp
string[] summaries = ["Freezing", "Bracing", "Chilly"];
forecasts ??= [];
return forecasts?.ToArray() ?? [];
```

## Important Notes for Agents

1. **Always run from solution root**: Commands should be executed from `/home/aj/Projects/DevQualX`
2. **Use Aspire patterns**: Follow .NET Aspire conventions for service communication and configuration
3. **Implicit usings**: Common namespaces are imported automatically; check project files for details
4. **Primary constructors**: Prefer primary constructors for simple dependency injection
5. **Modern C#**: Use latest C# features (records, pattern matching, collection expressions, etc.)
6. **Health checks**: Always available at `/health` and `/alive` endpoints in development

## References

- Solution file: `DevQualX.slnx` (XML-based solution format)
- .NET version: 10.0.100
- Aspire documentation: https://aka.ms/dotnet/aspire
