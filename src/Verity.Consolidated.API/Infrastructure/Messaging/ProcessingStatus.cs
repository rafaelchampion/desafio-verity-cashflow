namespace Verity.Consolidated.API.Infrastructure.Messaging;

public class ProcessingStatus
{
    public DateTime LastProcessedTime { get; set; } = DateTime.UtcNow;
}
