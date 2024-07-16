using MongoDB.Bson;
using WebService.Attributes;

namespace WebService.Models.Entities;

[BsonCollection("groupMessages")]
public class GroupMessage : Document
{
    public ObjectId SenderId { get; set; }
    public ObjectId GroupId { get; set; }
    public string Content { get; set; } = null!;
}