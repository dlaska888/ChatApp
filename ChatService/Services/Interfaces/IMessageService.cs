using ChatService.Models.Entities;

namespace ChatService.Services.Interfaces;

public interface IMessageService
{
    Task CreateAsync(Message newMessage);
}