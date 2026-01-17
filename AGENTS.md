# Agent Guidelines for DevQualX

This document provides essential information for AI coding agents working in the DevQualX codebase.

## Project Overview

DevQualX is a .NET Aspire application built with:
- .NET 10.0 (net10.0)
- ASP.NET Core Web API (Api)
- Blazor Server Web UI (Web)
- .NET Aspire AppHost for orchestration
- Implicit usings and nullable reference types enabled

## Project Structure

```
src/
├── DevQualX.Api/              # REST API backend service
├── DevQualX.Web/              # Blazor Server frontend
│   └── Components/            # Razor components
│       ├── Pages/             # Routable page components
│       └── Layout/            # Layout components
├── DevQualX.AppHost/          # Aspire orchestration host
├── DevQualX.ServiceDefaults/  # Shared service configuration
├── DevQualX.Functional/       # Functional programming primitives (foundation layer)
│   └── Extensions/            # Option/Result extension methods
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
Api/Web → Application → Domain ← Data
              ↓            ↓       ← Infrastructure
         Functional   Functional
```

**Foundation Layer:**
- **DevQualX.Functional** - Core functional programming primitives
  - Zero dependencies on other DevQualX projects
  - Available to ALL layers (foundation layer)
  - Provides: `Option<T>`, `Result<T, TError>`, `Either<TLeft, TRight>`, `Error` hierarchy

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
// In Program.cs for both Api and Web
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
   - ✅ Api/Web → Application + Domain (allowed transitively)
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

## Functional Error Handling with DevQualX.Functional

DevQualX uses the **DevQualX.Functional** library for type-safe error handling and optional values. This library is a **foundation layer** available to all projects.

### Core Principles

**CRITICAL: Never use `null` for absence - use `Option<T>` instead.**
**CRITICAL: Never use exceptions for known error conditions - use `Result<T, TError>` instead.**

### When to Use Each Type

**Use `Option<T>` when:**
- A value may or may not be present (replaces `null`)
- Searching for an item that might not exist
- Optional parameters or properties
- Parsing that might fail without a specific error reason

**Use `Result<T, TError>` when:**
- An operation can fail with known error conditions
- You need to communicate WHY something failed
- Validation errors, not found errors, authorization errors, etc.
- Replacing exceptions for expected failure cases

**Use exceptions ONLY for:**
- Truly exceptional, unrecoverable situations (out of memory, stack overflow)
- Programming errors (null reference, index out of range)
- Framework/infrastructure failures you can't handle

### Option<T> - Type-Safe Optional Values

#### DON'T ❌
```csharp
// BAD: Using null
public User? FindUserById(int id)
{
    return users.FirstOrDefault(u => u.Id == id);  // Returns null if not found
}

var user = FindUserById(42);
if (user == null)  // Easy to forget null check, causes NullReferenceException
{
    // Handle not found
}
```

#### DO ✅
```csharp
// GOOD: Using Option<T>
public Option<User> FindUserById(int id)
{
    return users.FirstOrNone(u => u.Id == id);  // Returns None<User> if not found
}

var user = FindUserById(42);
user.Match(
    some: u => Console.WriteLine($"Found: {u.Name}"),
    none: () => Console.WriteLine("User not found")
);

// Or use GetValueOrDefault
var userName = FindUserById(42)
    .Map(u => u.Name)
    .GetValueOrDefault("Unknown");
```

#### Option LINQ Integration
```csharp
// SelectMany automatically filters out None values
var userNames = userIds
    .Select(id => FindUserById(id))  // Returns IEnumerable<Option<User>>
    .Choose()                         // Filters None, unwraps to IEnumerable<User>
    .Select(u => u.Name);

// Or use ChooseMap for one-step map + filter
var userEmails = userIds
    .ChooseMap(id => FindUserById(id).Map(u => u.Email));
```

### Result<T, TError> - Functional Error Handling

#### DON'T ❌
```csharp
// BAD: Using exceptions for known errors
public User GetUserById(int id)
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        throw new NotFoundException($"User {id} not found");  // Exception for expected case
    }
    return user;
}

try
{
    var user = GetUserById(42);
    // Process user
}
catch (NotFoundException ex)
{
    // Handle not found - using exceptions for control flow
}
```

#### DO ✅
```csharp
// GOOD: Using Result<T, TError>
public Result<User, Error> GetUserById(int id)
{
    return users
        .FirstOrNone(u => u.Id == id)
        .Match(
            some: user => new Success<User, Error>(user),
            none: () => new Failure<User, Error>(
                new NotFoundError 
                { 
                    Message = $"User {id} not found",
                    ResourceType = "User",
                    ResourceId = id.ToString()
                }
            )
        );
}

var result = GetUserById(42);
result.Match(
    success: user => Console.WriteLine($"Found: {user.Name}"),
    failure: error => Console.WriteLine($"Error: {error.Message}")
);
```

#### Result LINQ Integration for Railway-Oriented Programming
```csharp
// Chain operations that can fail - stops at first failure
var result = ValidateUserInput(request)
    .Bind(validInput => CreateUser(validInput))
    .Bind(user => SendWelcomeEmail(user))
    .Map(user => new UserDto(user.Id, user.Name, user.Email));

// If any step fails, the error propagates through the chain
// If all succeed, you get Success<UserDto, Error>
```

### Error Hierarchy

DevQualX provides a discriminated union of error types:

```csharp
// Base error class
public abstract record Error
{
    public required string Message { get; init; }
    public string? Code { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

// Specific error types (automatically map to HTTP status codes in API)
ValidationError        // 400 Bad Request - validation failures
BadRequestError        // 400 Bad Request - malformed requests
UnauthorizedError      // 401 Unauthorized - authentication required
ForbiddenError         // 403 Forbidden - insufficient permissions
NotFoundError          // 404 Not Found - resource doesn't exist
ConflictError          // 409 Conflict - duplicate resources
ExternalServiceError   // 502 Bad Gateway - third-party service failures
InternalError          // 500 Internal Server Error - unexpected errors
```

**Example:**
```csharp
// Domain service returns Result
public Result<User, Error> CreateUser(CreateUserRequest request)
{
    // Validation
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return new ValidationError 
        { 
            Message = "Email is required",
            Code = "VAL001",
            Errors = new Dictionary<string, string[]> 
            {
                ["Email"] = ["Email field is required"]
            }
        };
    }
    
    // Check for conflicts
    if (users.Any(u => u.Email == request.Email))
    {
        return new ConflictError 
        { 
            Message = $"User with email {request.Email} already exists",
            ConflictingResource = request.Email
        };
    }
    
    var user = new User(request.Email, request.Name);
    users.Add(user);
    return user;  // Implicit conversion to Success<User, Error>
}
```

### API Integration

Use the extension methods in `DevQualX.Api.Extensions` to convert Result to HTTP responses:

```csharp
using DevQualX.Api.Extensions;

// Automatic conversion to 200 OK or ProblemDetails
app.MapGet("/users/{id}", async (int id, IGetUser getUser) =>
{
    var result = await getUser.ExecuteAsync(id);
    return result.ToHttpResult();
});

// 201 Created with location header
app.MapPost("/users", async (CreateUserRequest request, ICreateUser createUser) =>
{
    var result = await createUser.ExecuteAsync(request);
    return result.ToCreatedResult(user => $"/users/{user.Id}");
});

// 204 No Content for updates/deletes
app.MapPut("/users/{id}", async (int id, UpdateUserRequest request, IUpdateUser updateUser) =>
{
    var result = await updateUser.ExecuteAsync(id, request);
    return result.ToNoContentResult();
});

// Errors automatically convert to appropriate ProblemDetails responses:
// - ValidationError → 400 with validation details
// - NotFoundError → 404 with resource info
// - UnauthorizedError → 401
// - ConflictError → 409
// - InternalError → 500
```

### Extension Methods

**Option Extensions:**
- `ToOption<T>()` - Convert nullable types to Option
- `FirstOrNone<T>()` - Safe First that returns Option
- `SingleOrNone<T>()` - Safe Single that returns Option
- `LastOrNone<T>()` - Safe Last that returns Option
- `Choose<T>()` - Filter None values from `IEnumerable<Option<T>>`
- `ChooseMap<T, R>()` - Map and filter in one step
- `Flatten<T>()` - Flatten nested Options

**Result Extensions:**
- `ToResult<T>()` - Wrap function execution in Result (catches exceptions)
- `ToResultAsync<T>()` - Async version
- `Combine<T>()` - Combine multiple Results (fails on first failure)
- `CollectErrors<T>()` - Collect all ValidationErrors
- `Choose<T>()` - Filter failures and unwrap successes
- `Partition<T>()` - Split into (successes, failures) tuple

### Best Practices

1. **Always use Option for nullable domain models:**
   ```csharp
   // DON'T
   public string? MiddleName { get; set; }
   
   // DO
   public Option<string> MiddleName { get; set; }
   ```

2. **Chain operations with Bind/Map:**
   ```csharp
   return GetUserById(id)
       .Bind(user => ValidateUser(user))
       .Bind(user => UpdateUser(user))
       .Map(user => new UserDto(user));
   ```

3. **Use LINQ query syntax for readability:**
   ```csharp
   var result = from user in GetUserById(id)
                from validation in ValidateUser(user)
                from updated in UpdateUser(validation)
                select new UserDto(updated);
   ```

4. **Avoid GetValueOrThrow - prefer Match:**
   ```csharp
   // DON'T
   var user = result.GetValueOrThrow();  // Defeats the purpose
   
   // DO
   var userName = result.Match(
       success: u => u.Name,
       failure: _ => "Unknown"
   );
   ```

5. **Use appropriate Error types:**
   - Domain validation → `ValidationError`
   - Resource not found → `NotFoundError`
   - Auth failures → `UnauthorizedError` or `ForbiddenError`
   - External API failures → `ExternalServiceError`
   - Unexpected errors → `InternalError` (last resort)

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

## Component Library

DevQualX uses a **custom component library** built on static-first principles with progressive enhancement. Components are organized using Atomic Design principles.

### Component Library Structure

```
src/DevQualX.Web/
├── Components/
│   └── Library/
│       ├── Atoms/              # Basic building blocks
│       │   ├── Button.razor
│       │   ├── Badge.razor
│       │   ├── Card.razor
│       │   ├── TextInput.razor
│       │   ├── Switch.razor
│       │   ├── Tabs.razor
│       │   └── *.razor.css     # Scoped CSS files
│       └── Molecules/          # Composed components
│           ├── Pagination.razor
│           ├── ThemeSwitcher.razor
│           └── *.razor.css
├── wwwroot/
│   ├── styles/
│   │   ├── core/
│   │   │   ├── variables.css   # CSS custom properties
│   │   │   ├── reset.css       # Minimal CSS reset
│   │   │   ├── typography.css  # Typography system
│   │   │   └── utilities.css   # Utility classes
│   │   ├── layout/
│   │   │   ├── flex.css        # Flexbox utilities
│   │   │   └── grid.css        # Grid utilities
│   │   └── main.css            # Main entry point
│   └── js/
│       ├── validation.js       # Form validation enhancement
│       ├── tabs.js             # Tab navigation enhancement
│       └── theme.js            # Theme switching
```

### Component Library Principles

**CRITICAL: All new components MUST follow these principles:**

#### 1. **Static-First Rendering**
- Components MUST work with static SSR (no `@rendermode` by default)
- Use `@attribute [StreamRendering]` for pages that load data asynchronously to enable progressive rendering
- Only use `@rendermode InteractiveServer` when absolutely necessary (real-time updates, complex client interactions)
- Progressive enhancement: base functionality works without JavaScript, enhanced with JS

**Example:**
```razor
@namespace DevQualX.Web.Components.Library.Atoms

<button 
    type="@Type"
    class="btn btn--@Variant.ToString().ToLowerInvariant() @Class"
    disabled="@Disabled"
    @onclick="OnClick"
    @attributes="AdditionalAttributes">
    @ChildContent
</button>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Default;
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
```

#### 2. **Type-Safe Component APIs**
- Use C# enums for variants, sizes, types (never magic strings)
- Use `[EditorRequired]` and `required` for mandatory parameters
- Use primary constructors for dependency injection
- Always provide `Class` and `AdditionalAttributes` parameters for extensibility

**Example:**
```csharp
public enum ButtonVariant
{
    Default,
    Primary,
    Secondary,
    Danger,
    Ghost,
    Link
}

public enum ButtonSize
{
    Sm,
    Md,
    Lg
}
```

#### 3. **Scoped CSS with BEM Naming**
- Every component has a `.razor.css` file with scoped styles
- Use BEM (Block Element Modifier) naming: `.component__element--modifier`
- Use CSS custom properties from `/wwwroot/styles/core/variables.css`
- Never use inline styles or `<style>` tags in components

**Example (Button.razor.css):**
```css
.btn {
    display: inline-flex;
    align-items: center;
    gap: var(--spacing-2);
    padding: var(--spacing-2) var(--spacing-4);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    border-radius: var(--radius-md);
    transition: all var(--transition-base);
}

.btn--primary {
    background-color: var(--color-primary);
    color: var(--color-white);
}

.btn__spinner {
    width: 1rem;
    height: 1rem;
    animation: spin 1s linear infinite;
}
```

#### 4. **Progressive Enhancement with JavaScript**
- Base functionality works without JavaScript
- JavaScript enhances UX (smooth transitions, keyboard navigation, etc.)
- JavaScript files in `/wwwroot/js/` use vanilla JS (no frameworks)
- Use IIFE pattern with namespace: `window.DevQualX.*`
- Listen for Blazor enhanced navigation events
- Always provide `init()` function for manual initialization

**Example (tabs.js):**
```javascript
(function() {
    'use strict';
    
    function initTabs() {
        const tabContainers = document.querySelectorAll('[data-tabs]');
        // Enhancement logic...
    }
    
    // Initialize immediately
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTabs);
    } else {
        initTabs();
    }
    
    // Reinitialize after Blazor enhanced navigation
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancednavigation', initTabs);
    }
    
    // Expose API
    window.DevQualX = window.DevQualX || {};
    window.DevQualX.tabs = { init: initTabs };
})();
```

#### 5. **Accessibility-First**
- Use semantic HTML elements
- Include proper ARIA attributes (`role`, `aria-label`, `aria-controls`, etc.)
- Support keyboard navigation (Tab, Enter, Space, Arrow keys, Home, End)
- Ensure sufficient color contrast
- Test with screen readers

**Example:**
```razor
<button 
    role="tab"
    aria-selected="@IsActive.ToString().ToLowerInvariant()"
    aria-controls="@PanelId"
    tabindex="@(IsActive ? "0" : "-1")">
    @ChildContent
</button>
```

#### 6. **HTML5 Form Validation**
- Use HTML5 validation attributes (`required`, `pattern`, `minlength`, `maxlength`, `min`, `max`, `type`)
- Progressive enhancement with JavaScript for better UX (validate on blur, custom error messages)
- Server-side validation is always required (never trust client-side validation)
- Use `data-validation-*` attributes for custom error messages

**Example:**
```razor
<input 
    type="@Type.ToString().ToLowerInvariant()"
    required="@Required"
    minlength="@MinLength"
    maxlength="@MaxLength"
    pattern="@Pattern"
    data-validation-required="@ValidationRequiredMessage"
    data-validation-pattern="@ValidationPatternMessage" />
```

### Available Components

**Atoms:**
- `Button` - Multiple variants, sizes, loading state
- `Badge` - Color variants with optional dot indicator
- `Card` + `CardHeader`/`CardBody`/`CardFooter` - Container components
- `TextInput` - Full HTML5 validation, all input types, helper text, error messages
- `Switch` - Toggle switch with label support
- `Tabs` + `TabButton`/`TabPanel` - Tab navigation with progressive enhancement

**Molecules:**
- `Pagination` - Server-side pagination with query parameters
- `ThemeSwitcher` - Light/Dark/System theme switching

**Component Showcase:**
- Visit `/dev/components` to see live examples of all components

### Custom CSS System

DevQualX uses a **custom CSS foundation** (Bootstrap removed). Never add Bootstrap or other CSS frameworks.

**CSS Variables (defined in `/wwwroot/styles/core/variables.css`):**
```css
/* Colors */
--color-primary: #6366f1;      /* Indigo */
--color-white: #ffffff;
--color-gray-50: #f9fafb;
--color-gray-100: #f3f4f6;
/* ... more colors ... */

/* Spacing (0.25rem = 4px increments) */
--spacing-1: 0.25rem;   /* 4px */
--spacing-2: 0.5rem;    /* 8px */
--spacing-3: 0.75rem;   /* 12px */
--spacing-4: 1rem;      /* 16px */
/* ... more spacing ... */

/* Typography */
--font-size-xs: 0.75rem;    /* 12px */
--font-size-sm: 0.875rem;   /* 14px */
--font-size-base: 1rem;     /* 16px */
/* ... more typography ... */

/* Border Radius */
--radius-sm: 0.25rem;   /* 4px */
--radius-md: 0.375rem;  /* 6px */
--radius-lg: 0.5rem;    /* 8px */
/* ... more radius ... */

/* Shadows */
--shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
--shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.1);
/* ... more shadows ... */

/* Transitions */
--transition-base: 150ms cubic-bezier(0.4, 0, 0.2, 1);
--transition-slow: 300ms cubic-bezier(0.4, 0, 0.2, 1);
```

**Utility Classes (available from `/wwwroot/styles/core/utilities.css` and `/wwwroot/styles/layout/*.css`):**
- Spacing: `m-{size}`, `mt-{size}`, `p-{size}`, `px-{size}`, etc. (sizes: 0-12)
- Display: `block`, `inline-block`, `flex`, `inline-flex`, `grid`, `hidden`
- Flexbox: `flex`, `flex-col`, `items-center`, `justify-between`, `gap-{size}`
- Grid: `grid`, `grid-cols-{count}`, `gap-{size}`, responsive variants
- Typography: `text-{size}`, `font-{weight}`, `text-{align}`, `text-{color}`
- Borders: `border`, `border-{side}`, `rounded-{size}`
- Shadows: `shadow-{size}`

**Responsive Breakpoints:**
- `sm`: 640px
- `md`: 768px
- `lg`: 1024px
- `xl`: 1280px
- `2xl`: 1536px

**Example usage:**
```razor
<div class="flex flex-col gap-4 p-6 md:flex-row md:gap-6">
    <div class="flex-1">
        <h2 class="text-2xl font-bold mb-4">Title</h2>
        <p class="text-gray-600">Content</p>
    </div>
</div>
```

### Creating New Components

When creating new components:

1. **Choose the right level:**
   - **Atoms**: Basic, single-purpose components (buttons, inputs, icons)
   - **Molecules**: Composed components with multiple elements (forms, cards, menus)
   - **Organisms**: Complex sections (headers, sidebars, data tables)

2. **Follow the component template:**
   ```razor
   @namespace DevQualX.Web.Components.Library.Atoms
   
   <div class="component-name @Class" @attributes="AdditionalAttributes">
       @ChildContent
   </div>
   
   @code {
       [Parameter] public RenderFragment? ChildContent { get; set; }
       [Parameter] public string? Class { get; set; }
       [Parameter(CaptureUnmatchedValues = true)] 
       public Dictionary<string, object>? AdditionalAttributes { get; set; }
   }
   ```

3. **Create scoped CSS file** (`ComponentName.razor.css`):
   ```css
   .component-name {
       /* Base styles using CSS variables */
   }
   
   .component-name__element {
       /* Element styles */
   }
   
   .component-name--modifier {
       /* Modifier styles */
   }
   ```

4. **Add progressive enhancement if needed** (`/wwwroot/js/component-name.js`)

5. **Update `/dev/components` showcase page** with usage examples

6. **Never use:**
   - Inline styles
   - `<style>` tags in components
   - Magic strings for variants/sizes
   - `@rendermode` unless absolutely necessary
   - JavaScript frameworks (jQuery, React, etc.)
   - CSS frameworks (Bootstrap, Tailwind, etc.)

### StreamRendering for Pages

Use `@attribute [StreamRendering]` on **pages** (not components) that load data asynchronously to enable progressive rendering:

```razor
@page "/products"
@attribute [StreamRendering]
@inject IGetProducts GetProducts

<h1>Products</h1>

@if (products == null)
{
    <p>Loading...</p>
}
else
{
    <div class="grid grid-cols-3 gap-4">
        @foreach (var product in products)
        {
            <Card>
                <CardBody>
                    <h3>@product.Name</h3>
                    <p>@product.Price.ToString("C")</p>
                </CardBody>
            </Card>
        }
    </div>
}

@code {
    private Product[]? products;
    
    protected override async Task OnInitializedAsync()
    {
        // StreamRendering will render the "Loading..." state first,
        // then update with the products once loaded
        products = await GetProducts.ExecuteAsync();
    }
}
```

**When to use StreamRendering:**
- Pages with slow data loading (database queries, API calls)
- Pages where showing a loading state improves perceived performance
- Pages with multiple async data sources

**When NOT to use StreamRendering:**
- On components (only use on pages)
- When data loads instantly (< 100ms)
- When you need data before rendering (use regular SSR)

## Build, Test, and Run Commands

### Building
```bash
# Build entire solution
dotnet build DevQualX.slnx

# Build specific project
dotnet build src/DevQualX.Api/DevQualX.Api.csproj
dotnet build src/DevQualX.Web/DevQualX.Web.csproj

# Clean and rebuild
dotnet clean && dotnet build
```

### Running
```bash
# Run the Aspire AppHost (starts all services)
dotnet run --project src/DevQualX.AppHost/DevQualX.AppHost.csproj

# Run individual services (for development)
dotnet run --project src/DevQualX.Api/DevQualX.Api.csproj
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
├── DevQualX.Api.Tests/     # Unit + minimal service tests for API
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

**2. Service Tests** (Api, Web - MINIMAL USE ONLY)
- Test HTTP pipeline with real dependency injection using `WebApplicationFactory<Program>`
- Validate DI configuration and endpoint routing
- **Much slower than unit tests** - use sparingly (only for happy path DI validation)
- Called "service tests" not "integration tests" (no external processes like databases)
- Located in: `DevQualX.Api.Tests`, `DevQualX.Web.Tests`

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
- Microsoft.AspNetCore.Mvc.Testing: 10.0.1 (Api.Tests only)
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
