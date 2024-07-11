using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WebService.Models.Dtos;
using WebService.Models.Options;
using WebService.Models.Queues;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class NotificationProducerService : INotificationProducerService
{
    private readonly IOptions<QueueOptions> _options;

    private readonly IProducer<Null, MessageNotification> _producer;

    public NotificationProducerService(IOptions<QueueOptions> options)
    {
        _options = options;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.Value.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, MessageNotification>(producerConfig).Build();
    }

    public async Task SendMessageNotificationAsync(CreateMessageDto dto)
    {
        var payload = new MessageNotification
        {
            SenderId = dto.SenderId, 
            ReceiverId = dto.ReceiverId, 
            Content = dto.Content,
            CreatedAt = DateTime.Now
        };

        var message = new Message<Null, MessageNotification> { Value = payload };

        await _producer.ProduceAsync(dto.ReceiverId, message);
    }
}