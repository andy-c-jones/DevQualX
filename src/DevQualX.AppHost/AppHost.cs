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

// Add Azurite (Azure Storage emulator) with persistent volume
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(configureContainer: container =>
    {
        container.WithDataVolume("devqualx-azurite");
    });

var blobs = storage.AddBlobs("blobs");

// Add Azure Service Bus emulator
var messaging = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator();

// Configure API service
var apiService = builder.AddProject<Projects.DevQualX_Api>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WithReference(blobs)
    .WithReference(messaging)
    .WaitFor(database)
    .WaitFor(storage)
    .WaitFor(messaging);

// Configure Web frontend
builder.AddProject<Projects.DevQualX_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database);

// Configure Worker service
builder.AddProject<Projects.DevQualX_Worker>("worker")
    .WithReference(blobs)
    .WithReference(messaging)
    .WaitFor(storage)
    .WaitFor(messaging);

builder.Build().Run();
