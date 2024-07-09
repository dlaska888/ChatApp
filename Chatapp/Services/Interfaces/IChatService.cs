using MongoDB.Bson;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IChatService
{
    Task CreateAsync(CreateMessageDto dto);
    Task<IEnumerable<GetChatDto>> GetAllChats(string userId);
    Task<IEnumerable<GetMessageDto>> GetMessagesByChat(string userId, string receiverId, ChatTypeEnum chatTypeEnum, string? earliestMessageId);
}