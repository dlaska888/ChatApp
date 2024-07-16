using WebService.Models.Dtos.Messages;

namespace WebService.Services.Interfaces;

public interface INotificationProducerService
{
    Task SendMessageNotificationRequestAsync(CreateMessageDto dto);
}