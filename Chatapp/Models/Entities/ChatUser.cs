using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using WebService.Attributes;
using WebService.Models.Entities.Interfaces;

namespace WebService.Models.Entities;

[BsonCollection("chatUsers")]
public class ChatUser : MongoIdentityUser<ObjectId>, IDocument
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExp { get; set; }
    
    public DateTime CreatedAt => Id.CreationTime;
}