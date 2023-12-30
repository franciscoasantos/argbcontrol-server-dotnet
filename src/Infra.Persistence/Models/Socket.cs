using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infra.Persistence.Models;

public class Socket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ObjectId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? OwnerId { get; set; }
    public string[]? Clients { get; set; }
}
