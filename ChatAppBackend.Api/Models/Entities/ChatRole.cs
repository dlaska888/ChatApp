using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using WebService.Attributes;
using WebService.Models.Entities.Interfaces;

namespace WebService.Models.Entities;

[BsonCollection("chatRoles")]
public class ChatRole : MongoIdentityRole<ObjectId>, IDocument
{
    public DateTime CreatedAt => Id.CreationTime;
}