using MongoDB.Bson;

namespace WebService.Models.Entities;

public class Friendship : Document
{
    public ObjectId User1Id { get; set; }
    public ObjectId User2Id { get; set; }
    public bool IsAccepted { get; set; }
}