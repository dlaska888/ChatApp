using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebService.Models.Entities.Interfaces;

public interface IDocument
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonDateTimeOptions] public DateTime CreatedAt { get; set; }
}