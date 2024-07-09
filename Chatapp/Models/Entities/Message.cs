using MongoDB.Bson;
using WebService.Attributes;

namespace WebService.Models.Entities;

[BsonCollection("messages")]
public class Message : Document
{
    public ObjectId SenderId { get; set; }
    public ObjectId ReceiverId { get; set; }
    public string Content { get; set; } = null!;
}