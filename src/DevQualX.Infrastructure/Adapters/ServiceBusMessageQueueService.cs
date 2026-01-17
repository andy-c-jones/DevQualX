using System.Text.Json;
using Azure.Messaging.ServiceBus;
using DevQualX.Domain.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DevQualX.Infrastructure.Adapters;

public class ServiceBusMessageQueueService(
    ServiceBusClient serviceBusClient,
    ILogger<ServiceBusMessageQueueService> logger) : IMessageQueueService
{
    public async Task SendMessageAsync<T>(
        string queueName,
        T message,
        Dictionary<string, object>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var sender = serviceBusClient.CreateSender(queueName);
        
        var messageBody = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        if (properties != null)
        {
            foreach (var (key, value) in properties)
            {
                serviceBusMessage.ApplicationProperties[key] = value;
            }
        }

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        
        logger.LogInformation("Sent message to queue {QueueName}: {MessageId}", queueName, serviceBusMessage.MessageId);
    }
}
