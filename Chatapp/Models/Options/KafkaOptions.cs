namespace WebService.Models.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public string MessageNotificationTopic { get; set; } = null!;
}