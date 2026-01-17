using System.Text.Json;
using Azure.Messaging.ServiceBus;
using DevQualX.Application.Reports;
using DevQualX.Domain.Models;

namespace DevQualX.Worker;

public class ReportProcessorService(
    ServiceBusClient serviceBusClient,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ReportProcessorService> logger) : BackgroundService
{
    private const string QueueName = "reports";
    private const int MaxRetryAttempts = 5;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = serviceBusClient.CreateProcessor(QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        
        logger.LogInformation("Report processor service started");
        
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Report processor service stopping");
        }
        
        await processor.StopProcessingAsync(CancellationToken.None);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var reportMetadata = args.Message.Body.ToObjectFromJson<ReportMetadata>();
            if (reportMetadata == null)
            {
                logger.LogError("Failed to deserialize report metadata from message");
                await args.DeadLetterMessageAsync(args.Message, "Invalid message format", 
                    "Could not deserialize report metadata", args.CancellationToken);
                return;
            }
            
            var attemptCount = args.Message.ApplicationProperties.TryGetValue("AttemptCount", out var count) 
                ? (int)count : 1;

            logger.LogInformation("Processing report: {Organisation}/{Project}/{FileName} (Attempt {Attempt})", 
                reportMetadata.Organisation, reportMetadata.Project, reportMetadata.FileName, attemptCount);

            // Create a scope to resolve scoped services
            using var scope = serviceScopeFactory.CreateScope();
            var processReport = scope.ServiceProvider.GetRequiredService<IProcessReport>();
            
            var result = await processReport.ExecuteAsync(reportMetadata, args.CancellationToken);

            if (result.Success)
            {
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                logger.LogInformation("Successfully processed report: {BlobUrl}", reportMetadata.BlobUrl);
            }
            else if (result.ShouldRetry && attemptCount < MaxRetryAttempts)
            {
                // Abandon message to requeue with exponential backoff
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attemptCount));
                logger.LogWarning("Report processing requires retry. Attempt {Attempt}/{Max}. Reason: {Reason}", 
                    attemptCount, MaxRetryAttempts, result.FailureReason);
                
                // Update attempt count and abandon to requeue
                var propertiesToModify = new Dictionary<string, object>
                {
                    ["AttemptCount"] = attemptCount + 1
                };
                
                await args.AbandonMessageAsync(args.Message, propertiesToModify, args.CancellationToken);
                
                // Wait for exponential backoff before processing continues
                await Task.Delay(delay, args.CancellationToken);
            }
            else
            {
                // Dead-letter the message after max retries
                logger.LogError("Report processing failed after {Attempts} attempts: {Reason}. Moving to dead-letter queue.", 
                    attemptCount, result.FailureReason);
                await args.DeadLetterMessageAsync(args.Message, 
                    "ProcessingFailed", 
                    result.FailureReason, 
                    args.CancellationToken);
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize message body");
            await args.DeadLetterMessageAsync(args.Message, 
                "DeserializationError", 
                ex.Message, 
                args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing message");
            await args.DeadLetterMessageAsync(args.Message, 
                "UnexpectedError", 
                ex.Message, 
                args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus processor error: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
