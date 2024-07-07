using MongoDB.Bson;
using Shared.Models;
using WebService.Models.Dtos;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces;

public interface IMessageService
{
    Task CreateAsync(Message newMessage);
    Task<IEnumerable<GetChatDto>> GetAllChats(ObjectId userId);
    Task<IEnumerable<GetMessageDto>> GetMessagesByChat(ObjectId senderId, ObjectId receiverId, ChatTypeEnum chatTypeEnum, ObjectId? earliestMessageId);
}