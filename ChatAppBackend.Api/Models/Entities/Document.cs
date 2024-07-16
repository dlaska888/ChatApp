using MongoDB.Bson;
using WebService.Models.Entities.Interfaces;

namespace WebService.Models.Entities;

public class Document : IDocument
{
    public ObjectId Id { get; set; }
    public DateTime CreatedAt => Id.CreationTime;
}