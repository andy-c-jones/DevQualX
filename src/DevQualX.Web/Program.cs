using DevQualX.Application;
using DevQualX.Data;
using DevQualX.Infrastructure;
using DevQualX.Web.Components;
using DevQualX.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Azure clients with OpenTelemetry
builder.AddAzureBlobServiceClient("blobs");
builder.AddAzureServiceBusClient("messaging");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/signin";
        options.LogoutPath = "/auth/signout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "DevQualX.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

// Add session for PKCE state management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "DevQualX.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add application and domain services
builder.Services.AddApplicationServices();
builder.Services.AddDomainServices();
builder.Services.AddDataServices(); // Repository registrations
builder.Services.AddInfrastructureServices(); // GitHub service registrations

// Add Web-specific services
builder.Services.AddScoped<OrgContextService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Add session middleware (before authentication)
app.UseSession();

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

// Expose Program class for testing
public partial class Program { }
