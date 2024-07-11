using MongoDB.Bson;
using WebService.Enums;
using WebService.Exceptions;
using WebService.Models.Dtos;
using WebService.Models.Dtos.Messages;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class ChatService(
    IMongoRepository<ChatUser> userRepo,
    IMongoRepository<PrivateMessage> privateMessageRepo,
    IMongoRepository<GroupMessage> groupMessageRepo,
    IGroupService groupService) : IChatService
{
    public async Task CreatePrivateMessageAsync(CreateMessageDto dto)
    {
        var message = new PrivateMessage
        {
            SenderId = new ObjectId(dto.SenderId),
            ReceiverId = new ObjectId(dto.ReceiverId),
            Content = dto.Content,
        };

        await privateMessageRepo.InsertOneAsync(message);
    }

    public async Task CreateGroupMessageAsync(CreateMessageDto dto)
    {
        var message = new GroupMessage()
        {
            SenderId = new ObjectId(dto.SenderId),
            GroupId = new ObjectId(dto.ReceiverId),
            Content = dto.Content,
        };

        await groupMessageRepo.InsertOneAsync(message);
    }

    public async Task<IEnumerable<GetChatDto>> GetAllChatsAsync(string userId)
    {
        var allChats = new List<GetChatDto>();

        var privateChats = await GetPrivateChatsAsync(userId);
        allChats.AddRange(privateChats);

        var groupChats = await GetGroupChatsAsync(userId);
        allChats.AddRange(groupChats);

        return allChats;
    }

    public async Task<IEnumerable<GetMessageDto>> GetPrivateMessagesAsync(string userId, string receiverId,
        string? earliestMessageId)
    {
        var userObjectId = new ObjectId(userId);
        var receiverObjectId = new ObjectId(receiverId);
        ObjectId.TryParse(earliestMessageId, out var earliestMessageObjectId);

        return await privateMessageRepo
            .AsQueryable()
            .Where(m =>
                (m.SenderId == userObjectId && m.ReceiverId == receiverObjectId) ||
                (m.SenderId == receiverObjectId && m.ReceiverId == userObjectId))
            .Where(m => earliestMessageId == null || m.Id < earliestMessageObjectId)
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

    public async Task<IEnumerable<GetMessageDto>> GetGroupMessagesAsync(string userId, string groupId,
        string? earliestMessageId)
    {
        if (!await groupService.UserHasAccessToGroup(userId, groupId))
            throw new UnauthorizedException("User does not have access to this group.");

        var groupObjectId = new ObjectId(groupId);
        ObjectId.TryParse(earliestMessageId, out var earliestMessageObjectId);

        return await groupMessageRepo
            .AsQueryable()
            .Where(m => m.GroupId == groupObjectId)
            .Where(m => earliestMessageId == null || m.Id < earliestMessageObjectId)
            .Select(m => new GetMessageDto
            {
                Id = m.Id.ToString(),
                SenderId = m.SenderId.ToString(),
                ReceiverId = m.GroupId.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToAsyncEnumerable()
            .ToListAsync();
    }

    private async Task<IEnumerable<GetChatDto>> GetPrivateChatsAsync(string userId)
    {
        var userObjectId = new ObjectId(userId);

        var privateChats = await privateMessageRepo
            .AsQueryable()
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

        return privateChats
            .Select(m => new GetChatDto
            {
                Name = userNames[m.ChatNameId]!,
                ReceiverId = m.ChatNameId.ToString(),
                ChatTypeEnum = ChatTypeEnum.Private
            })
            .ToList();
    }

    private async Task<IEnumerable<GetChatDto>> GetGroupChatsAsync(string userId)
    {
        var groups = await groupService.GetAllGroupsAsync(userId);

        return groups.Select(g => new GetChatDto
        {
            Name = g.Name,
            ReceiverId = g.Id.ToString(),
            ChatTypeEnum = ChatTypeEnum.Group
        }).ToList();
    }
}