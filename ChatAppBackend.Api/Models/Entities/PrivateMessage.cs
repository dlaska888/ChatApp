using MongoDB.Bson;
using WebService.Attributes;

namespace WebService.Models.Entities;

[BsonCollection("privateMessages")]
public class PrivateMessage : Document
{
    public ObjectId SenderId { get; set; }
    public ObjectId ReceiverId { get; set; }
    public string Content { get; set; } = null!;
}