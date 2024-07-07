using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace WebService.Models.Entities;

public class ChatRole : MongoIdentityRole<ObjectId>
{
}