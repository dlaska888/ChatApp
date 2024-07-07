using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebService.Models.Entities
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public List<ObjectId> UserIds { get; set; } = new();
    }
}