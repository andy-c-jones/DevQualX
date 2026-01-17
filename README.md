# DevQualX

A .NET Aspire application for code quality metrics and analysis with GitHub integration.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/)
- Trusted HTTPS development certificate (see setup below)

## Setup

### Trust HTTPS Development Certificate

**Required**: Trust the ASP.NET Core HTTPS development certificate to avoid health check failures.

Check and trust the certificate:
```bash
dotnet dev-certs https --check --trust
```

**Linux/Podman users**: Manual installation may be required:
```bash
dotnet dev-certs https --export-path ~/aspnetcore-dev-cert.crt --format PEM --no-password
sudo cp ~/aspnetcore-dev-cert.crt /usr/local/share/ca-certificates/aspnetcore-dev-cert.crt
sudo update-ca-certificates
rm ~/aspnetcore-dev-cert.crt
```

## Build

```bash
dotnet build DevQualX.slnx
```

## Test

```bash
dotnet test
```

## Run

Start the application with Aspire AppHost:
```bash
dotnet run --project src/DevQualX.AppHost/DevQualX.AppHost.csproj
```

The Aspire Dashboard will open at `http://localhost:15014` showing all services, logs, and metrics.

## Troubleshooting

**Services showing as "Unhealthy"**: Ensure HTTPS certificate is trusted (see Setup above).

**Container errors**: Ensure Docker Desktop or Podman is running.

## More Information

See [AGENTS.md](AGENTS.md) for architecture guidelines and development practices.
