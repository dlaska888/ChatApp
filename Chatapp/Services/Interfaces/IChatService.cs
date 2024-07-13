using MongoDB.Bson;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Dtos.Messages;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IChatService
{
    Task CreateAsync(CreateMessageDto dto);
    Task<IEnumerable<GetChatDto>> GetAllChatsAsync(string userId);
    Task<IEnumerable<GetMessageDto>> GetMessagesByChatAsync(string userId, string receiverId, ChatTypeEnum chatTypeEnum, string? earliestMessageId);
}