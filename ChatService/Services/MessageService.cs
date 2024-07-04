using ChatService.Models.Entities;
using ChatService.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Models;

namespace ChatService.Services;

public class MessageService : IMessageService
{
    private readonly IMongoCollection<Message> _messagesCollection;

    public MessageService(
        IOptions<ChatAppDbOptions> chatAppDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            chatAppDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            chatAppDatabaseSettings.Value.DatabaseName);

        _messagesCollection = mongoDatabase.GetCollection<Message>("messages");
    }

    public async Task CreateAsync(Message newMessage) =>
        await _messagesCollection.InsertOneAsync(newMessage);
}