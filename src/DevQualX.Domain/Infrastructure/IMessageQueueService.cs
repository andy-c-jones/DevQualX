namespace DevQualX.Domain.Infrastructure;

/// <summary>
/// Service for message queue operations.
/// </summary>
public interface IMessageQueueService
{
    /// <summary>
    /// Sends a message to a queue.
    /// </summary>
    Task SendMessageAsync<T>(
        string queueName,
        T message,
        Dictionary<string, object>? properties = null,
        CancellationToken cancellationToken = default);
}
