using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WebService.Models.Options;
using WebService.Models.Queues;

namespace WebService.Services;

public class NotificationConsumerService : BackgroundService
{
    private readonly ILogger<NotificationConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaOptions _options;

    public NotificationConsumerService(IOptions<KafkaOptions> options, ILogger<NotificationConsumerService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        _consumer.Subscribe(_options.MessageNotificationTopic);

        var i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeMessageNotificationRequest(stoppingToken);
            if (i++ % 1000 == 0)
            {
                _consumer.Commit();
            }
        }

        _consumer.Close();
    }

    private void ConsumeMessageNotificationRequest(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = _consumer.Consume(stoppingToken);
            var messageString = consumeResult.Message.Value;
            var message = JsonSerializer.Deserialize<MessageNotificationRequest>(messageString);

            _logger.LogInformation(
                $"Received message notification: Sender: {message.ReceiverId} Receiver: {message.SenderId} {message.Content}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing Kafka message: {ex.Message}");
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}