using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shared.Models;
using WebService.Models.Dtos;
using WebService.Models.Entities;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class MessageService : IMessageService
{
    private readonly IMongoCollection<Message> _messagesCollection;
    private readonly IMongoCollection<Group> _groupsCollection;
    private readonly IMongoCollection<ChatUser> _chatUsersCollection;

    public MessageService(
        IOptions<ChatAppDbOptions> chatAppDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            chatAppDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            chatAppDatabaseSettings.Value.DatabaseName);

        _messagesCollection = mongoDatabase.GetCollection<Message>("messages");
        _groupsCollection = mongoDatabase.GetCollection<Group>("groups");
        _chatUsersCollection = mongoDatabase.GetCollection<ChatUser>("chatUsers");
    }

    public async Task<IEnumerable<GetChatDto>> GetAllChats(ObjectId userId)
    {
        // LINQ query to get private chats
        var privateChats = await _messagesCollection.AsQueryable()
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => new { m.SenderId, m.ReceiverId })
            .Select(g => new
            {
                ChatNameId = g.Key.SenderId == userId ? g.Key.ReceiverId : g.Key.SenderId,
                g.Key.SenderId,
                g.Key.ReceiverId,
            })
            .ToListAsync();

        // Get all ChatNameIds
        var chatNameIds = privateChats.Select(pc => pc.ChatNameId).Distinct().ToList();

        // Retrieve all usernames in a single query
        var userNames = _chatUsersCollection.AsQueryable()
            .Where(user => chatNameIds.Contains(user.Id))
            .ToDictionary(user => user.Id, user => user.UserName);

        // Create chat DTOs for private chats
        var chats = privateChats.Select(m => new GetChatDto
        {
            Name = userNames[m.ChatNameId]!,
            ReceiverId = m.ChatNameId.ToString(),
            ChatTypeEnum = ChatTypeEnum.Private,
        }).ToList();

        // LINQ query to get group chats
        var groups = await _groupsCollection.AsQueryable()
            .Where(g => g.UserIds.Contains(userId))
            .ToListAsync();

        // Add group chats to the chat list
        chats.AddRange(groups.Select(g => new GetChatDto
        {
            Name = g.Name,
            ReceiverId = g.Id.ToString(),
            ChatTypeEnum = ChatTypeEnum.Group
        }));

        return chats;
    }

    public async Task<IEnumerable<GetMessageDto>> GetMessagesByChat(ObjectId senderId, ObjectId receiverId,
        ChatTypeEnum chatTypeEnum, ObjectId? earliestMessageId)

    {
        return chatTypeEnum switch
        {
            ChatTypeEnum.Private => await GetPrivateMessages(senderId, receiverId, earliestMessageId),
            ChatTypeEnum.Group => await GetGroupMessages(receiverId, earliestMessageId),
            _ => throw new ArgumentOutOfRangeException(nameof(chatTypeEnum), chatTypeEnum, null)
        };
    }
    
    public async Task CreateAsync(Message newMessage) =>
        await _messagesCollection.InsertOneAsync(newMessage);

    private async Task<IEnumerable<GetMessageDto>> GetPrivateMessages(ObjectId senderId, ObjectId receiverId,
        ObjectId? earliestMessageId)
    {
        return await _messagesCollection
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
            .ToListAsync();
    }

    private async Task<IEnumerable<GetMessageDto>> GetGroupMessages(ObjectId groupId, ObjectId? earliestMessageId)
    {
        return await _messagesCollection
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
            .ToListAsync();
    }
}