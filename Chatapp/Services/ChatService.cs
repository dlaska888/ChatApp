using MongoDB.Bson;
using WebService.Exceptions;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class ChatService(
    IMongoRepository<ChatUser> userRepo,
    IMongoRepository<Message> messageRepo,
    IGroupService groupService) : IChatService
{
    public async Task CreateAsync(CreateMessageDto dto)
    {
        var message = new Message
        {
            SenderId = new ObjectId(dto.SenderId),
            ReceiverId = new ObjectId(dto.ReceiverId),
            Content = dto.Content,
        };

        await messageRepo.InsertOneAsync(message);
    }

    public async Task<IEnumerable<GetChatDto>> GetAllChatsAsync(string userId)
    {
        var userObjectId = new ObjectId(userId);
        var allChats = new List<GetChatDto>();

        var privateChats = await messageRepo.AsQueryable()
            .Where(m => m.SenderId == userObjectId || m.ReceiverId == userObjectId)
            .GroupBy(m => new { m.SenderId, m.ReceiverId })
            .Select(g => new
            {
                ChatNameId = g.Key.SenderId == userObjectId ? g.Key.ReceiverId : g.Key.SenderId,
                g.Key.SenderId,
                g.Key.ReceiverId,
            })
            .ToAsyncEnumerable()
            .ToListAsync();

        var chatNameIds = privateChats.Select(pc => pc.ChatNameId).Distinct().ToList();

        var userNames = userRepo
            .AsQueryable()
            .Where(user => chatNameIds.Contains(user.Id))
            .ToDictionary(user => user.Id, user => user.UserName);

        allChats.AddRange(privateChats.Select(m => new GetChatDto
        {
            Name = userNames[m.ChatNameId]!,
            ReceiverId = m.ChatNameId.ToString(),
            ChatTypeEnum = ChatTypeEnum.Private,
        }));

        var groups = await groupService.GetAllGroupsAsync(userId);

        allChats.AddRange(groups.Select(g => new GetChatDto
        {
            Name = g.Name,
            ReceiverId = g.Id.ToString(),
            ChatTypeEnum = ChatTypeEnum.Group
        }));

        return allChats;
    }

    public async Task<IEnumerable<GetMessageDto>> GetMessagesByChatAsync(string userId, string receiverId,
        ChatTypeEnum chatTypeEnum, string? earliestMessageId)

    {
        var userObjectId = new ObjectId(userId);
        var receiverObjectId = new ObjectId(receiverId);
        var earliestMessageObjectId = earliestMessageId != null ? new ObjectId(earliestMessageId) : (ObjectId?)null;

        return chatTypeEnum switch
        {
            ChatTypeEnum.Private => await GetPrivateMessagesAsync(userObjectId, receiverObjectId,
                earliestMessageObjectId),
            ChatTypeEnum.Group => await GetGroupMessagesAsync(userObjectId, receiverObjectId, earliestMessageObjectId),
            _ => throw new ArgumentOutOfRangeException(nameof(chatTypeEnum), chatTypeEnum, null)
        };
    }

    private async Task<IEnumerable<GetMessageDto>> GetPrivateMessagesAsync(ObjectId senderId, ObjectId receiverId,
        ObjectId? earliestMessageId)
    {
        return await messageRepo
            .AsQueryable()
            .Where(m =>
                (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == senderId))
            .Where(m => earliestMessageId == null || m.Id < earliestMessageId)
            .Select(m => new GetMessageDto
            {
                Id = m.Id.ToString(),
                SenderId = m.SenderId.ToString(),
                ReceiverId = m.ReceiverId.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToAsyncEnumerable()
            .ToListAsync();
    }

    private async Task<IEnumerable<GetMessageDto>> GetGroupMessagesAsync(ObjectId userId, ObjectId groupId,
        ObjectId? earliestMessageId)
    {
        if (!await groupService.UserHasAccessToGroup(userId.ToString(), groupId.ToString()))
            throw new UnauthorizedException("User does not have access to this group.");

        return await messageRepo
            .AsQueryable()
            .Where(m => m.ReceiverId == groupId)
            .Where(m => earliestMessageId == null || m.Id < earliestMessageId)
            .Select(m => new GetMessageDto
            {
                Id = m.Id.ToString(),
                SenderId = m.SenderId.ToString(),
                ReceiverId = m.ReceiverId.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToAsyncEnumerable()
            .ToListAsync();
    }
}