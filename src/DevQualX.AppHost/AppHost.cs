var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server with persistent volume
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume("devqualx-sqldata")
    .WithLifetime(ContainerLifetime.Persistent);

// Add database
var database = sqlServer.AddDatabase("devqualx");

// Add SQL Database project with DACPAC deployment
builder.AddSqlProject<Projects.DevQualX_Database>("database-project")
    .WithReference(database)
    .WithConfigureDacDeployOptions(options =>
    {
        options.DropObjectsNotInSource = true;
        options.BlockOnPossibleDataLoss = false; // Local dev only - change for production
    });

var apiService = builder.AddProject<Projects.DevQualX_Api>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.DevQualX_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
