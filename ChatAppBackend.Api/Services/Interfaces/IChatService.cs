using WebService.Models.Dtos;
using WebService.Models.Dtos.Messages;

namespace WebService.Services.Interfaces;

public interface IChatService
{
    Task<IEnumerable<GetChatDto>> GetAllChatsAsync(string userId);
    Task CreatePrivateMessageAsync(CreateMessageDto dto);
    Task CreateGroupMessageAsync(CreateMessageDto dto);
    Task<IEnumerable<GetMessageDto>> GetPrivateMessagesAsync(string userId, string receiverId, string? earliestMessageId);
    Task<IEnumerable<GetMessageDto>> GetGroupMessagesAsync(string userId, string groupId, string? earliestMessageId);
}