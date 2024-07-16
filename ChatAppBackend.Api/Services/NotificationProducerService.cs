using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WebService.Models.Dtos.Messages;
using WebService.Models.Options;
using WebService.Models.Queues;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class NotificationProducerService : INotificationProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaOptions _options;

    public NotificationProducerService(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendMessageNotificationRequestAsync(CreateMessageDto dto)
    {
        var payload = new MessageNotificationRequest
        {
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content
        };

        var payloadString = JsonSerializer.Serialize(payload);
        var message = new Message<Null, string> { Value = payloadString };

        await _producer.ProduceAsync(_options.MessageNotificationTopic, message);
    }
}