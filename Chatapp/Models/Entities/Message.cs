using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebService.Models.Entities;

public class Message
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonDateTimeOptions] public DateTime CreatedAt { get; set; }
    public ObjectId SenderId { get; set; }
    public ObjectId ReceiverId { get; set; }
    public string Content { get; set; } = null!;
}