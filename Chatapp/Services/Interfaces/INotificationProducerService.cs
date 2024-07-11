using WebService.Models.Dtos;
using WebService.Models.Queues;

namespace WebService.Services.Interfaces;

public interface INotificationProducerService
{
    Task SendMessageNotificationAsync(CreateMessageDto dto);
}