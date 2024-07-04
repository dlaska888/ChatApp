using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace WebService.Models.Entities;

public class ChatUser : MongoIdentityUser<ObjectId>
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExp { get; set; }
}